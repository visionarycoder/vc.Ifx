#Requires -Version 7.0
[CmdletBinding(DefaultParameterSetName = 'Package')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Package')][string] $Package,
    [Parameter(ParameterSetName = 'Package')][string] $Filter,
    [Parameter(ParameterSetName = 'Package')][string] $TestSourceScope,
    [Parameter(ParameterSetName = 'Package')][string] $TestPackage,
    [Parameter(Mandatory, ParameterSetName = 'Full')][switch] $Full,
    [ValidateSet('Debug', 'Release')][string] $Configuration = 'Debug',
    [switch] $NoBuild,
    [switch] $WarningsAsErrors,
    [switch] $ReportOnly
)

$arguments = @{ Configuration = $Configuration; NoBuild = $NoBuild; ReportOnly = $ReportOnly; WarningsAsErrors = $WarningsAsErrors }
if ($Full) { $arguments.FullCoverage = $true }
else {
    $arguments.CoveragePackage = $Package
    if ($Filter) { $arguments.Filter = $Filter }
    if ($TestSourceScope) { $arguments.TestSourceScope = $TestSourceScope }
    if ($TestPackage) { $arguments.TestPackage = $TestPackage }
}
& (Join-Path $PSScriptRoot '../Invoke-FrameworkTests.ps1') @arguments
