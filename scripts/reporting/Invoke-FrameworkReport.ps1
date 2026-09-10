#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $BuildDirectory,
    [Parameter(Mandatory)][string] $CoverageRunDirectory,
    [Parameter(Mandatory)][string] $Revision,
    [string[]] $Package,
    [ValidateSet('Debug', 'Release')][string] $Configuration = 'Release',
    [ValidateRange(1, 3600)][int] $LockTimeoutSeconds = 1200
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
Import-Module (Join-Path $root 'scripts/testing/TestSourceSelection.psm1') -Force
Import-Module (Join-Path $root 'scripts/testing/BuildProvenance.psm1') -Force
$build = [IO.Path]::GetFullPath($BuildDirectory)
$coverageRun = [IO.Path]::GetFullPath($CoverageRunDirectory)
$output = Join-Path $build 'report-v1'
$identity = if ($IsWindows) { $root.ToUpperInvariant() } else { $root }
$digest = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($identity)))
$mutex = [Threading.Mutex]::new($false, "vc.Ifx.FrameworkTests.$digest")
$acquired = $false
try {
    Write-Host 'Waiting for exclusive framework build/test/report access...'
    try { $acquired = $mutex.WaitOne([TimeSpan]::FromSeconds($LockTimeoutSeconds)) }
    catch [Threading.AbandonedMutexException] { $acquired = $true; throw 'Previous build/test owner abandoned its mutex; rebuild evidence before reporting.' }
    if (-not $acquired) { throw 'Timed out waiting for framework build/test access.' }
    if (Test-Path -LiteralPath $output) { throw 'Report output already exists. Use fresh build evidence; no reports are overwritten.' }
    $projects = @(Get-ChildItem (Join-Path $root 'src') -Recurse -Filter '*.csproj' -File | Where-Object FullName -NotMatch '[\\/](obj|bin)[\\/]')
    $expected = @($projects.BaseName | Sort-Object)
    $scope = 'full'
    if ($Package) {
        if (@($Package | Where-Object { $_ -cnotin $expected }).Count -gt 0) { throw 'Unknown package requested.' }
        $expected = $Package
        $scope = 'scoped'
    }
    $context = Get-Content -LiteralPath (Join-Path $coverageRun 'run-context.json') -Raw | ConvertFrom-Json
    $buildContext = Get-Content -LiteralPath (Join-Path $build 'build-complete.json') -Raw | ConvertFrom-Json
    Assert-IfxTestSelectionContext -Context $context -RequireFull:($scope -eq 'full')
    Assert-IfxTestSelectionContext -Context $buildContext -RequireFull:($scope -eq 'full')
    if ($buildContext.revision -cne $Revision -or $context.revision -cne $Revision) { throw 'Build, coverage and requested revisions must match.' }
    if ($buildContext.configuration -cne $Configuration) { throw 'Build configuration does not match reporting.' }
    if ($scope -eq 'full' -and (-not $context.full -or $context.filter -or $context.testSourceScope -or $context.testPackage)) {
        throw 'Full reporting requires an unfiltered full coverage run.'
    }
    if ($scope -eq 'full' -and ($context.reportBuildDirectory -cne $build -or $buildContext.project -cne 'vc.Ifx.slnx' -or $buildContext.testSourceScope)) {
        throw 'Full reporting requires the same complete solution build used by this coverage run.'
    }
    if ($context.configuration -cne $Configuration) { throw 'Build/report and coverage configurations must match.' }
    $boundBuild = $scope -eq 'full' -or ($context.PSObject.Properties['noBuild'] -and $context.noBuild)
    if ($boundBuild) {
        $null = Assert-IfxBuildProvenance -BuildDirectory $build -CoverageRunDirectory $coverageRun -Configuration $Configuration
    }
    $coverageJson = $null
    $summaryPath = Join-Path $coverageRun 'summary.json'
    if (Test-Path -LiteralPath $summaryPath) {
        $summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
        $coveragePath = [IO.Path]::GetFullPath($summary.coverageFile)
        if (-not $coveragePath.StartsWith($coverageRun + [IO.Path]::DirectorySeparatorChar, [StringComparison]::Ordinal)) { throw 'Coverage summary points outside this run.' }
        $coverageJson = [IO.File]::ReadAllText($coveragePath)
    }
    $packages = foreach ($name in $expected) {
        $directory = Join-Path $build $name
        $manifest = Join-Path $directory 'sources.txt'
        $sarif = Join-Path $directory 'diagnostics.sarif'
        $sources = $null
        if (Test-Path -LiteralPath $manifest) {
            $symbols = @((Get-Content -LiteralPath (Join-Path $directory 'symbols.txt')) | Where-Object { $_ })
            $sources = @(foreach ($line in Get-Content -LiteralPath $manifest) {
                $parts = $line.Split('|', 2)
                if ($parts.Length -ne 2) { throw 'Malformed compile manifest.' }
                $path = [IO.Path]::GetFullPath($parts[1])
                $relative = [IO.Path]::GetRelativePath($root, $path).Replace('\', '/')
                if ($relative.StartsWith('../', [StringComparison]::Ordinal)) { throw 'Source lies outside the repository.' }
                if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -cne $parts[0]) { throw "Source changed since compilation: $relative" }
                @{ path = $relative; text = [IO.File]::ReadAllText($path); symbols = $symbols }
            })
        }
        @{ package = $name; sarifJson = $(if (Test-Path -LiteralPath $sarif) { [IO.File]::ReadAllText($sarif) } else { $null }); sources = $sources }
    }
    $request = @{ revision = $Revision; scope = $scope; expectedPackages = $expected; coverageJson = $coverageJson; packages = @($packages) }
    $requestPath = Join-Path $build 'report-request.json'
    $request | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $requestPath -Encoding utf8
    if (-not $boundBuild) {
        & dotnet build (Join-Path $PSScriptRoot 'FrameworkReport.Host.csproj') -c $Configuration '-m:1' '-p:BuildInParallel=false' '-p:GeneratePackageOnBuild=false'
        if ($LASTEXITCODE -ne 0) { throw 'Reporting host build failed.' }
    }
    & dotnet run --project (Join-Path $PSScriptRoot 'FrameworkReport.Host.csproj') --no-build -c $Configuration -- $requestPath $output
    if ($LASTEXITCODE -ne 0) { throw "Reporting failed (exit $LASTEXITCODE). See $output." }
}
finally {
    if ($acquired) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}
