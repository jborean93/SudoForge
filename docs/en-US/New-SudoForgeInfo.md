---
external help file: SudoForge.dll-Help.xml
Module Name: SudoForge
online version: https://www.github.com/jborean93/SudoForge/blob/main/docs/en-US/New-SudoForgeInfo.md
schema: 2.0.0
---

# New-SudoForgeInfo

## SYNOPSIS
Create a connection info object for `sudo` connections.

## SYNTAX

```
New-SudoForgeInfo [[-Credential] <PSCredential>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Creates a connection info object to use with the [RemoteForge cmdlets](https://github.com/jborean93/RemoteForge) like `Invoke-Remote` and `Enter-Remote`.

## EXAMPLES

### Example 1: Create sudo connection info object to use
```powershell
PS C:\> Invoke-Remote (New-SudoForgeInfo -Credential root) { whoami }
```

Creates a connection info object that can start a new `pwsh` process under sudo.
This example will store the credential to use for the `sudo` password prompt.

## PARAMETERS

### -Credential
The credentials to use for the `sudo` invocation.
If no credential is specified, PowerShell will automatically prompt for a password through the `PSHost` implementation allowing this to work over an existing remote connection.

```yaml
Type: PSCredential
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
New common parameter introduced in PowerShell 7.4.

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### SudoForge.SudoInfo
The connection info for `sudo`.

## NOTES

## RELATED LINKS
