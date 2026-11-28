#Requires -Version 7.0
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
Import-Module (Join-Path $root 'scripts/testing/VerificationEvidence.psm1') -Force
$fixtureRoot = Join-Path $root "test-results/lsi-evidence/$([Guid]::NewGuid().ToString('N'))"
$build = Join-Path $fixtureRoot 'build'
$run = Join-Path $fixtureRoot 'run'
$null = New-Item -ItemType Directory -Path $build
$null = New-Item -ItemType Directory -Path $run
$script:checks = [Collections.Generic.List[string]]::new()

function Assert-True([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
    $script:checks.Add($Message)
}

function Assert-Rejected([string] $Name, [scriptblock] $Action, [string] $Pattern) {
    $message = $null
    try { & $Action | Out-Null } catch { $message = $_.Exception.Message }
    if (-not $message -or $message -notmatch $Pattern) { throw "$Name was not rejected: $message" }
    $script:checks.Add($Name)
}

$revision = 'a' * 40
$identityPath = Join-Path $build 'build-identity.json'
$completionPath = Join-Path $build 'build-complete.json'
$identity = [ordered]@{ schemaVersion = 1; revision = $revision; projects = @(@{ name = 'Fixture'; inputs = @(); outputs = @() }) }
$identity | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $identityPath -Encoding utf8
$identityHash = (Get-FileHash -LiteralPath $identityPath -Algorithm SHA256).Hash.ToLowerInvariant()
@{ buildIdentity = @{ sha256 = $identityHash } } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $completionPath -Encoding utf8
@{
    revision = $revision; configuration = 'Release'; noBuild = $true
    testSelection = @{ Fingerprint = '' }; buildIdentity = @{ path = $identityPath; sha256 = $identityHash }
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $run 'run-context.json') -Encoding utf8
$trxPath = Join-Path $run 'tests.trx'
'<TestRun><ResultSummary><Counters total="2" executed="2" passed="2" failed="0" notExecuted="0" /></ResultSummary></TestRun>' |
    Set-Content -LiteralPath $trxPath -Encoding utf8

$result = Write-IfxVerificationEvidence -RepositoryRoot $root -TestRunDirectory $run -BuildDirectory $build -ExitCode 0
$evidence = Get-Content -LiteralPath $result.path -Raw | ConvertFrom-Json
Assert-True ($evidence.outcome -eq 'passed' -and $evidence.sourceRevision -eq $revision) 'passed evidence pins the source revision'
Assert-True ($evidence.build.sha256 -eq $identityHash -and $evidence.testResults.Count -eq 1) 'passed evidence pins build and test artifacts'
Assert-True ($evidence.tooling.Count -eq 6 -and $evidence.tooling.sha256 -match '\A[0-9a-f]{64}\z') 'passed evidence pins the test and CI tooling'
$firstHash = $result.sha256
Remove-Item -LiteralPath $result.path
$repeat = Write-IfxVerificationEvidence -RepositoryRoot $root -TestRunDirectory $run -BuildDirectory $build -ExitCode 0
Assert-True ($repeat.sha256 -eq $firstHash) 'identical run inputs produce deterministic evidence'

$failedRun = Join-Path $fixtureRoot 'failed-run'
$null = New-Item -ItemType Directory -Path $failedRun
@{
    revision = $revision; configuration = 'Release'; noBuild = $true
    testSelection = @{ Fingerprint = '' }; buildIdentity = @{ path = $identityPath; sha256 = $identityHash }
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $failedRun 'run-context.json') -Encoding utf8
'<TestRun><ResultSummary><Counters total="2" executed="2" passed="1" failed="1" notExecuted="0" /></ResultSummary></TestRun>' |
    Set-Content -LiteralPath (Join-Path $failedRun 'tests.trx') -Encoding utf8
$failedResult = Write-IfxVerificationEvidence -RepositoryRoot $root -TestRunDirectory $failedRun -BuildDirectory $build -ExitCode 1
$failedEvidence = Get-Content -LiteralPath $failedResult.path -Raw | ConvertFrom-Json
Assert-True ($failedEvidence.outcome -eq 'failed' -and $failedEvidence.syndromes[0].code -eq 'IFX_TEST_VERIFICATION_FAILED') 'failed tests emit a blocker syndrome'
Assert-True ($failedEvidence.syndromes[0].references.Count -eq 1) 'blocker syndrome points to failed test evidence'

$abortedRun = Join-Path $fixtureRoot 'aborted-run'
$null = New-Item -ItemType Directory -Path $abortedRun
@{
    revision = $revision; configuration = 'Release'; noBuild = $true
    testSelection = @{ Fingerprint = '' }; buildIdentity = @{ path = $identityPath; sha256 = $identityHash }
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $abortedRun 'run-context.json') -Encoding utf8
'<TestRun><ResultSummary><Counters total="0" executed="0" passed="0" failed="0" notExecuted="0" /></ResultSummary></TestRun>' |
    Set-Content -LiteralPath (Join-Path $abortedRun 'aborted.trx') -Encoding utf8
$abortedResult = Write-IfxVerificationEvidence -RepositoryRoot $root -TestRunDirectory $abortedRun -BuildDirectory $build -ExitCode 1
$abortedEvidence = Get-Content -LiteralPath $abortedResult.path -Raw | ConvertFrom-Json
Assert-True ($abortedEvidence.outcome -eq 'failed' -and $abortedEvidence.syndromes.Count -eq 1) 'aborted zero-test run remains blocked with a syndrome'

$missingRun = Join-Path $fixtureRoot 'missing-run'
$null = New-Item -ItemType Directory -Path $missingRun
@{
    revision = $revision; configuration = 'Release'; noBuild = $true
    testSelection = @{ Fingerprint = '' }; buildIdentity = @{ path = $identityPath; sha256 = $identityHash }
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $missingRun 'run-context.json') -Encoding utf8
$missingResult = Write-IfxVerificationEvidence -RepositoryRoot $root -TestRunDirectory $missingRun -BuildDirectory $build -ExitCode 1
$missingEvidence = Get-Content -LiteralPath $missingResult.path -Raw | ConvertFrom-Json
Assert-True ($missingEvidence.syndromes[0].references[0] -eq 'test-results/lsi-evidence/' + (Split-Path $fixtureRoot -Leaf) + '/missing-run/run-context.json') 'missing test output syndrome references the run context'

$staleRun = Join-Path $fixtureRoot 'stale-run'
$null = New-Item -ItemType Directory -Path $staleRun
@{
    revision = ('b' * 40); configuration = 'Release'; noBuild = $true
    testSelection = @{ Fingerprint = '' }; buildIdentity = @{ path = $identityPath; sha256 = $identityHash }
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $staleRun 'run-context.json') -Encoding utf8
Copy-Item -LiteralPath $trxPath -Destination (Join-Path $staleRun 'tests.trx')
Assert-Rejected 'stale source revision' {
    Write-IfxVerificationEvidence -RepositoryRoot $root -TestRunDirectory $staleRun -BuildDirectory $build -ExitCode 0
} 'not bound'

$tamperedRun = Join-Path $fixtureRoot 'tampered-run'
$null = New-Item -ItemType Directory -Path $tamperedRun
@{
    revision = $revision; configuration = 'Release'; noBuild = $true
    testSelection = @{ Fingerprint = '' }; buildIdentity = @{ path = $identityPath; sha256 = ('0' * 64) }
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $tamperedRun 'run-context.json') -Encoding utf8
Copy-Item -LiteralPath $trxPath -Destination (Join-Path $tamperedRun 'tests.trx')
Assert-Rejected 'changed build identity hash' {
    Write-IfxVerificationEvidence -RepositoryRoot $root -TestRunDirectory $tamperedRun -BuildDirectory $build -ExitCode 0
} 'not bound'

@{ passed = $script:checks.Count; checks = @($script:checks) } |
    ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $fixtureRoot 'results.json') -Encoding utf8
Write-Host "PASS: $($script:checks.Count) LSI verification evidence checks. Artifacts: $fixtureRoot"

