#Requires -Version 7.0
[CmdletBinding()]
param([Parameter(Mandatory)][string] $Directory)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
Import-Module (Join-Path $root 'scripts/packaging/PackageArtifactManifest.psm1') -Force
$manifestPath = Join-Path $Directory 'package-manifest.json'
$original = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($original.schemaVersion -ne 2) { throw 'Expected current release manifest.' }
$hash = (Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash.ToLowerInvariant()
$null = Test-PackageArtifactManifest -Directory $Directory -Revision $original.revision -ManifestSha256 $hash
$cases = [ordered]@{
    'archive substitution' = { param($folder, $manifest) [IO.File]::AppendAllText((Join-Path $folder $manifest.packages[0].package.file), 'changed') }
    'missing archive' = { param($folder, $manifest) Move-Item -LiteralPath (Join-Path $folder $manifest.packages[0].package.file) -Destination (Join-Path $folder 'renamed') }
    'extra entry' = { param($folder, $manifest) 'unexpected' | Set-Content -LiteralPath (Join-Path $folder 'extra.txt') }
    'SDK substitution' = { param($folder, $manifest) '{}' | Set-Content -LiteralPath (Join-Path $folder 'global.json') }
    'wrong revision' = { param($folder, $manifest) $manifest.revision = 'b' * 40 }
    'unknown schema' = { param($folder, $manifest) $manifest.schemaVersion = 3 }
    'duplicate identity' = { param($folder, $manifest) $manifest.packages[1].id = $manifest.packages[0].id }
    'invalid archive path' = { param($folder, $manifest) $manifest.packages[0].package.file = '../outside.nupkg' }
}
foreach ($case in $cases.GetEnumerator()) {
    $folder = Join-Path $root ('test-results/release-manifest-tests/' + [Guid]::NewGuid().ToString('N'))
    $null = New-Item -ItemType Directory -Path $folder
    Get-ChildItem -LiteralPath $Directory -File | Copy-Item -Destination $folder
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    & $case.Value $folder $manifest
    $path = Join-Path $folder 'package-manifest.json'
    $manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $path -Encoding utf8
    $caseHash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    $rejected = $false
    try { $null = Test-PackageArtifactManifest -Directory $folder -Revision $original.revision -ManifestSha256 $caseHash }
    catch { $rejected = $true }
    if (-not $rejected) { throw "Accepted malformed release artifacts: $($case.Key)" }
    Write-Host "PASS: rejected $($case.Key)"
}
Write-Host 'PASS: schema v2 release inventory and eight mutation rejections.'
