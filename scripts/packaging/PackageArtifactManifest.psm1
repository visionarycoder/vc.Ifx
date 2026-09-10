#Requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-ArtifactRule([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}

function Get-ArtifactHash([string] $Path) {
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Assert-ArchiveIdentity {
    param([string] $Path, [string] $Id, [string] $Version, [string] $Revision, [hashtable] $ExpectedPayloads)
    $archive = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $specs = @($archive.Entries | Where-Object FullName -CLike '*.nuspec')
        Assert-ArtifactRule ($specs.Count -eq 1) "Expected one nuspec: $Path"
        $reader = [IO.StreamReader]::new($specs[0].Open())
        try { [xml] $document = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $metadata = $document.SelectSingleNode('/*[local-name()="package"]/*[local-name()="metadata"]')
        Assert-ArtifactRule ($null -ne $metadata) "Missing archive metadata: $Path"
        $identity = $metadata.SelectSingleNode('*[local-name()="id"]')
        $versionNode = $metadata.SelectSingleNode('*[local-name()="version"]')
        $repository = $metadata.SelectSingleNode('*[local-name()="repository"]')
        Assert-ArtifactRule ($null -ne $identity -and $identity.InnerText -ceq $Id) "Archive ID mismatch: $Path"
        Assert-ArtifactRule ($null -ne $versionNode -and $versionNode.InnerText -ceq $Version) "Archive version mismatch: $Path"
        Assert-ArtifactRule ($null -ne $repository -and $repository.GetAttribute('commit') -ceq $Revision) "Archive revision mismatch: $Path"
        $payloads = @($archive.Entries | Where-Object FullName -Match '\.(dll|pdb)$')
        Assert-ArtifactRule ($payloads.Count -gt 0) "Archive contains no tested binary payload: $Path"
        foreach ($entry in $payloads) {
            # Strip only known NuGet payload roots, preserving culture/subdirectory identity.
            $match = [regex]::Match($entry.FullName, '\A(?:lib/[A-Za-z0-9.-]+/|analyzers/dotnet/cs/)(?<relative>[^\\]+)\z')
            Assert-ArtifactRule ($match.Success) "Unsupported archive payload root: $($entry.FullName)"
            $relative = $match.Groups['relative'].Value
            $segments = $relative.Split('/')
            Assert-ArtifactRule (-not ($segments -contains '' -or $segments -contains '.' -or $segments -contains '..')) "Invalid archive payload path: $($entry.FullName)"
            Assert-ArtifactRule ($relative -cin $ExpectedPayloads.Keys) "Archive contains an unattested payload: $relative"
            $stream = $entry.Open()
            try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)).ToLowerInvariant() }
            finally { $stream.Dispose() }
            Assert-ArtifactRule ($hash -ceq $ExpectedPayloads[$relative]) "Archive payload differs from tested build: $relative"
        }
    } finally { $archive.Dispose() }
}

function New-PackageArtifactManifest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $PackageDirectory,
        [Parameter(Mandatory)][string] $DestinationDirectory,
        [Parameter(Mandatory)][object[]] $ExpectedPackages,
        # Package ID -> path relative to captured TargetDir -> SHA256, including private dependencies/satellites.
        [Parameter(Mandatory)][hashtable] $ExpectedPayloads,
        [Parameter(Mandatory)][string] $Revision,
        [Parameter(Mandatory)][string] $BuildManifestPath,
        [Parameter(Mandatory)][string] $CoverageManifestPath,
        [Parameter(Mandatory)][string] $SdkPath
    )
    Assert-ArtifactRule ($Revision -cmatch '\A[0-9a-f]{40}\z') 'Invalid artifact revision.'
    Assert-ArtifactRule ($ExpectedPackages.Count -gt 0) 'Expected package inventory is empty.'
    Assert-ArtifactRule (-not (Test-Path -LiteralPath $DestinationDirectory)) 'Artifact destination must be fresh.'
    $expectedFiles = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $ids = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($package in $ExpectedPackages) {
        Assert-ArtifactRule ($package.id -cmatch '\Avc\.Ifx(?:\.[A-Za-z0-9]+)*\z') 'Invalid expected package ID.'
        Assert-ArtifactRule ($package.version -cmatch '\A[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?\z') 'Invalid expected package version.'
        Assert-ArtifactRule ($ids.Add($package.id)) 'Duplicate expected package ID.'
        Assert-ArtifactRule ($ExpectedPayloads.ContainsKey($package.id) -and $ExpectedPayloads[$package.id] -is [hashtable] -and
            $ExpectedPayloads[$package.id].Count -gt 0) 'Missing tested payload inventory for package.'
        foreach ($extension in @('nupkg', 'snupkg')) { $null = $expectedFiles.Add("$($package.id).$($package.version).$extension") }
    }
    $files = @(Get-ChildItem -LiteralPath $PackageDirectory -Force)
    Assert-ArtifactRule ($files.Count -eq $expectedFiles.Count) 'Package directory does not match the exact expected archive inventory.'
    foreach ($file in $files) {
        Assert-ArtifactRule (-not $file.PSIsContainer -and -not ($file.Attributes -band [IO.FileAttributes]::ReparsePoint) -and $expectedFiles.Contains($file.Name)) 'Unexpected package directory entry.'
    }
    $buildHash = Get-ArtifactHash $BuildManifestPath
    $coverageHash = Get-ArtifactHash $CoverageManifestPath
    $sdkHash = Get-ArtifactHash $SdkPath
    $packages = @(foreach ($package in ($ExpectedPackages | Sort-Object id -CaseSensitive)) {
        $entries = @{}
        foreach ($kind in @('package', 'symbols')) {
            $extension = if ($kind -eq 'package') { 'nupkg' } else { 'snupkg' }
            $file = "$($package.id).$($package.version).$extension"
            $path = Join-Path $PackageDirectory $file
            Assert-ArchiveIdentity -Path $path -Id $package.id -Version $package.version -Revision $Revision -ExpectedPayloads $ExpectedPayloads[$package.id]
            $entries[$kind] = [ordered]@{ file = $file; sha256 = Get-ArtifactHash $path }
        }
        [ordered]@{ id = $package.id; version = $package.version; package = $entries.package; symbols = $entries.symbols }
    })
    $manifest = [ordered]@{
        schemaVersion = 1; revision = $Revision; buildManifestSha256 = $buildHash; coverageManifestSha256 = $coverageHash
        sdk = [ordered]@{ file = 'global.json'; sha256 = $sdkHash }; packages = $packages
    }
    $null = New-Item -ItemType Directory -Path $DestinationDirectory
    foreach ($file in $files) { Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $DestinationDirectory $file.Name) }
    Copy-Item -LiteralPath $SdkPath -Destination (Join-Path $DestinationDirectory 'global.json')
    $manifestPath = Join-Path $DestinationDirectory 'package-manifest.json'
    $manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $manifestPath -Encoding utf8
    $hash = Get-ArtifactHash $manifestPath
    $null = Test-PackageArtifactManifest -Directory $DestinationDirectory -Revision $Revision -ManifestSha256 $hash
    return [pscustomobject]@{ Directory = [IO.Path]::GetFullPath($DestinationDirectory); ManifestPath = $manifestPath; ManifestSha256 = $hash; PackageCount = $packages.Count }
}

