#Requires -Version 7.0
[CmdletBinding()]
param([Parameter(Mandatory)][string] $PackageDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$revision = (& git -C $root rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $revision -cnotmatch '\A[0-9a-f]{40}\z') { throw 'Cannot determine source revision.' }
$destination = Join-Path $PackageDirectory 'release'
if (Test-Path -LiteralPath $destination) { throw 'Release staging must be fresh.' }
$expected = @('Ifx', 'Ifx.Integrations', 'Ifx.Integrations.Azure', 'Ifx.Integrations.Windows', 'Ifx.Roslyn', 'Ifx.Roslyn.Analyzers', 'Ifx.Roslyn.Generators', 'Ifx.Roslyn.CodeFixes')
$packages = @(foreach ($id in $expected) {
    $files = @(Get-ChildItem -LiteralPath $PackageDirectory -Filter '*.nupkg' -File | Where-Object Name -Match ('^' + [regex]::Escape($id) + '\.\d+\.\d+\.\d+.*\.nupkg$'))
    if ($files.Count -ne 1) { throw "Expected one archive for $id." }
    $version = $files[0].Name.Substring($id.Length + 1) -replace '\.nupkg$', ''
    $entries = @{}
    foreach ($kind in @('package', 'symbols')) {
        $extension = if ($kind -eq 'package') { 'nupkg' } else { 'snupkg' }
        $name = "$id.$version.$extension"
        $archive = [IO.Compression.ZipFile]::OpenRead((Join-Path $PackageDirectory $name))
        try {
            $entry = $archive.GetEntry("$id.nuspec")
            if ($null -eq $entry) { throw "Missing package specification: $name" }
            $reader = [IO.StreamReader]::new($entry.Open())
            try { [xml] $spec = $reader.ReadToEnd() } finally { $reader.Dispose() }
            $metadata = $spec.SelectSingleNode('/*[local-name()="package"]/*[local-name()="metadata"]')
            if ($metadata.id -cne $id -or $metadata.version -cne $version -or $metadata.repository.GetAttribute('commit') -cne $revision) {
                throw "Archive identity or revision does not match this build: $name"
            }
        } finally { $archive.Dispose() }
        $entries[$kind] = [ordered]@{ file = $name; sha256 = (Get-FileHash -LiteralPath (Join-Path $PackageDirectory $name) -Algorithm SHA256).Hash.ToLowerInvariant() }
    }
    [ordered]@{ id = $id; version = $version; package = $entries.package; symbols = $entries.symbols }
})
$null = New-Item -ItemType Directory -Path $destination
foreach ($package in $packages) {
    foreach ($entry in @($package.package, $package.symbols)) { Copy-Item -LiteralPath (Join-Path $PackageDirectory $entry.file) -Destination $destination }
}
Copy-Item -LiteralPath (Join-Path $root 'global.json') -Destination $destination
$manifest = [ordered]@{
    schemaVersion = 2; revision = $revision
    sdk = [ordered]@{ file = 'global.json'; sha256 = (Get-FileHash -LiteralPath (Join-Path $destination 'global.json') -Algorithm SHA256).Hash.ToLowerInvariant() }
    packages = $packages
}
$manifestPath = Join-Path $destination 'package-manifest.json'
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $manifestPath -Encoding utf8
$hash = (Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash.ToLowerInvariant()
Import-Module (Join-Path $PSScriptRoot 'PackageArtifactManifest.psm1') -Force
$null = Test-PackageArtifactManifest -Directory $destination -Revision $revision -ManifestSha256 $hash
if ($env:GITHUB_ENV) { "IFX_PACKAGE_UPLOAD_DIRECTORY=$destination" | Add-Content -LiteralPath $env:GITHUB_ENV -Encoding utf8 }
if ($env:GITHUB_OUTPUT) { "manifest-sha256=$hash" | Add-Content -LiteralPath $env:GITHUB_OUTPUT -Encoding utf8 }
Write-Host "Validated publication inventory: $destination ($hash)"
