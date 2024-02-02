#!/usr/bin/env pwsh

using namespace System.Management.Automation.Runspaces
using namespace System.Net

# First arg is prompt from sudo itself.
param ([string]$Prompt)

$ErrorActionPreference = 'Stop'

# Parent is sudo, grandparent is the pwsh process
$parentPid = (Get-Process -Id $pid).Parent.Parent.Id

if ($env:SUDOFORGE_RID) {
    # If SUDOFORGE_RID is set the password is stored in the runspace specified
    $script = {
        $ErrorActionPreference = 'Stop'

        $rs = Get-Runspace -Id $args[0]
        if (-not $rs) {
            throw "Failed to find runspace $($args[0]) to retrieve sudo password"
        }
        $rs.SessionStateProxy.GetVariable("sudo")
    }

    $argument = $env:SUDOFORGE_RID
}
else {
    # If SUDOFORGE_RID isn't set we need to prompt for the password
    $script = {
        $ErrorActionPreference = 'Stop'

        $rs = Get-Runspace -Id 1
        $remoteHost = $rs.GetType().GetProperty(
            'Host',
            [System.Reflection.BindingFlags]'Instance, NonPublic'
        ).GetValue($rs)
        $remoteHost.UI.Write($args[0])
        $remoteHost.UI.ReadLineAsSecureString()
    }

    $argument = $Prompt
}

$ci = [NamedPipeConnectionInfo]::new($parentPid)
$rs = $ps = $null
try {
    $rs = [runspacefactory]::CreateRunspace($ci)
    $rs.Open()

    $ps = [powershell]::Create($rs)
    $null = $ps.AddScript($script.ToString()).AddArgument($argument)

    try {
        $output = $ps.Invoke()
    }
    catch {
        $host.UI.WriteErrorLine($_.Exception.InnerException.SerializedRemoteException.Message)
        exit 1
    }

    if ($output) {
        $host.UI.WriteLine([NetworkCredential]::new('', $output[0]).Password)
    }
}
finally {
    ${ps}?.Dispose()
    ${rs}?.Dispose()
}
