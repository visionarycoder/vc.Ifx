#Requires -Version 7.0
[CmdletBinding(DefaultParameterSetName = 'Package')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Package')]
    [ValidatePattern('^vc\.Ifx(?:\.[A-Za-z0-9]+)*$')][string] $Package,
    [Parameter(ParameterSetName = 'Package')][string] $Filter,
    [Parameter(ParameterSetName = 'Package')][string] $TestSourceScope,
    [Parameter(ParameterSetName = 'Package')][string] $TestPackage,
    [Parameter(Mandatory, ParameterSetName = 'Full')][switch] $Full,
    [ValidateSet('Debug', 'Release')][string] $Configuration = 'Debug',
    [switch] $NoBuild,
    [switch] $WarningsAsErrors,
    [switch] $ReportOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
Import-Module (Join-Path $repoRoot 'scripts/testing/TestSourceSelection.psm1') -Force
Import-Module (Join-Path $repoRoot 'scripts/testing/BuildProvenance.psm1') -Force
$sourceProjects = @(Get-ChildItem -LiteralPath (Join-Path $repoRoot 'src') -Recurse -Filter '*.csproj' | Sort-Object FullName)
$testProjects = @(Get-ChildItem -LiteralPath (Join-Path $repoRoot 'tests') -Recurse -Filter '*.csproj' | Sort-Object FullName -Descending)
if ($sourceProjects.Count -eq 0 -or $testProjects.Count -ne 2) {
    throw 'Expected source projects and exactly two centralized test projects. Update this gate explicitly when adding projects.'
}
[xml] $solution = Get-Content -LiteralPath (Join-Path $repoRoot 'vc.Ifx.slnx') -Raw
$solutionPaths = @($solution.SelectNodes('//Project') | ForEach-Object { [IO.Path]::GetFullPath((Join-Path $repoRoot $_.Path)) })
foreach ($project in @($sourceProjects) + @($testProjects)) {
    if ($project.FullName -notin $solutionPaths) { throw "Project absent from solution: $($project.FullName)" }
}
foreach ($testProject in $testProjects) {
    [xml] $projectXml = Get-Content -LiteralPath $testProject.FullName -Raw
    $referencePaths = @($projectXml.SelectNodes('//ProjectReference') | ForEach-Object {
        [IO.Path]::GetFullPath((Join-Path $testProject.DirectoryName $_.Include))
    })
    foreach ($source in $sourceProjects) {
        if ($source.FullName -notin $referencePaths) { throw "$($testProject.Name) is missing a direct reference to $($source.Name)." }
    }
}
$expected = @($sourceProjects.BaseName)
if (-not $Full) {
    if ($Package -cnotin $expected) { throw "Unknown package: $Package" }
    $expected = @($Package)
    $testProjects = @($testProjects | Where-Object BaseName -eq 'vc.Ifx.UnitTests')
}
$selection = Resolve-IfxTestSelection -ProjectPath $testProjects[0].FullName -SourceScope $TestSourceScope -TestPackage $TestPackage
$TestSourceScope = $selection.SourceScope
if ($Package -and $TestPackage -and $Package -cne $TestPackage) { throw 'Coverage package must match TestPackage.' }
$buildIdentity = $null
if ($NoBuild) {
    if (-not $env:IFX_REPORT_BUILD) { throw 'NoBuild requires IFX_REPORT_BUILD pointing to a captured fresh build.' }
    $buildIdentity = Assert-IfxBuildProvenance -BuildDirectory $env:IFX_REPORT_BUILD -RequiredProject @($testProjects.FullName) -Configuration $Configuration -Selection $selection
}

# All coverage invocations share build outputs; refuse concurrent instrumentation in this checkout.
$resultsRoot = Join-Path $repoRoot 'TestResults/coverage'
$null = New-Item -ItemType Directory -Path $resultsRoot -Force
$lockPath = Join-Path $resultsRoot 'coverage.lock'
try { $coverageLock = [IO.File]::Open($lockPath, 'OpenOrCreate', 'ReadWrite', 'None') }
catch { throw 'Another coverage run owns this checkout. Wait for it to finish.' }
try {
    $scope = if ($Full) { 'full' } else { $Package }
    $runRoot = Join-Path $resultsRoot "$scope/$([Guid]::NewGuid().ToString('N'))"
    $null = New-Item -ItemType Directory -Path $runRoot -Force
    Write-Host "Coverage artifacts: $runRoot"
    $revision = & git -C $repoRoot rev-parse HEAD
    if ($LASTEXITCODE -ne 0) { throw 'Cannot identify coverage revision.' }
    @{ revision = $revision; reportBuildDirectory = $env:IFX_REPORT_BUILD; testSourceScope = $TestSourceScope; testSelection = $selection; testPackage = $TestPackage; filter = $Filter; configuration = $Configuration; noBuild = [bool] $NoBuild; full = [bool] $Full;
        buildIdentity = $(if ($buildIdentity) { @{ path = $buildIdentity.path; sha256 = $buildIdentity.sha256 } } else { $null }) } |
        ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runRoot 'run-context.json') -Encoding utf8
    if ($buildIdentity) { Copy-Item -LiteralPath $buildIdentity.path -Destination (Join-Path $runRoot 'consumed-build-identity.json') }
    if ($env:GITHUB_ENV -and $Full -and -not $ReportOnly) {
        "IFX_COVERAGE_RUN=$runRoot" | Add-Content -LiteralPath $env:GITHUB_ENV -Encoding utf8
    }
    $previousJson = $null
    foreach ($project in $testProjects) {
        if ($NoBuild) { $null = Assert-IfxBuildProvenance -BuildDirectory $env:IFX_REPORT_BUILD -RequiredProject $project.FullName -Configuration $Configuration -Selection $selection }
        $output = Join-Path $runRoot $project.BaseName
        $null = New-Item -ItemType Directory -Path $output -Force
        $arguments = @('test', $project.FullName, '-c', $Configuration, '-m:1', '-p:BuildInParallel=false',
            '-p:CollectCoverage=true', '-p:Threshold=0', '-p:GeneratePackageOnBuild=false', "-p:CoverletOutput=$($output.Replace('\', '/'))/",
            '--logger', 'trx;LogFileName=tests.trx', '--results-directory', $output, '-v:minimal',
            '--blame-hang', '--blame-hang-timeout', '2m', '--blame-hang-dump-type', 'none') + $selection.Arguments
        if ($NoBuild) { $arguments += '--no-build' }
        if ($WarningsAsErrors) { $arguments += '-warnaserror' }
        if ($TestSourceScope) {
            Write-Host "SCOPED TEST SOURCES: $TestSourceScope; this run is not full-suite evidence."
        }
        if (-not $Full) { $arguments += "-p:IfxCoveragePackage=$Package" }
        if ($Filter) { $arguments += @('--filter', $Filter) }
        if ($previousJson) { $arguments += "-p:MergeWith=$previousJson" }
        & dotnet @arguments
        if ($LASTEXITCODE -ne 0) { throw "Tests or collection failed for $($project.Name) (exit $LASTEXITCODE)." }
        [xml] $trx = Get-Content -LiteralPath (Join-Path $output 'tests.trx') -Raw
        $counters = $trx.SelectSingleNode('//*[local-name()="Counters"]')
        if ($null -eq $counters -or [long] $counters.executed -eq 0 -or [long] $counters.passed -eq 0) {
            throw "No tests executed successfully for $($project.Name)."
        }
        $previousJson = Join-Path $output 'coverage.json'
        if (-not (Test-Path -LiteralPath $previousJson)) { throw "Missing Coverlet JSON for $($project.Name)." }
        if (-not (Test-Path -LiteralPath (Join-Path $output 'coverage.opencover.xml'))) { throw 'Missing OpenCover report.' }
    }
    if ($NoBuild) {
        $null = Assert-IfxBuildProvenance -BuildDirectory $env:IFX_REPORT_BUILD -RequiredProject @($testProjects.FullName) -Configuration $Configuration -Selection $selection
        @{ sha256 = $buildIdentity.sha256; restoredOutputsVerified = $true } | ConvertTo-Json |
            Set-Content -LiteralPath (Join-Path $runRoot 'build-identity-verified.json') -Encoding utf8
    }
    & (Join-Path $PSScriptRoot 'Test-CoverageReport.ps1') -CoveragePath $previousJson -ExpectedPackage $expected -OutputDirectory $runRoot -ReportOnly:$ReportOnly
}
finally { $coverageLock.Dispose() }
