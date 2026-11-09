#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
& pwsh -NoProfile -File (Join-Path $PSScriptRoot '../../../scripts/Invoke-FrameworkTests.ps1') -Project tests/unit/Ifx.UnitTests/Ifx.UnitTests.csproj -TestSourceScope Generators/Implementation -Configuration Release -WarningsAsErrors
if ($LASTEXITCODE -ne 0) { throw 'Generator integration verification failed.' }
