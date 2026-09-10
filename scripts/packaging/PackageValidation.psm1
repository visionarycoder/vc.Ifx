Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-PackageRule {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}

function Read-ArchiveText {
    param($Archive, [string] $Path)
    $entry = $Archive.GetEntry($Path)
    Assert-PackageRule ($null -ne $entry) "Missing archive entry: $Path"
    $reader = [IO.StreamReader]::new($entry.Open())
    try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
}

function Test-PackageArchive {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $Path,
        [Parameter(Mandatory)][string] $PackageId,
        [Parameter(Mandatory)][string] $ReadmePath,
        [string[]] $RequiredDependencies = @(),
        [string] $RepositoryUrl = 'https://github.com/visionarycoder/vc.Ifx'
    )

    $compilerExtensions = @('vc.Ifx.Analyzers', 'vc.Ifx.CodeFixes', 'vc.Ifx.Generators')
    $archive = [IO.Compression.ZipFile]::OpenRead([IO.Path]::GetFullPath($Path))
    try {
        [xml] $nuspec = Read-ArchiveText $archive "$PackageId.nuspec"
        $metadata = $nuspec.SelectSingleNode('/*[local-name()="package"]/*[local-name()="metadata"]')
        Assert-PackageRule ($null -ne $metadata) 'Missing nuspec metadata.'
        foreach ($name in @('id', 'version', 'authors', 'description', 'tags', 'readme', 'license', 'repository')) {
            $node = $metadata.SelectSingleNode("*[local-name()='$name']")
            Assert-PackageRule ($null -ne $node) "Missing nuspec $name."
            if ($name -ne 'repository') {
                Assert-PackageRule (-not [string]::IsNullOrWhiteSpace($node.InnerText)) "Empty nuspec $name."
            }
        }
        Assert-PackageRule ($metadata.id -eq $PackageId) 'Package identity differs from project name.'
        Assert-PackageRule ($metadata.readme -eq 'README.md') 'Expected package-root README.md.'
        Assert-PackageRule ((Read-ArchiveText $archive 'README.md') -ceq [IO.File]::ReadAllText($ReadmePath)) 'Package README differs from source README.'
        Assert-PackageRule ($metadata.license.type -eq 'expression' -and $metadata.license.InnerText -eq 'MIT') 'Expected MIT license expression.'
        Assert-PackageRule ($metadata.repository.type -eq 'git' -and $metadata.repository.url -eq $RepositoryUrl) 'Incorrect repository metadata.'
        Assert-PackageRule ($metadata.repository.GetAttribute('commit') -match '^[0-9a-f]{40}$') 'Missing repository commit.'

        $paths = @($archive.Entries.FullName)
        Assert-PackageRule (@($paths | Where-Object { $_ -match '(^|/)(obj|bin)/|(^|/)[^/]*(Tests|Benchmarks)[^/]*\.dll$' }).Count -eq 0) 'Build/test/benchmark output leaked into package.'
        $dependencies = @($metadata.SelectNodes('.//*[local-name()="dependency"]') | ForEach-Object { $_.id })
        Assert-PackageRule ($dependencies -notcontains 'Microsoft.CodeAnalysis.Analyzers') 'Build-only Roslyn analyzers leaked into public dependencies.'
        if ($metadata.version -notmatch '-') {
            foreach ($dependency in $metadata.SelectNodes('.//*[local-name()="dependency"]')) {
                Assert-PackageRule ($dependency.version -notmatch '-[0-9A-Za-z]') 'Stable package exposes a prerelease dependency.'
            }
        }
        if ($PackageId -ne 'vc.Ifx') {
            Assert-PackageRule ($dependencies -notcontains 'vc.Ifx') 'Aggregator dependency leaked into a small package.'
        }
        foreach ($dependency in $RequiredDependencies) {
            Assert-PackageRule ($dependencies -contains $dependency) "Missing public dependency: $dependency"
        }
        if ($PackageId -in $compilerExtensions) {
            Assert-PackageRule ($dependencies.Count -eq 0) 'Compiler extension exposes runtime package dependencies.'
            Assert-PackageRule (@($paths | Where-Object { $_ -match '^(lib|ref|runtimes)/' }).Count -eq 0) 'Compiler extension exposes runtime or reference assets.'
            Assert-PackageRule ($paths -contains "analyzers/dotnet/cs/$PackageId.dll") 'Compiler extension is not in analyzers/dotnet/cs.'
            Assert-PackageRule ($paths -contains 'analyzers/dotnet/cs/vc.Ifx.Roslyn.dll') 'Compiler extension is missing vc.Ifx.Roslyn dependency.'
            if ($PackageId -eq 'vc.Ifx.CodeFixes') {
                Assert-PackageRule (@($paths | Where-Object { [IO.Path]::GetFileName($_) -eq 'vc.Ifx.Analyzers.dll' }).Count -eq 0) 'Code fixes bundle vc.Ifx.Analyzers and duplicate diagnostic execution; install the separate analyzer package.'
            }
            Assert-PackageRule (@($paths | Where-Object { $_ -match '/Microsoft\.CodeAnalysis[^/]*\.dll$' }).Count -eq 0) 'Compiler extension bundles host Roslyn assemblies.'
        } else {
            $framework = if ($PackageId -eq 'vc.Ifx.Roslyn') { 'netstandard2.0' } else { 'net10.0' }
            Assert-PackageRule ($paths -contains "lib/$framework/$PackageId.dll") 'Missing framework library assembly.'
            Assert-PackageRule ($paths -contains "lib/$framework/$PackageId.xml") 'Missing XML API documentation.'
        }
    } finally { $archive.Dispose() }

    $symbolsPath = [IO.Path]::ChangeExtension($Path, '.snupkg')
    Assert-PackageRule (Test-Path -LiteralPath $symbolsPath) 'Missing snupkg symbol archive.'
    $symbols = [IO.Compression.ZipFile]::OpenRead([IO.Path]::GetFullPath($symbolsPath))
    try {
        $pdbs = @($symbols.Entries | Where-Object { $_.FullName -like "*/$PackageId.pdb" })
        Assert-PackageRule ($pdbs.Count -eq 1) 'Expected one package assembly PDB in symbols archive.'
        $stream = [IO.MemoryStream]::new()
        $entryStream = $pdbs[0].Open()
        try { $entryStream.CopyTo($stream) } finally { $entryStream.Dispose() }
        $stream.Position = 0
        $provider = [System.Reflection.Metadata.MetadataReaderProvider]::FromPortablePdbStream($stream, [System.Reflection.Metadata.MetadataStreamOptions]::Default, 0)
        try {
            $reader = $provider.GetMetadataReader([System.Reflection.Metadata.MetadataReaderOptions]::Default, $null)
            $sourceLink = @($reader.CustomDebugInformation | ForEach-Object {
                $info = $reader.GetCustomDebugInformation($_)
                if ($reader.GetGuid($info.Kind) -eq [guid]'CC110556-A091-4D38-9FEC-25AB9A351A6A') {
                    [Text.Encoding]::UTF8.GetString($reader.GetBlobBytes($info.Value)) | ConvertFrom-Json
                }
            })
            Assert-PackageRule ($sourceLink.Count -eq 1) 'Missing Source Link information in portable PDB.'
            Assert-PackageRule (@($sourceLink[0].documents.PSObject.Properties).Count -gt 0) 'Empty Source Link document map.'
            foreach ($document in $sourceLink[0].documents.PSObject.Properties) {
                Assert-PackageRule ($document.Value -match '^https://raw\.githubusercontent\.com/visionarycoder/vc\.Ifx/[0-9a-f]{40}/') 'Unexpected Source Link source URL.'
                Assert-PackageRule ($document.Value.Contains('/' + $metadata.repository.GetAttribute('commit') + '/')) 'Source Link commit differs from package repository commit.'
            }
        } finally { $provider.Dispose(); $stream.Dispose() }
    } finally { $symbols.Dispose() }
    [pscustomobject]@{ PackageId = $PackageId; Archive = $Path; Symbols = $symbolsPath; SourceLink = $true }
}

Export-ModuleMember -Function Test-PackageArchive, Assert-PackageRule
