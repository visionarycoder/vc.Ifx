#Requires -Version 7.0
[CmdletBinding(DefaultParameterSetName = 'Test')]
param(
    [Parameter(ParameterSetName = 'Test')]
    [Parameter(ParameterSetName = 'Build')]
    [string] $Project = 'tests/unit/vc.Ifx.UnitTests/vc.Ifx.UnitTests.csproj',
    [Parameter(ParameterSetName = 'Test')]
    [Parameter(ParameterSetName = 'Coverage')][string] $Filter,
    [Parameter(ParameterSetName = 'Test')]
    [Parameter(ParameterSetName = 'Build')]
    [Parameter(ParameterSetName = 'Coverage')]
    [string] $TestSourceScope,
    [Parameter(ParameterSetName = 'Test')]
    [Parameter(ParameterSetName = 'Build')]
    [Parameter(ParameterSetName = 'Coverage')]
    [ValidatePattern('^vc\.Ifx(?:\.[A-Za-z0-9]+)*$')][string] $TestPackage,
    [Parameter(Mandatory, ParameterSetName = 'Coverage')]
    [Alias('Package')][string] $CoveragePackage,
    [Parameter(Mandatory, ParameterSetName = 'FullCoverage')][switch] $FullCoverage,
    [Parameter(Mandatory, ParameterSetName = 'Build')][switch] $BuildOnly,
    [Parameter(ParameterSetName = 'Build')][string] $ReportBuildDirectory,
    [Parameter(ParameterSetName = 'Test')]
    [Parameter(ParameterSetName = 'Coverage')]
    [Parameter(ParameterSetName = 'FullCoverage')][switch] $NoBuild,
    [Parameter(ParameterSetName = 'Test')]
    [Parameter(ParameterSetName = 'Build')][switch] $NoRestore,
    [Parameter(ParameterSetName = 'Coverage')]
    [Parameter(ParameterSetName = 'FullCoverage')][switch] $ReportOnly,
    [ValidateSet('Debug', 'Release')][string] $Configuration = 'Debug',
    [switch] $WarningsAsErrors,
    [ValidateRange(1, 3600)][int] $LockTimeoutSeconds = 1200
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Import-Module (Join-Path $PSScriptRoot 'testing/TestSourceSelection.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'testing/BuildProvenance.psm1') -Force
if ($TestPackage -and -not $TestSourceScope) { throw 'TestPackage requires TestSourceScope; full tests retain all references.' }
if ($CoveragePackage -and $TestPackage -and $CoveragePackage -cne $TestPackage) { throw 'CoveragePackage must match the selected TestPackage.' }
$identity = if ($IsWindows) { $repoRoot.ToUpperInvariant() } else { $repoRoot }
$digest = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($identity)))
$mutex = [Threading.Mutex]::new($false, "vc.Ifx.FrameworkTests.$digest")
$acquired = $false
try {
    Write-Host 'Waiting for exclusive framework build/test access...'
    try { $acquired = $mutex.WaitOne([TimeSpan]::FromSeconds($LockTimeoutSeconds)) }
    catch [Threading.AbandonedMutexException] {
        $acquired = $true
        throw 'Previous worker abandoned the build/test mutex. Rebuild affected projects before retrying coverage.'
    }
    if (-not $acquired) { throw 'Timed out waiting for another framework build/test operation.' }
    Write-Host 'Exclusive framework build/test access acquired.'
    if ($PSCmdlet.ParameterSetName -in @('Coverage', 'FullCoverage')) {
        $coverageArguments = @{ Configuration = $Configuration; NoBuild = $NoBuild; ReportOnly = $ReportOnly; WarningsAsErrors = $WarningsAsErrors }
        if ($TestSourceScope) { $coverageArguments.TestSourceScope = $TestSourceScope }
        if ($TestPackage) { $coverageArguments.TestPackage = $TestPackage }
        if ($FullCoverage) { $coverageArguments.Full = $true }
        else {
            $coverageArguments.Package = $CoveragePackage
            if ($Filter) { $coverageArguments.Filter = $Filter }
        }
        & (Join-Path $PSScriptRoot 'coverage/Invoke-CoverageCore.ps1') @coverageArguments
    }
    else {
        $projectPath = [IO.Path]::GetFullPath((Join-Path $repoRoot $Project))
        $selection = Resolve-IfxTestSelection -ProjectPath $projectPath -SourceScope $TestSourceScope -TestPackage $TestPackage
        $TestSourceScope = $selection.SourceScope
        $verb = if ($BuildOnly) { 'build' } else { 'test' }
        $arguments = @($verb, $projectPath, '-c', $Configuration, '-m:1', '-p:BuildInParallel=false', '-p:CollectCoverage=false', '-p:GeneratePackageOnBuild=false',
            '-v:minimal') + $selection.Arguments
        if ($TestSourceScope) {
            Write-Host "SCOPED TEST SOURCES: $TestSourceScope; this run is not full-suite evidence."
        }
        if ($NoRestore) { $arguments += '--no-restore' }
        if ($ReportBuildDirectory) {
            $ReportBuildDirectory = [IO.Path]::GetFullPath($ReportBuildDirectory)
            if (Test-Path -LiteralPath $ReportBuildDirectory) { throw 'Reporting requires a fresh build directory.' }
            $arguments += @('--no-incremental', "-p:IfxReportDirectory=$($ReportBuildDirectory.Replace('\', '/'))")
        }
        if ($WarningsAsErrors) { $arguments += '-warnaserror' }
        if ($NoBuild) { $arguments += '--no-build' }
        $buildIdentity = $null
        if ($NoBuild) {
            if (-not $env:IFX_REPORT_BUILD) { throw 'NoBuild requires IFX_REPORT_BUILD pointing to a captured fresh build.' }
            $buildIdentity = Assert-IfxBuildProvenance -BuildDirectory $env:IFX_REPORT_BUILD -RequiredProject $projectPath -Configuration $Configuration -Selection $selection
        }
        if ($Filter) { $arguments += @('--filter', $Filter) }
        if (-not $BuildOnly) {
            $results = Join-Path $repoRoot "TestResults/tests/$([IO.Path]::GetFileNameWithoutExtension($projectPath))/$([Guid]::NewGuid().ToString('N'))"
            $arguments += @('--logger', 'trx;LogFileName=tests.trx', '--results-directory', $results,
                '--blame-hang', '--blame-hang-timeout', '2m', '--blame-hang-dump-type', 'none')
            Write-Host "Test artifacts: $results"
            $null = New-Item -ItemType Directory -Path $results -Force
            @{ testSourceScope = $TestSourceScope; testSelection = $selection; testPackage = $TestPackage; filter = $Filter; configuration = $Configuration; noBuild = [bool] $NoBuild;
                buildIdentity = $(if ($buildIdentity) { @{ path = $buildIdentity.path; sha256 = $buildIdentity.sha256 } } else { $null }) } |
                ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $results 'run-context.json') -Encoding utf8
            if ($buildIdentity) { Copy-Item -LiteralPath $buildIdentity.path -Destination (Join-Path $results 'consumed-build-identity.json') }
        }
        & dotnet @arguments
        $testExitCode = $LASTEXITCODE
        if (-not $BuildOnly -and (Test-Path -LiteralPath (Join-Path $results 'tests.trx'))) {
            [xml] $trx = Get-Content -LiteralPath (Join-Path $results 'tests.trx') -Raw
            $counters = $trx.SelectSingleNode('//*[local-name()="Counters"]')
            if ($null -eq $counters -or [long] $counters.executed -eq 0) { throw 'Zero tests executed.' }
            Write-Host "Recorded test results: total=$($counters.total), executed=$($counters.executed), passed=$($counters.passed), failed=$($counters.failed), notExecuted=$($counters.notExecuted)"
            $trx.SelectNodes('//*[local-name()="UnitTestResult"][@outcome="Failed"]') | ForEach-Object {
                Write-Host "FAILED: $($_.testName)"
            }
        }
        elseif (-not $BuildOnly -and $testExitCode -eq 0) { throw 'Missing test results; zero-test success is not accepted.' }
        if ($testExitCode -ne 0) { throw "Framework $verb failed (exit $testExitCode)." }
        if ($NoBuild) { $null = Assert-IfxBuildProvenance -BuildDirectory $env:IFX_REPORT_BUILD -RequiredProject $projectPath -Configuration $Configuration -Selection $selection }
        if ($ReportBuildDirectory) {
            $revision = & git -C $repoRoot rev-parse HEAD
            if ($LASTEXITCODE -ne 0) { throw 'Cannot identify build revision.' }
            $buildIdentity = Complete-IfxBuildProvenance -BuildDirectory $ReportBuildDirectory -RepositoryRoot $repoRoot -Revision $revision -Configuration $Configuration -Selection $selection
            @{ revision = $revision; configuration = $Configuration; project = $Project; testSourceScope = $TestSourceScope; testPackage = $TestPackage; testSelection = $selection; buildIdentity = $buildIdentity } |
                ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $ReportBuildDirectory 'build-complete.json') -Encoding utf8
        }
    }
}
finally {
    if ($acquired) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}
