#Requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
Import-Module (Join-Path $repoRoot 'scripts/testing/TestSourceSelection.psm1') -Force
$identity = if ($IsWindows) { $repoRoot.ToUpperInvariant() } else { $repoRoot }
$digest = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($identity)))
$mutex = [Threading.Mutex]::new($false, "vc.Ifx.FrameworkTests.$digest")
$acquired = $false
try {
try { $acquired = $mutex.WaitOne([TimeSpan]::FromMinutes(20)) }
catch [Threading.AbandonedMutexException] { $acquired = $true; throw 'Abandoned framework mutex; rebuild before verification.' }
if (-not $acquired) { throw 'Framework mutex timeout.' }
$validator = Join-Path $repoRoot 'scripts/coverage/Test-CoverageReport.ps1'
$output = Join-Path $repoRoot "TestResults/coverage-infrastructure/$([Guid]::NewGuid().ToString('N'))"
$null = New-Item -ItemType Directory -Path $output -Force
$fixture = Join-Path $output 'coverage.json'
$script:passed = 0

function Assert-Report {
    param([string] $Name, [hashtable] $Document, [string[]] $Expected, [bool] $ShouldPass, [string] $ExpectedStatus)
    $Document | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $fixture -Encoding utf8
    $destination = Join-Path $output $Name
    $failure = $null
    try { & $validator -CoveragePath $fixture -ExpectedPackage $Expected -OutputDirectory $destination }
    catch { $failure = $_ }
    if (($null -eq $failure) -ne $ShouldPass) { throw "$Name produced unexpected result: $failure" }
    if ($ExpectedStatus) {
        $summary = Get-Content -LiteralPath (Join-Path $destination 'summary.json') -Raw | ConvertFrom-Json
        if ($ExpectedStatus -notin $summary.packages.status) { throw "${Name}: expected status $ExpectedStatus." }
    }
    $script:passed++
    Write-Host "PASS: $Name"
}

function New-CoverageDocument {
    param([hashtable] $Lines, [object[]] $Branches)
    return @{ 'vc.Ifx.Fixture.dll' = @{ 'Fixture.cs' = @{ Fixture = @{ Run = @{ Lines = $Lines; Branches = $Branches } } } } }
}

$complete = New-CoverageDocument @{ '1' = 1; '2' = 3 } @(@{ Hits = 1 }, @{ Hits = 2 })
Assert-Report 'complete' $complete @('vc.Ifx.Fixture') $true 'Passed'
Assert-Report 'missing-package' $complete @('vc.Ifx.Fixture', 'vc.Ifx.Missing') $false 'MissingModule'
Assert-Report 'missed-line' (New-CoverageDocument @{ '1' = 1; '2' = 0 } @(@{ Hits = 1 })) @('vc.Ifx.Fixture') $false 'BelowThreshold'
Assert-Report 'missed-branch' (New-CoverageDocument @{ '1' = 1 } @(@{ Hits = 0 })) @('vc.Ifx.Fixture') $false 'BelowThreshold'
Assert-Report 'branchless' (New-CoverageDocument @{ '1' = 1 } @()) @('vc.Ifx.Fixture') $true 'Passed'
Assert-Report 'no-lines' (New-CoverageDocument @{} @()) @('vc.Ifx.Fixture') $false 'NoExecutableLines'
Assert-Report 'no-modules' @{} @('vc.Ifx.Fixture') $false ''
Assert-Report 'negative-hits' (New-CoverageDocument @{ '1' = -1 } @()) @('vc.Ifx.Fixture') $false ''
Assert-Report 'invalid-branch' (New-CoverageDocument @{ '1' = 1 } @(@{ Wrong = 1 })) @('vc.Ifx.Fixture') $false ''
$manyLines = @{}
foreach ($line in 1..20001) { $manyLines["$line"] = 1 }
$manyLines['20001'] = 0
Assert-Report 'rounded-100-is-not-complete' (New-CoverageDocument $manyLines @()) @('vc.Ifx.Fixture') $false 'BelowThreshold'
$duplicate = $complete.Clone()
$duplicate['other/vc.Ifx.Fixture.dll'] = $complete['vc.Ifx.Fixture.dll']
Assert-Report 'duplicate-module' $duplicate @('vc.Ifx.Fixture') $false ''

foreach ($project in @('tests/unit/vc.Ifx.UnitTests/vc.Ifx.UnitTests.csproj', 'tests/integration/vc.Ifx.IntegrationTests/vc.Ifx.IntegrationTests.csproj')) {
    $evaluation = & dotnet msbuild (Join-Path $repoRoot $project) '-getProperty:LangVersion,IsTestProject,IsPackable,GeneratePackageOnBuild,GenerateDocumentationFile,CollectCoverage,Threshold,ThresholdStat,ThresholdType,ExcludeByAttribute,SkipAutoProps,ExcludeAssembliesWithoutSources' '-m:1' '-p:BuildInParallel=false'
    if ($LASTEXITCODE -ne 0) { throw "MSBuild evaluation failed: $project" }
    $properties = ($evaluation | ConvertFrom-Json -AsHashtable).Properties
    $expectedProperties = @{
        LangVersion = '14.0'; IsTestProject = 'true'; IsPackable = 'false'; GeneratePackageOnBuild = 'false'
        GenerateDocumentationFile = 'false'; CollectCoverage = 'false'; Threshold = '100'
        ThresholdStat = 'minimum'; ThresholdType = 'line,branch'; ExcludeByAttribute = 'GeneratedCodeAttribute'
        SkipAutoProps = 'false'; ExcludeAssembliesWithoutSources = 'None'
    }
    foreach ($name in $expectedProperties.Keys) {
        if ($properties[$name] -cne $expectedProperties[$name]) { throw "$project has incorrect evaluated $name = $($properties[$name])." }
    }
    $script:passed++
    Write-Host "PASS: evaluated test configuration $project"
}
$testProject = Join-Path $repoRoot 'tests/unit/vc.Ifx.UnitTests/vc.Ifx.UnitTests.csproj'
$scopedJson = & dotnet msbuild $testProject '-p:IfxTestSourceScope=Filtering' '-p:IfxTestPackage=vc.Ifx.Filtering' '-getItem:Compile,ProjectReference' '-getProperty:OutputPath,MSBuildProjectExtensionsPath' '-m:1' '-p:BuildInParallel=false'
if ($LASTEXITCODE -ne 0) { throw 'Scoped test evaluation failed.' }
$scoped = $scopedJson | ConvertFrom-Json
$selected = @($scoped.Items.Compile | ForEach-Object { $_.Identity.Replace('\', '/') })
if ('Usings.cs' -notin $selected -or @($selected | Where-Object { $_ -like 'Filtering/*' }).Count -eq 0) { throw 'Scoped test setup or selected tests missing.' }
if (@($selected | Where-Object { $_ -notmatch '^(Filtering/|Helpers/|Usings\.cs$|GlobalUsings\.cs$|AssemblyInfo\.cs$|Properties/AssemblyInfo\.cs$)' }).Count -gt 0) { throw 'Unrelated test sources leaked into scope.' }
if (@($scoped.Items.ProjectReference).Count -ne 1 -or $scoped.Items.ProjectReference[0].Filename -ne 'vc.Ifx.Filtering') { throw 'Scoped test references were not restricted.' }
if ($scoped.Properties.OutputPath.Replace('\', '/') -notlike '*/bin/scoped/Filtering/*' -or $scoped.Properties.MSBuildProjectExtensionsPath.Replace('\', '/') -notlike '*/obj/scoped/Filtering/*') { throw 'Scoped output isolation failed.' }
$script:passed++
Write-Host 'PASS: scoped sources, direct references, and output isolation'
$fullJson = & dotnet msbuild $testProject '-p:IfxTestSourceScope=' '-p:IfxTestPackage=' '-getItem:Compile,ProjectReference' '-getProperty:OutputPath' '-m:1' '-p:BuildInParallel=false'
if ($LASTEXITCODE -ne 0) { throw 'Full test evaluation failed.' }
$full = $fullJson | ConvertFrom-Json
$sourceProjectCount = @(Get-ChildItem (Join-Path $repoRoot 'src') -Recurse -Filter '*.csproj' -File | Where-Object FullName -NotMatch '[\\/](obj|bin)[\\/]').Count
if (@($full.Items.Compile).Count -le $selected.Count -or @($full.Items.ProjectReference).Count -ne $sourceProjectCount -or $full.Properties.OutputPath -match 'scoped') { throw 'Full test configuration lost sources/references or reused scoped outputs.' }
if (@($full.Items.Compile | Where-Object { $_.Identity -match '(^|[\\/])(obj|bin)[\\/]' }).Count -gt 0) { throw 'Scoped generated files leaked into full compilation.' }
$script:passed++
Write-Host 'PASS: full configuration retains all tests and references'

function Assert-Selection {
    param([string] $Name, [string] $Scopes, [string] $Package)
    $selection = Resolve-IfxTestSelection -ProjectPath $testProject -SourceScope $Scopes -TestPackage $Package
    $json = & dotnet msbuild $testProject @($selection.Arguments) '-t:ValidateIfxTestSourceScope' '-getItem:Compile,ProjectReference' '-getProperty:OutputPath,MSBuildProjectExtensionsPath,GeneratedProgramFile' '-m:1' '-p:BuildInParallel=false'
    if ($LASTEXITCODE -ne 0) { throw "Selection evaluation failed: $Name`n$json" }
    $evaluation = $json | ConvertFrom-Json
    $expected = @($full.Items.Compile | Where-Object {
        $path = $_.Identity.Replace('\', '/')
        $keep = $path -match '^(Helpers/|Usings\.cs$|GlobalUsings\.cs$|AssemblyInfo\.cs$|Properties/AssemblyInfo\.cs$)'
        foreach ($scope in $selection.Scopes) {
            if ($path -ceq $scope -or $path.StartsWith("$scope/", [StringComparison]::Ordinal)) { $keep = $true }
        }
        $keep
    } | ForEach-Object { $_.Identity.Replace('\', '/') } | Sort-Object -Unique)
    # The restored test SDK's InitialTargets adds its own entry point when a target is executed.
    if ($evaluation.Properties.GeneratedProgramFile) { $expected += $evaluation.Properties.GeneratedProgramFile.Replace('\', '/') }
    $actual = @($evaluation.Items.Compile | ForEach-Object { $_.Identity.Replace('\', '/') } | Sort-Object -Unique)
    $difference = @(Compare-Object $expected $actual -CaseSensitive)
    if ($difference.Count) { throw "Exact source selection mismatch: $Name`n$($difference | Out-String)" }
    $expectedReferences = if ($Package) { @($Package) } else { @($full.Items.ProjectReference.Filename | Sort-Object) }
    if (@(Compare-Object $expectedReferences @($evaluation.Items.ProjectReference.Filename | Sort-Object) -CaseSensitive).Count) { throw "Reference isolation mismatch: $Name" }
    if ($evaluation.Properties.OutputPath.Replace('\', '/') -notlike "*/bin/scoped/$($selection.OutputKey)/*" -or
        $evaluation.Properties.MSBuildProjectExtensionsPath.Replace('\', '/') -notlike "*/obj/scoped/$($selection.OutputKey)/*") { throw "Output isolation mismatch: $Name" }
    $evaluation | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath (Join-Path $output "$Name.json") -Encoding utf8
    $script:passed++
    Write-Host "PASS: $Name exact source/reference/output isolation ($($actual.Count) files)"
    return $selection
}

$single = Assert-Selection 'single-compatible' 'Filtering' 'vc.Ifx.Filtering'
$combined = Assert-Selection 'proxy-combined' 'Proxy;Authentication;Authorization;Caching;Logging' 'vc.Ifx.Proxy'
$unrestricted = Assert-Selection 'combined-all-references' 'Proxy;Authentication;Authorization;Caching;Logging' ''
$legacy = Assert-Selection 'aggregator-literal-root' 'Aggregator;ConstantsTests.cs;FrameworkConstantsTests.cs' 'vc.Ifx'
$nested = Assert-Selection 'nested-folders' 'Filtering/EntityFrameworkCore;Infrastructure' 'vc.Ifx.Filtering.EntityFrameworkCore'
$reordered = Resolve-IfxTestSelection -ProjectPath $testProject -SourceScope ' Logging ;Proxy;Caching;Authentication;Authorization;Proxy ' -TestPackage vc.Ifx.Proxy
if ($combined.Fingerprint -cne $reordered.Fingerprint -or $combined.OutputKey -cne $reordered.OutputKey -or
    $combined.OutputKey -ceq $unrestricted.OutputKey -or $single.OutputKey -ceq $combined.OutputKey -or
    $combined.Fingerprint.Length -ne 64) { throw 'Canonical scope/reference key regression.' }
$script:passed++
Write-Host 'PASS: canonical ordering/deduplication and reference-sensitive SHA256 identity'

function Assert-Rejected {
    param([string] $Name, [scriptblock] $Action)
    $rejected = $false
    try { & $Action | Out-Null } catch { $rejected = $true }
    if (-not $rejected) { throw "Expected rejection: $Name" }
    $script:passed++
    Write-Host "PASS: rejects $Name"
}
foreach ($invalid in @('../Proxy', '/Proxy', 'C:/Proxy', 'Proxy;;Logging', 'Proxy;', 'Proxy/*', 'Proxy%3BLogging', 'Proxy/../Logging', 'obj', 'MissingScope', 'Proxy/ProxyContractTests.cs', 'proxy', 'Proxy;$(Bad)')) {
    Assert-Rejected "invalid selector $invalid" { Resolve-IfxTestSelection -ProjectPath $testProject -SourceScope $invalid }
}
Assert-Rejected 'unknown package' { Resolve-IfxTestSelection -ProjectPath $testProject -SourceScope Proxy -TestPackage vc.Ifx.Missing }
Assert-Rejected 'package without sources' { Resolve-IfxTestSelection -ProjectPath $testProject -TestPackage vc.Ifx.Proxy }
$pathFixture = Join-Path $output 'path-fixture'
$outsideFixture = Join-Path $output 'outside-fixture'
$null = New-Item -ItemType Directory -Path $pathFixture, $outsideFixture, (Join-Path $pathFixture 'Nested')
$fixtureProject = Join-Path $pathFixture 'vc.Ifx.UnitTests.csproj'
'<Project />' | Set-Content -LiteralPath $fixtureProject -Encoding utf8
$linkType = if ($IsWindows) { 'Junction' } else { 'SymbolicLink' }
$null = New-Item -ItemType $linkType -Path (Join-Path $pathFixture 'Linked') -Target $outsideFixture
$null = New-Item -ItemType $linkType -Path (Join-Path $pathFixture 'Nested/Linked') -Target $outsideFixture
Assert-Rejected 'scope linked outside test root' { Resolve-IfxTestSelection -ProjectPath $fixtureProject -SourceScope Linked }
Assert-Rejected 'nested link outside test root' { Resolve-IfxTestSelection -ProjectPath $fixtureProject -SourceScope Nested }
foreach ($argument in @('Filter', 'TestSourceScope', 'TestPackage')) {
    $parameters = @{ FullCoverage = $true; $argument = 'Proxy' }
    Assert-Rejected "FullCoverage with $argument" { & (Join-Path $repoRoot 'scripts/Invoke-FrameworkTests.ps1') @parameters }
}
Assert-Rejected 'coverage core Full with filter' { & (Join-Path $repoRoot 'scripts/coverage/Invoke-CoverageCore.ps1') -Full -Filter Proxy }
$context = [pscustomobject]@{ testSourceScope = $combined.SourceScope; testPackage = $combined.TestPackage; testSelection = $combined }
Assert-IfxTestSelectionContext -Context $context
$script:passed++
Write-Host 'PASS: canonical scoped report provenance'
Assert-Rejected 'scoped report provenance labeled full' { Assert-IfxTestSelectionContext -Context $context -RequireFull }
$context.testSourceScope = ''
Assert-Rejected 'hidden scope in report context' { Assert-IfxTestSelectionContext -Context $context }
$context.testSourceScope = $combined.SourceScope
$combined.Fingerprint = '0' * 64
Assert-Rejected 'tampered selection fingerprint' { Assert-IfxTestSelectionContext -Context $context }
$combined.Fingerprint = $reordered.Fingerprint
$combined.OutputKey = 'other'
Assert-Rejected 'tampered selection output key' { Assert-IfxTestSelectionContext -Context $context }
$empty = Resolve-IfxTestSelection -ProjectPath $testProject
Assert-IfxTestSelectionContext -Context ([pscustomobject]@{ testSourceScope = ''; testPackage = ''; testSelection = $empty }) -RequireFull
$script:passed++
Write-Host 'PASS: full report provenance remains unscoped'
Write-Host "$script:passed coverage infrastructure checks passed. Artifacts: $output"
} finally {
    if ($acquired) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}
