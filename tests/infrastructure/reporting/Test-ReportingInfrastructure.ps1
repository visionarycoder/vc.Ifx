#Requires -Version 7.0
[CmdletBinding()]
param([ValidateSet('Debug', 'Release')][string] $Configuration = 'Debug')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$fixtureRoot = Join-Path $root "TestResults/reporting-host-tests/$([Guid]::NewGuid().ToString('N'))"
$runner = Join-Path $root 'scripts/reporting/Invoke-FrameworkReport.ps1'
$package = 'vc.Ifx.Roslyn.Reporting'
$script:passed = 0

function New-Fixture([string] $Name) {
    $build = Join-Path $fixtureRoot "$Name/build"
    $directory = Join-Path $build $package
    $run = Join-Path $fixtureRoot "$Name/coverage"
    $null = New-Item -ItemType Directory -Path $directory, $run -Force
    $source = Join-Path $build 'Fixture.cs'
    'class C { int M() => 1; }' | Set-Content -LiteralPath $source -Encoding utf8
    "$((Get-FileHash -LiteralPath $source).Hash)|$source" | Set-Content -LiteralPath (Join-Path $directory 'sources.txt') -Encoding utf8
    'NET10_0' | Set-Content -LiteralPath (Join-Path $directory 'symbols.txt') -Encoding utf8
    '{"version":"2.1.0","runs":[{"tool":{"driver":{"name":"fixture"}},"results":[]}]}' |
        Set-Content -LiteralPath (Join-Path $directory 'diagnostics.sarif') -Encoding utf8
    @{ revision = 'fixture'; configuration = $Configuration; project = 'fixture'; testSourceScope = '' } |
        ConvertTo-Json | Set-Content -LiteralPath (Join-Path $build 'build-complete.json') -Encoding utf8
    @{ revision = 'fixture'; configuration = $Configuration; full = $false; filter = ''; testSourceScope = ''; testPackage = ''; reportBuildDirectory = $build } |
        ConvertTo-Json | Set-Content -LiteralPath (Join-Path $run 'run-context.json') -Encoding utf8
    $coverage = Join-Path $run 'coverage.json'
    @{ "$package.dll" = @{ file = @{ type = @{ method = @{ Lines = @{ '1' = 1 }; Branches = @() } } } } } |
        ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $coverage -Encoding utf8
    @{ coverageFile = $coverage } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $run 'summary.json') -Encoding utf8
    return @{ BuildDirectory = $build; CoverageRunDirectory = $run; Revision = 'fixture'; Package = @($package); Configuration = $Configuration }
}

function Test-Invocation([string] $Name, [hashtable] $Arguments, [string] $ExpectedFailure = '') {
    $failure = ''
    try { & $runner @Arguments }
    catch { $failure = $_.Exception.Message }
    if ($ExpectedFailure) {
        if (-not $failure.Contains($ExpectedFailure, [StringComparison]::Ordinal)) { throw "${Name}: expected '$ExpectedFailure', got '$failure'." }
    }
    elseif ($failure) { throw "${Name}: $failure" }
    $script:passed++
    Write-Host "PASS: $Name"
}

$complete = New-Fixture 'complete'
Test-Invocation 'complete host' $complete
$report = Get-Content (Join-Path $complete.BuildDirectory 'report-v1/report.json') -Raw | ConvertFrom-Json
if (-not $report.passed -or $report.scope -ne 'scoped' -or $report.packages[0].methods[0].cyclomaticComplexity -ne 1) { throw 'Host report facts differ from fixture.' }
Test-Invocation 'no overwrite' $complete 'already exists'
$mismatch = New-Fixture 'revision'
$mismatch.Revision = 'wrong'
Test-Invocation 'revision mismatch' $mismatch 'revisions must match'
$unknown = New-Fixture 'unknown'
$unknown.Package = @('vc.Ifx.Unknown')
Test-Invocation 'unknown package' $unknown 'Unknown package'
$full = New-Fixture 'full'
$full.Remove('Package')
Test-Invocation 'scoped is not full evidence' $full 'unfiltered full coverage'
$stale = New-Fixture 'stale'
'// changed' | Add-Content -LiteralPath (Join-Path $stale.BuildDirectory 'Fixture.cs') -Encoding utf8
Test-Invocation 'source fingerprint mismatch' $stale 'Source changed since compilation'
$escaped = New-Fixture 'escaped'
@{ coverageFile = (Join-Path $fixtureRoot 'other.json') } | ConvertTo-Json |
    Set-Content -LiteralPath (Join-Path $escaped.CoverageRunDirectory 'summary.json') -Encoding utf8
Test-Invocation 'coverage outside current run' $escaped 'outside this run'
$partial = New-Fixture 'partial'
@{ coverageFile = (Join-Path $partial.CoverageRunDirectory 'coverage.json') } | ConvertTo-Json |
    Set-Content -LiteralPath (Join-Path $partial.CoverageRunDirectory 'summary.json') -Encoding utf8
'{}' | Set-Content -LiteralPath (Join-Path $partial.CoverageRunDirectory 'coverage.json') -Encoding utf8
Test-Invocation 'partial coverage reports failure' $partial 'Reporting failed'
$failedReport = Get-Content (Join-Path $partial.BuildDirectory 'report-v1/report.json') -Raw | ConvertFrom-Json
if ($failedReport.passed -or $null -ne $failedReport.packages[0].coverage) { throw 'Partial coverage was fabricated.' }
Write-Host "$script:passed reporting host checks passed. Synthetic fixtures only: $fixtureRoot"
# Expected negative subprocess tests must not fail GitHub's pwsh exit-code epilogue.
# This is reached only after every assertion passes; unexpected failures still throw.
$global:LASTEXITCODE = 0
