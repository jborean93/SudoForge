using namespace System.IO

$moduleName = (Get-Item ([Path]::Combine($PSScriptRoot, '..', 'module', '*.psd1'))).BaseName
$global:ModuleManifest = [Path]::Combine($PSScriptRoot, '..', 'output', $moduleName)

if (-not (Get-Module -Name $moduleName -ErrorAction SilentlyContinue)) {
    Import-Module $ModuleManifest
}

if (-not (Get-Variable IsWindows -ErrorAction SilentlyContinue)) {
    # Running WinPS so guaranteed to be Windows.
    Set-Variable -Name IsWindows -Value $true -Scope Global
}
