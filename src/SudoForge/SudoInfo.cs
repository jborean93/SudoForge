using RemoteForge;
using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SudoForge;

public sealed class SudoInfo : IRemoteForge
{
    public static string ForgeName => "sudo";
    public static string ForgeDescription => "Sudo wrapped PowerShell session";

    public PSCredential? Credential { get; set; }

    public static IRemoteForge Create(string info)
        => new SudoInfo();

    public IRemoteForgeTransport CreateTransport()
        => new SudoTransport(Credential);

    // public async Task StartTransport(
    //     ChannelReader<string> reader,
    //     ChannelWriter<string> writer,
    //     CancellationToken cancellationToken)
    // {
    //     using Process proc = Process.Start("");

    //     await proc.WaitForExitAsync();
    // }
}

public sealed class SudoTransport : IRemoteForgeTransport
{
    private readonly ChannelReader<(bool, string?)> _reader;
    private readonly ChannelWriter<(bool, string?)> _writer;
    private readonly Runspace? _runspace;
    private readonly string? _sudoUser;

    private Process? _proc;

    internal SudoTransport(
        PSCredential? credential)
    {
        Channel<(bool, string?)> channel = Channel.CreateUnbounded<(bool, string?)>();
        _reader = channel.Reader;
        _writer = channel.Writer;

        if (credential != null)
        {
            _sudoUser = credential.UserName;
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
            _runspace.SessionStateProxy.SetVariable("sudo", credential.Password);
        }
    }

    public Task CreateConnection(CancellationToken cancellationToken)
    {
        // On Linux the command line value has .dll, we need to remove that.
        string cmd = Environment.GetCommandLineArgs()[0];
        if (cmd.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            cmd = cmd[..(cmd.Length - 4)];
        }

        string askPassScript = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(typeof(SudoTransport).Assembly.Location)!,
            "..",
            "..",
            "ask_pass.ps1"));
        if (!File.Exists(askPassScript))
        {
            throw new FileNotFoundException($"Failed to find ask_pass.ps1 script at '{askPassScript}'");
        }

        _proc = new()
        {
            StartInfo = new()
            {
                FileName = "sudo",
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
            }
        };
        _proc.StartInfo.Environment.Add("SUDO_ASKPASS", askPassScript);
        if (_runspace != null)
        {
            _proc.StartInfo.Environment.Add("SUDOFORGE_RID", _runspace.Id.ToString());
        }

        if (_sudoUser != null && _sudoUser != "root")
        {
            _proc.StartInfo.ArgumentList.Add($"--user={_sudoUser}");
        }
        _proc.StartInfo.ArgumentList.Add("--askpass");
        _proc.StartInfo.ArgumentList.Add(cmd);
        _proc.StartInfo.ArgumentList.Add("-NoLogo");
        _proc.StartInfo.ArgumentList.Add("-ServerMode");
        _proc.Start();

        Task stdoutTask = Task.Run(async () => await PumpStream(_proc.StandardOutput, false));
        Task stderrTask = Task.Run(async () => await PumpStream(_proc.StandardError, true));
        Task.Run(async () =>
        {
            try
            {
                await _proc.WaitForExitAsync();
                await stdoutTask;
                await stderrTask;
            }
            finally
            {
                _writer.Complete();
            }
        });

        return Task.CompletedTask;
    }

    private async Task PumpStream(StreamReader reader, bool isError)
    {
        while (true)
        {
            string? msg = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(msg))
            {
                break;
            }

            await _writer.WriteAsync((isError, msg));
        }
    }

    public async Task CloseConnection(CancellationToken cancellationToken)
    {
        if (_proc != null)
        {
            _proc.Kill();
            await _reader.Completion;
        }
    }

    public async Task WriteMessage(string message, CancellationToken cancellationToken)
    {
        Debug.Assert(_proc != null);
        await _proc.StandardInput.WriteLineAsync(message.AsMemory(), cancellationToken);
    }

    public async Task<string?> WaitMessage(CancellationToken cancellationToken)
    {
        Debug.Assert(_proc != null);
        try
        {
            (bool isError, string? msg) = await _reader.ReadAsync(cancellationToken);
            if (isError)
            {
                throw new Exception(msg);
            }

            return msg;
        }
        catch (ChannelClosedException)
        {
            return null;
        }
    }

    public void Dispose()
    {
        _proc?.Dispose();
        _proc = null;
    }
}
