#Requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$identity = if ($IsWindows) { $root.ToUpperInvariant() } else { $root }
$digest = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($identity)))
$mutex = [Threading.Mutex]::new($false, "vc.Ifx.FrameworkTests.$digest")
$acquired = $false
$script:passed = 0
$run = Join-Path $root "TestResults/docs-project/$([Guid]::NewGuid().ToString('N'))"
function Invoke-CheckedDotNet {
    param([string] $Name, [string[]] $Arguments)
    $result = & dotnet @Arguments '-m:1' '-p:BuildInParallel=false' '-p:GeneratePackageOnBuild=false' '-warnaserror' 2>&1
    $exitCode = $LASTEXITCODE
    $result | Set-Content -LiteralPath (Join-Path $run "$Name.log") -Encoding utf8
    if ($exitCode -ne 0 -or @($result | Where-Object { "$_" -match ': warning ' }).Count) { throw "$Name failed: $result" }
    $script:passed++
    Write-Host "PASS: $Name"
    return ($result -join [Environment]::NewLine)
}
function Assert-NoOutputs([string] $Directory) {
    $outputs = @(Get-ChildItem -LiteralPath $Directory -Recurse -Directory -Force | Where-Object Name -In @('obj','bin','.no-output'))
    if ($outputs.Count) { throw "Unexpected documentation outputs: $($outputs.FullName)" }
}
try {
    try { $acquired = $mutex.WaitOne([TimeSpan]::FromMinutes(20)) }
    catch [Threading.AbandonedMutexException] { $acquired = $true; throw 'Abandoned framework mutex.' }
    if (-not $acquired) { throw 'Framework mutex timeout.' }
    $null = New-Item -ItemType Directory -Path $run
    $docs = Join-Path $root 'docs'
    $project = Join-Path $docs 'docs.csproj'
    Assert-NoOutputs $docs
    [xml] $solution = Get-Content (Join-Path $root 'vc.Ifx.slnx') -Raw
    if (@($solution.SelectNodes('//Project[@Path="docs/docs.csproj"]')).Count -ne 1 -or
        @($solution.SelectNodes('//File') | Where-Object { $_.Path.StartsWith('docs/') }).Count) { throw 'Documentation must be one real solution project, not manual File items.' }
    $script:passed++
    Write-Host 'PASS: one documentation Project entry, no manual documentation File entries'
    $evaluation = Invoke-CheckedDotNet 'live-items' @('msbuild', $project, '-getItem:None,Compile,PackageReference,ProjectReference', '-getProperty:UsingMicrosoftNETSdk,TargetFramework,LangVersion,IsPackable,RestoreProjectStyle') | ConvertFrom-Json
    $properties = $evaluation.Properties
    if ($properties.UsingMicrosoftNETSdk -ne 'true' -or $properties.TargetFramework -ne 'net10.0' -or $properties.LangVersion -ne '14.0' -or
        $properties.IsPackable -ne 'false' -or $properties.RestoreProjectStyle -ne 'Unknown' -or $evaluation.Items.Compile.Count -or
        $evaluation.Items.PackageReference.Count -or $evaluation.Items.ProjectReference.Count) { throw 'Documentation project metadata or dependencies changed.' }
    $expected = @(Get-ChildItem $docs -Recurse -File -Force | Where-Object FullName -NE $project | ForEach-Object { [IO.Path]::GetRelativePath($docs, $_.FullName).Replace('\','/') })
    $actual = @($evaluation.Items.None | ForEach-Object {
        if ($_.Identity -cne $_.Link -or [IO.Path]::IsPathRooted($_.Link)) { throw 'Documentation item is not a matching relative Link.' }
        $_.Identity.Replace('\','/')
    })
    if (@(Compare-Object $expected $actual -CaseSensitive).Count) { throw 'Automatic documentation inventory mismatch.' }
    $script:passed++
    Write-Host "PASS: $($actual.Count) automatic relative documentation items; no code or dependencies"
    foreach ($verb in @('restore','build','clean','pack')) {
        $null = Invoke-CheckedDotNet "live-$verb" @($verb, $project, '-v:minimal')
        Assert-NoOutputs $docs
    }
    $null = Invoke-CheckedDotNet 'live-rebuild' @('msbuild', $project, '-t:Rebuild', '-v:minimal')
    $null = Invoke-CheckedDotNet 'live-design-time-target' @('msbuild', $project, '-t:CompileDesignTime', '-p:DesignTimeBuild=true', '-v:minimal')
    Assert-NoOutputs $docs
    $copy = Join-Path $run 'fixture/docs'
    $runtime = Join-Path $run 'fixture/runtime'
    $null = New-Item -ItemType Directory -Path $copy, $runtime
    Copy-Item -LiteralPath $project -Destination (Join-Path $copy 'docs.csproj')
    $null = Invoke-CheckedDotNet 'fixture-before-add' @('msbuild', (Join-Path $copy 'docs.csproj'), '-getItem:None')
    $null = New-Item -ItemType Directory -Path (Join-Path $copy 'new/nested')
    '# Added after the first evaluation' | Set-Content -LiteralPath (Join-Path $copy 'new/nested/added.md') -Encoding utf8
    $after = Invoke-CheckedDotNet 'fixture-after-add' @('msbuild', (Join-Path $copy 'docs.csproj'), '-getItem:None') | ConvertFrom-Json
    if ($after.Items.None.Count -ne 1 -or $after.Items.None[0].Link.Replace('\','/') -cne 'new/nested/added.md') { throw 'New nested document was not discovered automatically.' }
    $script:passed++
    Write-Host 'PASS: newly added nested document appears without project or solution edits'
    @'
<Project>
  <PropertyGroup>
    <ImportDirectoryBuildProps>false</ImportDirectoryBuildProps>
    <ImportDirectoryBuildTargets>false</ImportDirectoryBuildTargets>
    <ImportDirectoryPackagesProps>false</ImportDirectoryPackagesProps>
  </PropertyGroup>
  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk" />
  <PropertyGroup><TargetFramework>net10.0</TargetFramework><LangVersion>14.0</LangVersion><IsPackable>false</IsPackable></PropertyGroup>
  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk" />
</Project>
'@ | Set-Content -LiteralPath (Join-Path $runtime 'runtime.csproj') -Encoding utf8
    '<Solution><Folder Name="/docs/"><Project Path="docs/docs.csproj" /></Folder><Project Path="runtime/runtime.csproj" /></Solution>' |
        Set-Content -LiteralPath (Join-Path $run 'fixture/mixed.slnx') -Encoding utf8
    $null = Invoke-CheckedDotNet 'mixed-solution-release' @('build', (Join-Path $run 'fixture/mixed.slnx'), '-c', 'Release', '-v:minimal')
    Assert-NoOutputs $copy
    Assert-NoOutputs $docs
    $script:passed++
    Write-Host 'PASS: no documentation output directories after all operations'
    @{ checks = $script:passed; documentCount = $actual.Count; noDocumentationOutputs = $true; visualStudioVerified = $false } |
        ConvertTo-Json | Set-Content -LiteralPath (Join-Path $run 'summary.json') -Encoding utf8
    Write-Host "$script:passed documentation project checks passed. Artifacts: $run"
} finally {
    if ($acquired) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}
