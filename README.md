# PowerShell SudoForge

[![Test workflow](https://github.com/jborean93/SudoForge/workflows/Test%20SudoForge/badge.svg)](https://github.com/jborean93/SudoForge/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/jborean93/SudoForge/branch/main/graph/badge.svg?token=b51IOhpLfQ)](https://codecov.io/gh/jborean93/SudoForge)
[![PowerShell Gallery](https://img.shields.io/powershellgallery/dt/SudoForge.svg)](https://www.powershellgallery.com/packages/SudoForge)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/jborean93/SudoForge/blob/main/LICENSE)

PowerShell RemoteForge implementation for a sudo wrapped process.

See [SudoForge index](docs/en-US/SudoForge.md) for more details.

## Requirements

These cmdlets have the following requirements

* PowerShell v7.3 or newer
+ [RemoteForge](https://github.com/jborean93/RemoteForge)

## Examples

TODO

## Installing

The easiest way to install this module is through [PowerShellGet](https://docs.microsoft.com/en-us/powershell/gallery/overview).

You can install this module by running either of the following `Install-PSResource` or `Install-Module` command.

```powershell
# Install for only the current user
Install-PSResource -Name SudoForge -Scope CurrentUser
Install-Module -Name SudoForge -Scope CurrentUser

# Install for all users
Install-PSResource -Name SudoForge -Scope AllUsers
Install-Module -Name SudoForge -Scope AllUsers
```

The `Install-PSResource` cmdlet is part of the new `PSResourceGet` module from Microsoft available in newer versions while `Install-Module` is present on older systems.

## Contributing

Contributing is quite easy, fork this repo and submit a pull request with the changes.
To build this module run `.\build.ps1 -Task Build` in PowerShell.
To test a build run `.\build.ps1 -Task Test` in PowerShell.
This script will ensure all dependencies are installed before running the test suite.
