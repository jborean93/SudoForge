using System.Management.Automation;

namespace SudoForge;

[Cmdlet(VerbsCommon.New, "SudoForgeInfo")]
[OutputType(typeof(SudoInfo))]
public sealed class NewSudoForgeInfoCommand : PSCmdlet
{
    [Parameter(Position = 0)]
    [Credential]
    public PSCredential? Credential { get; set; }

    protected override void EndProcessing()
    {
        WriteObject(new SudoInfo() { Credential = Credential });
    }
}
