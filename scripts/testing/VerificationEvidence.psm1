#Requires -Version 7.0
Set-StrictMode -Version Latest

function Get-IfxRelativeEvidenceAddress([string] $RepositoryRoot, [string] $Path) {
    $fullPath = [IO.Path]::GetFullPath($Path)
    $relative = [IO.Path]::GetRelativePath([IO.Path]::GetFullPath($RepositoryRoot), $fullPath).Replace('\', '/')
    if ($relative.StartsWith('../', [StringComparison]::Ordinal) -or [IO.Path]::IsPathRooted($relative)) {
        throw "Evidence artifact is outside the repository: $Path"
    }
    return $relative
}

function Write-IfxVerificationEvidence {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][string] $TestRunDirectory,
        [Parameter(Mandatory)][string] $BuildDirectory,
        [Parameter(Mandatory)][int] $ExitCode
    )

    $root = [IO.Path]::GetFullPath($RepositoryRoot)
    $runDirectory = [IO.Path]::GetFullPath($TestRunDirectory)
    $build = [IO.Path]::GetFullPath($BuildDirectory)
    $contextPath = Join-Path $runDirectory 'run-context.json'
    $identityPath = Join-Path $build 'build-identity.json'
    $completionPath = Join-Path $build 'build-complete.json'
    foreach ($path in @($contextPath, $identityPath, $completionPath)) {
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing verification evidence input: $path" }
    }

    $context = Get-Content -LiteralPath $contextPath -Raw | ConvertFrom-Json
    $identityHash = (Get-FileHash -LiteralPath $identityPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $identity = Get-Content -LiteralPath $identityPath -Raw | ConvertFrom-Json
    $completion = Get-Content -LiteralPath $completionPath -Raw | ConvertFrom-Json
    if (-not $context.noBuild -or $context.revision -cne $identity.revision -or
        $context.buildIdentity.sha256 -cne $identityHash -or $completion.buildIdentity.sha256 -cne $identityHash) {
        throw 'Test results are not bound to the captured source revision and build identity.'
    }
    if ($identityHash -cnotmatch '\A[0-9a-f]{64}\z' -or $identity.schemaVersion -ne 1) {
        throw 'Invalid captured build identity.'
    }

    $testResults = @(foreach ($file in Get-ChildItem -LiteralPath $runDirectory -Filter '*.trx' -File | Sort-Object Name) {
        [xml] $trx = Get-Content -LiteralPath $file.FullName -Raw
        $counters = $trx.SelectSingleNode('//*[local-name()="Counters"]')
        if ($null -eq $counters) { throw "Test result has no counters: $($file.FullName)" }
        $executed = [long] $counters.executed
        @{
            address = Get-IfxRelativeEvidenceAddress $root $file.FullName
            sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            total = [long] $counters.total; executed = $executed; passed = [long] $counters.passed
            failed = [long] $counters.failed; notExecuted = [long] $counters.notExecuted
        }
    })
    $failedTests = $testResults.Count -eq 0 -or @($testResults | Where-Object {
        $_.executed -eq 0 -or $_.failed -gt 0 -or $_.notExecuted -gt 0 -or $_.passed -ne $_.executed -or $_.total -ne $_.executed
    }).Count -gt 0
    $failed = $ExitCode -ne 0 -or $failedTests
    $toolPaths = @(
        '.github/workflows/publish.yml', '.infra/yaml/framework-quality/action.yml',
        'scripts/Invoke-FrameworkTests.ps1', 'scripts/testing/BuildProvenance.psm1',
        'scripts/testing/TestSourceSelection.psm1', 'scripts/testing/VerificationEvidence.psm1'
    )
    $tooling = @(foreach ($toolPath in $toolPaths) {
        $path = Join-Path $root $toolPath
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing verification tool input: $toolPath" }
        @{ address = $toolPath; sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() }
    })
    $contextEvidence = @{
        address = Get-IfxRelativeEvidenceAddress $root $contextPath
        sha256 = (Get-FileHash -LiteralPath $contextPath -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    $syndromes = @()
    if ($failed) {
        $runId = if ($env:GITHUB_RUN_ID) { $env:GITHUB_RUN_ID } else { 'local' }
        $attempt = if ($env:GITHUB_RUN_ATTEMPT) { $env:GITHUB_RUN_ATTEMPT } else { '1' }
        $references = @($testResults | ForEach-Object address)
        if ($references.Count -eq 0) { $references = @($contextEvidence.address) }
        $syndromes = @(@{
            id = "ifx-tests-$runId-$attempt"; code = 'IFX_TEST_VERIFICATION_FAILED'; severity = 'blocker'
            owner = 'Test Dev'; state = 'open'
            references = $references
        })
    }

    $evidence = [ordered]@{
        schemaVersion = 1
        repository = 'vc.Ifx'
        sourceRevision = $identity.revision
        configuration = $context.configuration
        selectionFingerprint = $context.testSelection.Fingerprint
        workflow = [ordered]@{ provider = $(if ($env:GITHUB_ACTIONS -eq 'true') { 'github-actions' } else { 'local' }); runId = $env:GITHUB_RUN_ID; runAttempt = $env:GITHUB_RUN_ATTEMPT }
        outcome = $(if ($failed) { 'failed' } else { 'passed' })
        build = [ordered]@{ address = Get-IfxRelativeEvidenceAddress $root $identityPath; revision = $identity.revision; sha256 = $identityHash }
        tooling = $tooling
        runContext = $contextEvidence
        testResults = $testResults
        syndromes = $syndromes
    }
    $evidencePath = Join-Path $runDirectory 'verification-evidence.json'
    if (Test-Path -LiteralPath $evidencePath) { throw 'Verification evidence already exists; use a fresh test run directory.' }
    $evidence | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $evidencePath -Encoding utf8
    return [pscustomobject]@{ path = $evidencePath; sha256 = (Get-FileHash -LiteralPath $evidencePath -Algorithm SHA256).Hash.ToLowerInvariant(); outcome = $evidence.outcome }
}

Export-ModuleMember -Function Write-IfxVerificationEvidence

