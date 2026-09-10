#Requires -Version 7.0
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
Import-Module (Join-Path $root 'scripts/testing/BuildProvenance.psm1') -Force
$identity = if ($IsWindows) { $root.ToUpperInvariant() } else { $root }
$digest = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($identity)))
$mutex = [Threading.Mutex]::new($false, "vc.Ifx.FrameworkTests.$digest")
$acquired = $false
$checks = [Collections.Generic.List[string]]::new()
$run = Join-Path $root "TestResults/provenance-regressions/$([Guid]::NewGuid().ToString('N'))"
$fixture = Join-Path $run 'fixture'
$build = Join-Path $run 'build'
function Assert-Rejected([string] $Name, [scriptblock] $Action, [string] $Pattern) {
    $message = $null
    try { & $Action | Out-Null } catch { $message = $_.Exception.Message }
    if (-not $message -or $message -notmatch $Pattern) { throw "$Name did not fail as expected: $message" }
    $checks.Add($Name)
}
function Test-Mutation([string] $Name, [string] $Path, [switch] $Missing) {
    $bytes = [IO.File]::ReadAllBytes($Path)
    try {
        if ($Missing) { Remove-Item -LiteralPath $Path } else { [IO.File]::AppendAllText($Path, 'changed') }
        Assert-Rejected $Name { Assert-IfxBuildProvenance -BuildDirectory $build } 'Changed build input/output|Missing build input/output'
    } finally { [IO.File]::WriteAllBytes($Path, $bytes) }
}
try {
    $acquired = $mutex.WaitOne([TimeSpan]::FromMinutes(20))
    if (-not $acquired) { throw 'Timed out waiting for framework mutex.' }
    New-Item -ItemType Directory -Path $fixture | Out-Null
    '<Project><PropertyGroup><ImportDirectoryPackagesProps>false</ImportDirectoryPackagesProps><ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally></PropertyGroup></Project>' | Set-Content (Join-Path $fixture 'Directory.Build.props')
    "<Project><Import Project=`"$root/Directory.Build.targets`" /></Project>" | Set-Content (Join-Path $fixture 'Directory.Build.targets')
    '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><IsPackable>false</IsPackable><DebugType>portable</DebugType></PropertyGroup><ItemGroup><EmbeddedResource Include="*.json" /><Protobuf Include="*.proto" /></ItemGroup></Project>' | Set-Content (Join-Path $fixture 'Fixture.csproj')
    'public record Value(int Number);' | Set-Content (Join-Path $fixture 'Value.cs')
    '{"type":"object"}' | Set-Content (Join-Path $fixture 'schema.json')
    'syntax = "proto3";' | Set-Content (Join-Path $fixture 'dispatch.proto')
    & (Join-Path $root 'scripts/Invoke-FrameworkTests.ps1') -BuildOnly -Project ([IO.Path]::GetRelativePath($root, (Join-Path $fixture 'Fixture.csproj'))) -Configuration Release -ReportBuildDirectory $build -WarningsAsErrors
    $result = Assert-IfxBuildProvenance -BuildDirectory $build -Configuration Release
    if (@($result.manifest.projects).Count -ne 1 -or @($result.manifest.projects[0].projectReferences).Count -ne 0) { throw 'Fixture must have zero project references.' }
    $checks.Add('fresh real build with zero project references')
    $project = $result.manifest.projects[0]
    Test-Mutation 'changed DLL at same HEAD' $project.targetPath
    Test-Mutation 'missing DLL' $project.targetPath -Missing
    Test-Mutation 'changed PDB' ([IO.Path]::ChangeExtension($project.targetPath, '.pdb'))
    Test-Mutation 'missing PDB' ([IO.Path]::ChangeExtension($project.targetPath, '.pdb')) -Missing
    Test-Mutation 'changed handwritten record' (Join-Path $fixture 'Value.cs')
    Test-Mutation 'changed imported props' (Join-Path $fixture 'Directory.Build.props')
    Test-Mutation 'changed project' $project.projectPath
    Test-Mutation 'changed restore assets' (Join-Path $fixture 'obj/project.assets.json')
    foreach ($name in @('schema.json', 'dispatch.proto')) {
        Test-Mutation "changed non-code input $name" (Join-Path $fixture $name)
        Test-Mutation "missing non-code input $name" (Join-Path $fixture $name) -Missing
        $extra = Join-Path $fixture "extra$([IO.Path]::GetExtension($name))"
        try {
            'extra' | Set-Content -LiteralPath $extra
            Assert-Rejected "added non-code input $name" { Assert-IfxBuildProvenance -BuildDirectory $build } 'Changed non-code input inventory'
        } finally { Remove-Item -LiteralPath $extra }
    }
    $added = Join-Path $fixture 'Added.cs'
    try {
        'public class Added {}' | Set-Content $added
        Assert-Rejected 'added source' { Assert-IfxBuildProvenance -BuildDirectory $build } 'Changed compile source inventory'
    } finally { Remove-Item -LiteralPath $added }
    $addedOutput = Join-Path $project.settings.TargetDir 'extra.dll'
    try {
        'extra' | Set-Content $addedOutput
        Assert-Rejected 'added output' { Assert-IfxBuildProvenance -BuildDirectory $build } 'Changed build output inventory'
    } finally { Remove-Item -LiteralPath $addedOutput }
    $oldDefines = $env:DefineConstants
    try {
        $env:DefineConstants = 'PROVENANCE_CHANGED'
        Assert-Rejected 'changed evaluated constants' { Assert-IfxBuildProvenance -BuildDirectory $build } 'Changed build setting DefineConstants'
    } finally { $env:DefineConstants = $oldDefines }
    Assert-Rejected 'wrong configuration' { Assert-IfxBuildProvenance -BuildDirectory $build -Configuration Debug } 'configuration mismatch'
    Assert-Rejected 'wrong selection' { Assert-IfxBuildProvenance -BuildDirectory $build -Selection @{ Fingerprint = 'wrong' } } 'selection mismatch'
    Assert-Rejected 'uncaptured test project' { Assert-IfxBuildProvenance -BuildDirectory $build -RequiredProject (Join-Path $fixture 'Missing.csproj') } 'not compiled'
    Assert-Rejected 'missing build identity' { Assert-IfxBuildProvenance -BuildDirectory $fixture } 'Missing build identity'
    $manifestBytes = [IO.File]::ReadAllBytes($result.path)
    try {
        [IO.File]::AppendAllText($result.path, ' ')
        Assert-Rejected 'changed completed identity' { Assert-IfxBuildProvenance -BuildDirectory $build } 'does not match completed build'
    } finally { [IO.File]::WriteAllBytes($result.path, $manifestBytes) }
    $absent = @($project.absentInputs | Where-Object { $_.StartsWith($fixture + [IO.Path]::DirectorySeparatorChar, [StringComparison]::Ordinal) })
    if ($absent.Count -eq 0) { throw 'Fixture must record an absent local analyzer configuration.' }
    try {
        '' | Set-Content -LiteralPath $absent[0]
        # Unix dotfiles are hidden; model that attribute on Windows as well.
        if ($IsWindows) { [IO.File]::SetAttributes($absent[0], [IO.FileAttributes]::Hidden) }
        Assert-Rejected 'added previously absent config' { Assert-IfxBuildProvenance -BuildDirectory $build } 'Added build input'
    } finally { Remove-Item -LiteralPath $absent[0] -Force }
    if (Test-Path -LiteralPath $absent[0]) { throw 'Temporary hidden analyzer configuration was not removed.' }
    $checks.Add('hidden analyzer configuration cleanup')
    $coverage = Join-Path $run 'coverage'
    New-Item -ItemType Directory -Path $coverage | Out-Null
    @{ noBuild = $true; buildIdentity = @{ sha256 = $result.sha256 } } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $coverage 'run-context.json')
    Copy-Item -LiteralPath $result.path -Destination (Join-Path $coverage 'consumed-build-identity.json')
    @{ sha256 = $result.sha256; restoredOutputsVerified = $true } | ConvertTo-Json | Set-Content (Join-Path $coverage 'build-identity-verified.json')
    $null = Assert-IfxBuildProvenance -BuildDirectory $build -CoverageRunDirectory $coverage
    $checks.Add('matching consumed coverage identity')
    [IO.File]::AppendAllText((Join-Path $coverage 'consumed-build-identity.json'), ' ')
    Assert-Rejected 'changed consumed identity' { Assert-IfxBuildProvenance -BuildDirectory $build -CoverageRunDirectory $coverage } 'not bound'
    Remove-Item -LiteralPath (Join-Path $coverage 'consumed-build-identity.json')
    Assert-Rejected 'missing consumed identity' { Assert-IfxBuildProvenance -BuildDirectory $build -CoverageRunDirectory $coverage } 'not bound'
    Copy-Item -LiteralPath $result.path -Destination (Join-Path $coverage 'consumed-build-identity.json') -Force
    @{ sha256 = $result.sha256; restoredOutputsVerified = $false } | ConvertTo-Json | Set-Content (Join-Path $coverage 'build-identity-verified.json')
    Assert-Rejected 'unverified restored outputs' { Assert-IfxBuildProvenance -BuildDirectory $build -CoverageRunDirectory $coverage } 'did not verify'
    $null = Assert-IfxBuildProvenance -BuildDirectory $build
    $checks.Add('all fixture mutations restored')
    @{ passed = $checks.Count; checks = @($checks); buildIdentity = $result.path } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $run 'results.json')
    Write-Host "PASS: $($checks.Count) provenance checks. Artifacts: $run"
    $global:LASTEXITCODE = 0
} finally {
    if ($acquired) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}
