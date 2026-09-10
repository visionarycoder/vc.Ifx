#Requires -Version 7.0
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$previous = $env:DirectoryBuildTargetsPath
try {
    $env:DirectoryBuildTargetsPath = Join-Path $PSScriptRoot 'GeneratorIntegration.targets'
    & pwsh -NoProfile -File (Join-Path $PSScriptRoot '../../../scripts/Invoke-FrameworkTests.ps1') -CoveragePackage vc.Ifx.Generators -TestSourceScope Generators/Implementation -TestPackage vc.Ifx.Generators -Filter FullyQualifiedName~Generators.Implementation -WarningsAsErrors
    if ($LASTEXITCODE -ne 0) { throw 'Generated endpoint/WebApi integration verification failed.' }
}
finally {
    $env:DirectoryBuildTargetsPath = $previous
}
