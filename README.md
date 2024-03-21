# PowerShell SudoForge

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/jborean93/SudoForge/blob/main/LICENSE)

PowerShell RemoteForge implementation for a sudo wrapped process.

See [SudoForge index](docs/en-US/SudoForge.md) for more details.

## Requirements

These cmdlets have the following requirements

* PowerShell v7.3 or newer
+ [RemoteForge](https://github.com/jborean93/RemoteForge)

## Examples

The follow example runs the scriptblock specified under a new PowerShell process under `sudo`.

```powershell
Invoke-Remote sudo: { whoami }
```

The next example shows how a custom credential can be specified for non-interactive purposes.
How the credential is created can be done through various means like through a `SecretStore`.

```powershell
# Prompts for the password in this example
$sudoInfo = New-SudoForgeInfo -Credential root

Invoke-Remote $sudoInfo { whoami }
```

## Installing

This module is not available in the gallery at this point in time as it is more a POC for `RemoteForge`.

## Contributing

Contributing is quite easy, fork this repo and submit a pull request with the changes.
To build this module run `.\build.ps1 -Task Build` in PowerShell.
To test a build run `.\build.ps1 -Task Test` in PowerShell.
This script will ensure all dependencies are installed before running the test suite.