function Test-PackageArtifactManifest {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Directory, [Parameter(Mandatory)][string] $Revision,
        [Parameter(Mandatory)][string] $ManifestSha256)
    # Keep this verifier self-contained: the publish job executes this body inline, never downloaded scripts.
    $ErrorActionPreference = 'Stop'
    Set-StrictMode -Version Latest
    $manifestPath = Join-Path $Directory 'package-manifest.json'
    if ($ManifestSha256 -cnotmatch '\A[0-9a-f]{64}\z' -or
        (Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash.ToLowerInvariant() -cne $ManifestSha256) { throw 'Package manifest hash mismatch.' }
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    if ($manifest.schemaVersion -ne 1 -or $manifest.revision -cne $Revision -or $Revision -cnotmatch '\A[0-9a-f]{40}\z' -or
        $manifest.buildManifestSha256 -cnotmatch '\A[0-9a-f]{64}\z' -or $manifest.coverageManifestSha256 -cnotmatch '\A[0-9a-f]{64}\z') { throw 'Invalid package manifest provenance.' }
    if ($manifest.packages.Count -eq 0 -or $manifest.sdk.file -cne 'global.json') { throw 'Empty or invalid artifact inventory.' }
    $expected = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $ids = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $null = $expected.Add('package-manifest.json')
    $null = $expected.Add('global.json')
    $entries = @($manifest.sdk)
    foreach ($package in $manifest.packages) {
        if ($package.id -cnotmatch '\Avc\.Ifx(?:\.[A-Za-z0-9]+)*\z' -or -not $ids.Add($package.id) -or
            $package.version -cnotmatch '\A[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?\z') { throw 'Invalid or duplicate package identity.' }
        if ($package.package.file -cne "$($package.id).$($package.version).nupkg" -or
            $package.symbols.file -cne "$($package.id).$($package.version).snupkg") { throw 'Archive name does not match package identity.' }
        foreach ($entry in @($package.package, $package.symbols)) {
            if (-not $expected.Add($entry.file)) { throw 'Duplicate artifact filename.' }
            $entries += $entry
        }
    }
    $files = @(Get-ChildItem -LiteralPath $Directory -Force)
    if ($files.Count -ne $expected.Count) { throw 'Artifact file inventory mismatch.' }
    foreach ($file in $files) {
        if ($file.PSIsContainer -or ($file.Attributes -band [IO.FileAttributes]::ReparsePoint) -or -not $expected.Contains($file.Name)) { throw 'Unexpected artifact entry.' }
    }
    foreach ($entry in $entries) {
        if ($entry.sha256 -cnotmatch '\A[0-9a-f]{64}\z' -or
            (Get-FileHash -LiteralPath (Join-Path $Directory $entry.file) -Algorithm SHA256).Hash.ToLowerInvariant() -cne $entry.sha256) { throw 'Artifact hash mismatch.' }
    }
    return $manifest
}

Export-ModuleMember -Function New-PackageArtifactManifest, Test-PackageArtifactManifest
