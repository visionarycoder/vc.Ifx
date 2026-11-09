#Requires -Version 7.0
[CmdletBinding()]
param([switch] $UnderFrameworkMutex)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $UnderFrameworkMutex) { throw 'Use Invoke-FrameworkTests.ps1 -BuildOnly -Project scripts/packaging/CompilerHostSmoke.proj.' }
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$artifacts = Join-Path $root ('test-results/compressed-packages/' + [Guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $artifacts
Write-Host "Package artifacts: $artifacts"
Import-Module (Join-Path $PSScriptRoot 'PackageValidation.psm1') -Force
function Invoke-DotNet([string[]] $Arguments) {
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed: $($Arguments -join ' ')" }
}
$projects = @(Get-ChildItem (Join-Path $root 'src') -Recurse -Filter '*.csproj' -File | Where-Object FullName -NotMatch '[\\/](bin|obj)[\\/]')
$projects = @($projects | Where-Object { [xml] $definition = Get-Content -LiteralPath $_.FullName -Raw; $null -eq $definition.SelectSingleNode('//IsPackable[text()="false"]') })
if ($projects.Count -ne 8) { throw 'Expected the eight adopted production libraries.' }
foreach ($project in $projects) {
    Invoke-DotNet @('pack', $project.FullName, '-c', 'Release', '--no-restore', '-o', $artifacts, '-m:1', '-p:BuildInParallel=false', '-p:GeneratePackageOnBuild=false', '-warnaserror', '-v:minimal')
    $archive = @(Get-ChildItem $artifacts -Filter '*.nupkg' | Where-Object Name -Match ('^' + [regex]::Escape($project.BaseName) + '\.\d+\.\d+\.\d+.*\.nupkg$'))
    if ($archive.Count -ne 1) { throw "Expected one package for $($project.BaseName)." }
    Test-PackageArchive -Path $archive[0].FullName -PackageId $project.BaseName -ReadmePath (Join-Path $project.DirectoryName 'README.md') | Out-Host
    [IO.Compression.ZipFile]::ExtractToDirectory($archive[0].FullName, (Join-Path $artifacts $project.BaseName))
}
& (Join-Path $PSScriptRoot 'Test-PackageValidation.ps1') -RepositoryRoot $root -PackageDirectory $artifacts
# Use a clean external host without project references to the checkout.
$hostDirectory = Join-Path ([IO.Path]::GetTempPath()) ('ifx-package-host-' + [Guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $hostDirectory
Copy-Item (Join-Path $PSScriptRoot 'compiler-host/CompilerHost.csproj.template') (Join-Path $hostDirectory 'CompilerHost.csproj')
Copy-Item (Join-Path $PSScriptRoot 'compiler-host/CompressedHost.cs.txt') (Join-Path $hostDirectory 'Program.cs')
[xml] $versions = Get-Content (Join-Path $root 'Directory.Packages.props') -Raw
$version = $versions.SelectSingleNode('//PackageVersion[@Include="Microsoft.CodeAnalysis.CSharp.Workspaces"]').Version
Invoke-DotNet @('build', (Join-Path $hostDirectory 'CompilerHost.csproj'), '-c', 'Release', "-p:HostRoslynVersion=$version", '-m:1', '-p:BuildInParallel=false', '-warnaserror', '-v:minimal')
Invoke-DotNet @('exec', (Join-Path $hostDirectory 'bin/Release/net10.0/CompilerHost.dll'), $artifacts)
& (Join-Path $PSScriptRoot 'Stage-ReleasePackages.ps1') -PackageDirectory $artifacts
& (Join-Path $root 'tests/infrastructure/packaging/Test-ReleaseArtifactManifest.ps1') -Directory (Join-Path $artifacts 'release')
Write-Host "PASS: eight package/symbol pairs, compiler loading, generation, diagnostics, and standalone IDE-host code fixes. Artifacts: $artifacts"
