#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
# PackageSmoke.proj runs this entrypoint while holding the shared test mutex.
& (Join-Path $PSScriptRoot '../../../scripts/packaging/Test-CompressedPackages.ps1') -UnderFrameworkMutex
