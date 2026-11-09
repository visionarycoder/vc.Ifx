#Requires -Version 7.0
[CmdletBinding()]
param([switch] $UnderFrameworkMutex)
# Retain the entrypoint while using the adopted compressed package inventory.
& (Join-Path $PSScriptRoot 'Test-CompressedPackages.ps1') -UnderFrameworkMutex:$UnderFrameworkMutex
