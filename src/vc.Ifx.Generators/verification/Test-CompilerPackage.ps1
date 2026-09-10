#Requires -Version 7.0
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$artifacts = Join-Path $repo "TestResults/generator-package/$([Guid]::NewGuid().ToString('N'))"
$null = New-Item -ItemType Directory -Path $artifacts

# The caller is PackageSmoke.proj under Invoke-FrameworkTests, holding its mutex.
foreach ($package in @('vc.Ifx.Generators.Abstractions', 'vc.Ifx.Generators')) {
    & dotnet pack (Join-Path $repo "src/$package/$package.csproj") -c Release -o $artifacts -m:1 -p:BuildInParallel=false -p:GeneratePackageOnBuild=false -warnaserror -v:minimal
    if ($LASTEXITCODE -ne 0) { throw "Failed to pack $package." }
    $archive = @(Get-ChildItem -LiteralPath $artifacts -Filter '*.nupkg' | Where-Object { $_.Name -match ('^' + [regex]::Escape($package) + '\.\d+\.\d+\.\d+.*\.nupkg$') })
    if ($archive.Count -ne 1) { throw "Expected exactly one $package archive." }
    [IO.Compression.ZipFile]::ExtractToDirectory($archive[0].FullName, (Join-Path $artifacts $package))
}

$compilerFiles = Join-Path $artifacts 'vc.Ifx.Generators/analyzers/dotnet/cs'
foreach ($required in @('vc.Ifx.Generators.dll', 'vc.Ifx.Roslyn.dll', 'Microsoft.AspNetCore.Routing.dll', 'Microsoft.AspNetCore.Routing.Abstractions.dll')) {
    if (-not (Test-Path -LiteralPath (Join-Path $compilerFiles $required))) { throw "Missing compiler dependency $required." }
}
if (Test-Path -LiteralPath (Join-Path $artifacts 'vc.Ifx.Generators/lib')) { throw 'Compiler package must not expose runtime lib assets.' }
if (Get-ChildItem -LiteralPath $compilerFiles -Filter 'Microsoft.CodeAnalysis*.dll') { throw 'Do not ship host Roslyn assemblies.' }
[xml] $nuspec = Get-Content -LiteralPath (Join-Path $artifacts 'vc.Ifx.Generators/vc.Ifx.Generators.nuspec') -Raw
if ($nuspec.SelectNodes('//*[local-name()="dependency"]').Count -ne 0) { throw 'Compiler dependencies must be private bundled assets.' }

$dotnetRoot = Split-Path (Get-Command dotnet).Source
$sdk = (& dotnet --version).Trim()
$compiler = Join-Path $dotnetRoot "sdk/$sdk/Roslyn/bincore/csc.dll"
$references = foreach ($pack in @('Microsoft.NETCore.App.Ref', 'Microsoft.AspNetCore.App.Ref')) {
    $version = Get-ChildItem -LiteralPath (Join-Path $dotnetRoot "packs/$pack") -Directory |
        Where-Object { $_.Name -match '^10\.\d+\.\d+$' } | Sort-Object { [version] $_.Name } -Descending | Select-Object -First 1
    Get-ChildItem -LiteralPath (Join-Path $version.FullName 'ref/net10.0') -Filter '*.dll'
}
$references += Get-Item -LiteralPath (Join-Path $artifacts 'vc.Ifx.Generators.Abstractions/lib/net10.0/vc.Ifx.Generators.Abstractions.dll')
$generated = Join-Path $artifacts 'generated'
$null = New-Item -ItemType Directory -Path $generated
$arguments = @('/nostdlib+', '/target:library', '/langversion:14', '/nullable:enable', '/warnaserror+', '/deterministic+',
    "/out:`"$artifacts/EndpointConsumer.dll`"", "/generatedfilesout:`"$generated`"")
$arguments += $references | ForEach-Object { "/reference:`"$($_.FullName)`"" }
# Give Roslyn every adjacent private DLL as a dependency location, as NuGet does.
$arguments += Get-ChildItem -LiteralPath $compilerFiles -Filter '*.dll' | ForEach-Object { "/analyzer:`"$($_.FullName)`"" }
$arguments += "`"$PSScriptRoot/EndpointConsumer.cs.txt`""
$response = Join-Path $artifacts 'compiler.rsp'
$arguments | Set-Content -LiteralPath $response -Encoding utf8
& dotnet exec $compiler /noconfig "@$response"
if ($LASTEXITCODE -ne 0) { throw 'The packaged generator failed real compiler consumer compilation.' }
$output = @(Get-ChildItem -LiteralPath $generated -Recurse -Filter 'IfxEndpointRouteBuilderExtensions.g.cs')
if ($output.Count -ne 1) { throw 'Missing generated endpoint registry.' }
if ((Get-Content -LiteralPath $output[0].FullName -Raw) -notmatch 'global::Handlers.Read') { throw 'Registry omitted the declared handler.' }
Write-Host "Compiler package verification passed: $artifacts"
