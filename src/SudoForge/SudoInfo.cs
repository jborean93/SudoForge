using RemoteForge;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SudoForge;

public sealed class SudoInfo : IRemoteForge
{
    public static string ForgeName => "sudo";
    public static string ForgeDescription => "Sudo wrapped PowerShell session";

    public PSCredential? Credential { get; set; }

    public static IRemoteForge Create(string info)
        => new SudoInfo();

    public RemoteTransport CreateTransport()
    {
        string askPassScript = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(typeof(SudoTransport).Assembly.Location)!,
            "..",
            "..",
            "ask_pass.ps1"));
        if (!File.Exists(askPassScript))
        {
            throw new FileNotFoundException($"Failed to find ask_pass.ps1 script at '{askPassScript}'");
        }

        List<string> sudoArgs = new();
        if (Credential != null && !string.Equals(Credential.UserName, "root", StringComparison.OrdinalIgnoreCase))
        {
            sudoArgs.Add($"--user={Credential.UserName}");
        }

        sudoArgs.AddRange(new[]
        {
            "--askpass",
            Environment.ProcessPath!,
            "-NoLogo",
            "-ServerMode"
        });

        Dictionary<string, string> envVars = new()
        {
            { "SUDO_ASKPASS", askPassScript },
            { "SUDOFORGE_PID", Environment.ProcessId.ToString() },
        };

        return new SudoTransport("sudo", sudoArgs, envVars, Credential?.GetNetworkCredential()?.Password);
    }
}

public sealed class SudoTransport : ProcessTransport
{
    private Runspace? _runspace;

    internal SudoTransport(
        string executable,
        IEnumerable<string> arguments,
        Dictionary<string, string> environment,
        string? password) : base(executable, arguments, environment)

    {
        if (password != null)
        {
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
            _runspace.SessionStateProxy.SetVariable("sudo", password);
            Proc.StartInfo.Environment.Add("SUDOFORGE_RID", _runspace.Id.ToString());
        }
    }

    protected override async Task<string?> ReadOutput(CancellationToken cancellationToken)
    {
        string? msg = await base.ReadOutput(cancellationToken);

        // Once we receive a message we don't need to keep the password in
        // the Runspace state anymore.
        if (_runspace != null)
        {
            _runspace.Dispose();
            _runspace = null;
        }

        return msg;
    }

    protected override Task Close(CancellationToken cancellationToken)
    {
        // .NET Kill() method send SIGKILL but sudo cannot handle that
        // and pass it along to its children. It can handle SIGTERM but .NET
        // doesn't expose that so we need to use some PInvoke to tell the pwsh
        // subprocess to exit.
        Libc.kill(Proc.Id, Libc.SIGTERM);

        // The base method should still be called after sending SIGTERM so that
        // the normal cleanup behaviour is done.
        return base.Close(cancellationToken);
    }

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing && _runspace != null)
        {
            _runspace.Dispose();
        }
        base.Dispose(isDisposing);
    }
}

internal partial class Libc
{
    public const int SIGTERM = 15;

    [LibraryImport("libc", SetLastError = true)]
    public static partial int kill(int pid, int sig);
}
