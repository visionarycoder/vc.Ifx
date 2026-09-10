[CmdletBinding()]
param(
    [string] $RepositoryRoot = (Split-Path $PSScriptRoot -Parent),
    [switch] $SelfTest
)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($RepositoryRoot)
if ($SelfTest) {
    # Synthetic XML graphs exercise the validator without building or editing package projects.
    $artifacts = Join-Path $root "TestResults/dependency-audit/$([Guid]::NewGuid().ToString('N'))"
    $cases = @(
        @{ Name = 'valid'; Projects = @{ 'src/Core/Core.csproj' = ''; 'tests/Test/Test.csproj' = '<ProjectReference Include="../../src/Core/Core.csproj" />'; 'performance/Bench/Bench.csproj' = ''; 'scripts/reporting/Host.csproj' = '<ProjectReference Include="../../src/Core/Core.csproj" />' }; Error = $null },
        @{ Name = 'unlisted-host'; Projects = @{ 'src/Core/Core.csproj' = ''; 'scripts/reporting/Host.csproj' = '' }; Omit = 'scripts/reporting/Host.csproj'; Error = 'Project missing from solution' },
        @{ Name = 'host-cycle'; Projects = @{ 'src/Core/Core.csproj' = ''; 'scripts/One/One.csproj' = '<ProjectReference Include="../Two/Two.csproj" />'; 'scripts/Two/Two.csproj' = '<ProjectReference Include="../One/One.csproj" />' }; Error = 'Project dependency cycle' },
        @{ Name = 'duplicate-entry'; Projects = @{ 'src/Core/Core.csproj' = '' }; Duplicate = 'src/Core/Core.csproj'; Error = 'Duplicate solution project' },
        @{ Name = 'missing-reference'; Projects = @{ 'src/Core/Core.csproj' = '<ProjectReference Include="../Missing/Missing.csproj" />' }; Error = 'Missing reference' },
        @{ Name = 'reverse-aggregator'; Projects = @{ 'src/Core/Core.csproj' = '<ProjectReference Include="../vc.Ifx/vc.Ifx.csproj" />'; 'src/vc.Ifx/vc.Ifx.csproj' = '' }; Error = 'depends on the aggregator' },
        @{ Name = 'abstraction-implementation'; Projects = @{ 'src/Core.Abstractions/Core.Abstractions.csproj' = '<ProjectReference Include="../Core/Core.csproj" />'; 'src/Core/Core.csproj' = '' }; Error = 'Implementation reference in' },
        @{ Name = 'abstraction-provider'; Projects = @{ 'src/Core.Abstractions/Core.Abstractions.csproj' = '<PackageReference Include="Azure.Identity" />' }; Error = 'Provider dependency in' },
        @{ Name = 'abstraction-host'; Projects = @{ 'src/Core.Abstractions/Core.Abstractions.csproj' = '<FrameworkReference Include="Microsoft.AspNetCore.App" />' }; Error = 'Host framework in' },
        @{ Name = 'roslyn-runtime'; Projects = @{ 'src/vc.Ifx.Generators/vc.Ifx.Generators.csproj' = '<ProjectReference Include="../Core/Core.csproj" />'; 'src/Core/Core.csproj' = '' }; Error = 'Compiler-host runtime reference' },
        @{ Name = 'runtime-roslyn'; Projects = @{ 'src/Core/Core.csproj' = '<ProjectReference Include="../vc.Ifx.Analyzers/vc.Ifx.Analyzers.csproj" />'; 'src/vc.Ifx.Analyzers/vc.Ifx.Analyzers.csproj' = '' }; Error = 'Runtime library references compiler tooling' },
        @{ Name = 'missing-solution-project'; Projects = @{ 'src/Core/Core.csproj' = '' }; Extra = 'scripts/Missing.csproj'; Error = 'Solution references missing project' },
        @{ Name = 'dynamic-reference'; Projects = @{ 'src/Core/Core.csproj' = '<ProjectReference Include="$(Unknown)/Other.csproj" />' }; Error = 'Unevaluated project reference' },
        @{ Name = 'source-to-host'; Projects = @{ 'src/Core/Core.csproj' = '<ProjectReference Include="../../scripts/Host.csproj" />'; 'scripts/Host.csproj' = '' }; Error = 'Source library references a non-library project' },
        @{ Name = 'empty-inventory'; Projects = @{}; NoDocs = $true; Error = 'No projects discovered' },
        @{ Name = 'abstraction-to-abstraction'; Projects = @{ 'src/One.Abstractions/One.Abstractions.csproj' = '<ProjectReference Include="../Two.Abstractions/Two.Abstractions.csproj" />'; 'src/Two.Abstractions/Two.Abstractions.csproj' = '' }; Error = $null },
        @{ Name = 'missing-docs'; Projects = @{ 'src/Core/Core.csproj' = '' }; NoDocs = $true; Error = 'Exactly one documentation project' },
        @{ Name = 'unlisted-docs'; Projects = @{ 'src/Core/Core.csproj' = '' }; Omit = 'docs/docs.csproj'; Error = 'Project missing from solution' },
        @{ Name = 'docs-as-file'; Projects = @{ 'src/Core/Core.csproj' = '' }; DocsAsFile = $true; Error = 'Project missing from solution' },
        @{ Name = 'packable-docs'; Projects = @{ 'src/Core/Core.csproj' = '' }; DocsProperties = '<IsPackable>true</IsPackable>'; Error = 'Documentation project must explicitly declare unconditional IsPackable=false' },
        @{ Name = 'missing-docs-packability'; Projects = @{ 'src/Core/Core.csproj' = '' }; DocsProperties = ''; Error = 'Documentation project must explicitly declare unconditional IsPackable=false' },
        @{ Name = 'conditional-docs-packability'; Projects = @{ 'src/Core/Core.csproj' = '' }; DocsProperties = '<IsPackable Condition="false">false</IsPackable>'; Error = 'Documentation project must explicitly declare unconditional IsPackable=false' },
        @{ Name = 'extra-docs-project'; Projects = @{ 'src/Core/Core.csproj' = ''; 'docs/Extra.csproj' = '' }; Error = 'Exactly one documentation project' },
        @{ Name = 'source-to-docs'; Projects = @{ 'src/Core/Core.csproj' = '<ProjectReference Include="../../docs/docs.csproj" />' }; Error = 'Source library references a non-library project' },
        @{ Name = 'docs-host-cycle'; Projects = @{ 'src/Core/Core.csproj' = ''; 'docs/docs.csproj' = '<ProjectReference Include="../scripts/Host.csproj" />'; 'scripts/Host.csproj' = '<ProjectReference Include="../docs/docs.csproj" />' }; Error = 'Project dependency cycle' }
    )
    foreach ($case in $cases) {
        if (-not $case.NoDocs -and -not $case.Projects.ContainsKey('docs/docs.csproj')) {
            $case.Projects['docs/docs.csproj'] = ''
        }
        $fixture = Join-Path $artifacts $case.Name
        $null = [IO.Directory]::CreateDirectory($fixture)
        $entries = [Collections.Generic.List[string]]::new()
        foreach ($path in $case.Projects.Keys) {
            $fullPath = Join-Path $fixture $path
            $null = [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($fullPath))
            $properties = if ($path -eq 'docs/docs.csproj') {
                if ($case.ContainsKey('DocsProperties')) { $case.DocsProperties } else { '<IsPackable>false</IsPackable>' }
            } else { '' }
            [IO.File]::WriteAllText($fullPath, "<Project><PropertyGroup>$properties</PropertyGroup><ItemGroup>$($case.Projects[$path])</ItemGroup></Project>")
            if ($path -eq 'docs/docs.csproj' -and $case.DocsAsFile) { $entries.Add("<File Path=`"$path`" />") }
            elseif ($path -ne $case.Omit) { $entries.Add("<Project Path=`"$path`" />") }
        }
        foreach ($path in @($case.Duplicate, $case.Extra)) {
            if ($path) { $entries.Add("<Project Path=`"$path`" />") }
        }
        [IO.File]::WriteAllText((Join-Path $fixture 'vc.Ifx.slnx'), "<Solution>$($entries -join '')</Solution>")
        $failure = $null
        try { $null = & $PSCommandPath -RepositoryRoot $fixture }
        catch { $failure = $_.Exception.Message }
        if (($null -eq $case.Error -and $failure) -or ($case.Error -and ($null -eq $failure -or -not $failure.Contains($case.Error)))) {
            throw "Probe '$($case.Name)' expected '$($case.Error)', got '$failure'. Fixtures: $artifacts"
        }
    }
    Write-Output "Dependency validator probes passed: $($cases.Count). Fixtures: $artifacts"
    return
}

function Resolve-ProjectPath([string] $BasePath, [string] $RelativePath) {
    return [IO.Path]::GetFullPath((Join-Path $BasePath $RelativePath.Replace('\', '/')))
}

[xml] $solution = Get-Content -LiteralPath (Join-Path $root 'vc.Ifx.slnx') -Raw
$listed = @($solution.SelectNodes('//Project') | ForEach-Object {
    Resolve-ProjectPath $root $_.GetAttribute('Path')
})
$directories = @('src', 'tests', 'performance', 'scripts', 'docs' | ForEach-Object { Join-Path $root $_ } | Where-Object { Test-Path -LiteralPath $_ -PathType Container })
$projects = @(foreach ($directory in $directories) {
    Get-ChildItem -LiteralPath $directory -Recurse -Filter '*.csproj' -File |
        Where-Object { $_.FullName -notmatch '[\\/](obj|bin)[\\/]' }
})
$failures = [Collections.Generic.List[string]]::new()
$graph = @{}
$compilerHosts = @('vc.Ifx.Roslyn', 'vc.Ifx.Analyzers', 'vc.Ifx.CodeFixes', 'vc.Ifx.Generators')
$sourceRoot = (Join-Path $root 'src') + [IO.Path]::DirectorySeparatorChar
$docsRoot = (Join-Path $root 'docs') + [IO.Path]::DirectorySeparatorChar
$docsPath = Join-Path $root 'docs/docs.csproj'
$docsProjects = @($projects | Where-Object { $_.FullName.StartsWith($docsRoot, [StringComparison]::OrdinalIgnoreCase) })
if ($projects.Count -eq 0) { $failures.Add('No projects discovered in the audited inventory.') }
if ($docsProjects.Count -ne 1 -or $docsProjects[0].FullName -ne $docsPath) {
    $failures.Add('Exactly one documentation project is required, at docs/docs.csproj.')
}

foreach ($group in @($listed | Group-Object | Where-Object Count -gt 1)) {
    $failures.Add("Duplicate solution project: $($group.Name)")
}

foreach ($project in $projects) {
    if ($project.FullName -notin $listed) {
        $failures.Add("Project missing from solution: $($project.FullName)")
    }
    [xml] $document = Get-Content -LiteralPath $project.FullName -Raw
    if ($project.FullName -eq $docsPath) {
        $packability = @($document.SelectNodes('/Project/PropertyGroup/IsPackable'))
        if ($packability.Count -ne 1 -or $packability[0].InnerText.Trim() -ine 'false' -or
            $packability[0].HasAttribute('Condition') -or $packability[0].ParentNode.HasAttribute('Condition')) {
            $failures.Add('Documentation project must explicitly declare unconditional IsPackable=false.')
        }
    }
    $references = @($document.SelectNodes('//ProjectReference[@Include]') | ForEach-Object {
        $include = $_.GetAttribute('Include')
        if ($include -match '\$\(|[*?;]') {
            $failures.Add("Unevaluated project reference in $($project.BaseName): $include. Evaluate explicitly before accepting this boundary.")
        }
        else { Resolve-ProjectPath $project.DirectoryName $include }
    })
    $graph[$project.FullName] = $references
    foreach ($reference in $references) {
        if (-not (Test-Path -LiteralPath $reference -PathType Leaf)) {
            $failures.Add("Missing reference from $($project.BaseName): $reference")
        }
        elseif ($reference -notin $projects.FullName) {
            $failures.Add("Reference outside audited project graph from $($project.BaseName): $reference")
        }
        $targetName = [IO.Path]::GetFileNameWithoutExtension($reference)
        $isSource = $project.FullName.StartsWith($sourceRoot, [StringComparison]::OrdinalIgnoreCase)
        if ($isSource -and -not $reference.StartsWith($sourceRoot, [StringComparison]::OrdinalIgnoreCase)) {
            $failures.Add("Source library references a non-library project: $($project.BaseName) -> $reference")
        }
        if ($isSource -and
            [IO.Path]::GetFileName($reference) -eq 'vc.Ifx.csproj') {
            $failures.Add("Library $($project.BaseName) depends on the aggregator.")
        }
        if ($project.BaseName -like '*.Abstractions' -and $targetName -notlike '*.Abstractions') {
            $failures.Add("Implementation reference in $($project.BaseName): $targetName")
        }
        if ($project.BaseName -in $compilerHosts -and $targetName -notin $compilerHosts) {
            $failures.Add("Compiler-host runtime reference from $($project.BaseName): $targetName")
        }
        if ($isSource -and $project.BaseName -notin $compilerHosts -and $targetName -in $compilerHosts) {
            $failures.Add("Runtime library references compiler tooling: $($project.BaseName) -> $targetName")
        }
    }
    if ($project.BaseName -like '*.Abstractions') {
        foreach ($package in $document.SelectNodes('//PackageReference')) {
            if ($package.GetAttribute('Include') -match '^(Azure\.|FluentFTP|Microsoft\.EntityFrameworkCore|Microsoft\.AspNetCore)') {
                $failures.Add("Provider dependency in $($project.BaseName): $($package.GetAttribute('Include'))")
            }
        }
        foreach ($framework in $document.SelectNodes('//FrameworkReference')) {
            if ($framework.GetAttribute('Include') -eq 'Microsoft.AspNetCore.App') {
                $failures.Add("Host framework in $($project.BaseName): Microsoft.AspNetCore.App")
            }
        }
    }
}

foreach ($entry in $listed) {
    if (-not (Test-Path -LiteralPath $entry -PathType Leaf)) {
        $failures.Add("Solution references missing project: $entry")
    }
    elseif ($entry -notin $projects.FullName) {
        $failures.Add("Solution project outside audited src/tests/performance/scripts/docs inventory: $entry")
    }
}

$visiting = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$visited = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
function Test-DependencyPath([string] $ProjectPath) {
    if ($visited.Contains($ProjectPath)) { return }
    if (-not $visiting.Add($ProjectPath)) {
        $failures.Add("Project dependency cycle reaches: $ProjectPath")
        return
    }
    foreach ($reference in $graph[$ProjectPath]) {
        if ($graph.ContainsKey($reference)) { Test-DependencyPath $reference }
    }
    $null = $visiting.Remove($ProjectPath)
    $null = $visited.Add($ProjectPath)
}
foreach ($projectPath in @($graph.Keys)) { Test-DependencyPath $projectPath }

if ($failures.Count -gt 0) {
    throw ($failures -join [Environment]::NewLine)
}
$libraries = @($projects | Where-Object { $_.FullName.StartsWith($sourceRoot, [StringComparison]::OrdinalIgnoreCase) }).Count
Write-Output "Dependency audit passed: $($projects.Count) declared projects ($libraries source libraries), including script hosts and one non-packable docs project; exact solution inventory, references present, no cycles or forbidden abstraction/compiler/aggregator edges. Static XML audit only: imported/generated references, effective packability and evaluated NuGet assets require integration verification."
