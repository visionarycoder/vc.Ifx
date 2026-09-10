#Requires -Version 7.0
Set-StrictMode -Version Latest

function ConvertTo-IfxCanonicalSetting([string] $Value) {
    return [string]::Join(';', @($Value -split '[;\r\n]+' | Where-Object { $_ -ne '' }))
}

function Read-IfxFileIdentities([string] $Path) {
    $files = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
    foreach ($line in Get-Content -LiteralPath $Path) {
        $parts = $line.Split('|', 2)
        if ($parts.Count -ne 2 -or $parts[0] -cnotmatch '^[A-Fa-f0-9]{64}$' -or -not [IO.Path]::IsPathRooted($parts[1])) { throw "Invalid build file identity: $Path" }
        $file = [IO.Path]::GetFullPath($parts[1])
        $hash = $parts[0].ToLowerInvariant()
        if ($files.ContainsKey($file) -and $files[$file].sha256 -cne $hash) { throw "Conflicting build file identity: $file" }
        $files[$file] = [pscustomobject]@{ path = $file; sha256 = $hash }
    }
    return @($files.Values | Sort-Object path)
}

function Test-IfxRecordedFile([object] $File) {
    if (-not (Test-Path -LiteralPath $File.path -PathType Leaf)) { throw "Missing build input/output: $($File.path)" }
    $actual = (Get-FileHash -LiteralPath $File.path -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actual -cne $File.sha256) { throw "Changed build input/output: $($File.path)" }
}

function Complete-IfxBuildProvenance {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $BuildDirectory, [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][string] $Revision, [Parameter(Mandatory)][string] $Configuration,
        [Parameter(Mandatory)][object] $Selection)
    $path = Join-Path $BuildDirectory 'build-identity.json'
    if (Test-Path -LiteralPath $path) { throw 'Build identity already exists; capture a fresh build.' }
    $projects = @(foreach ($directory in Get-ChildItem -LiteralPath (Join-Path $BuildDirectory 'build-inventory') -Directory | Sort-Object Name) {
        $settings = [ordered]@{}
        foreach ($file in Get-ChildItem -LiteralPath (Join-Path $directory.FullName 'settings') -File | Sort-Object Name) {
            $settings[$file.BaseName] = ConvertTo-IfxCanonicalSetting (Get-Content -LiteralPath $file.FullName -Raw)
        }
        if (-not $settings['ProjectPath'] -or -not $settings['TargetPath'] -or $settings['Configuration'] -cne $Configuration) { throw "Incomplete build settings: $directory" }
        $inputs = @(Read-IfxFileIdentities (Join-Path $directory.FullName 'inputs.txt'))
        $outputs = @(Read-IfxFileIdentities (Join-Path $directory.FullName 'outputs.txt'))
        $outputPaths = @($outputs | ForEach-Object path)
        if ($inputs.Count -eq 0 -or $settings['TargetPath'] -cnotin $outputPaths) { throw "Missing compiled assembly: $directory" }
        if ($settings['DebugType'] -in @('portable','full','pdbonly') -and [IO.Path]::ChangeExtension($settings['TargetPath'], '.pdb') -cnotin $outputPaths) { throw "Missing compiled PDB: $directory" }
        [pscustomobject]@{
            name = $directory.Name; projectPath = $settings['ProjectPath']; targetPath = $settings['TargetPath']
            isTestProject = $settings['IsTestProject'] -eq 'true'; settings = $settings
            compilePaths = @(Get-Content -LiteralPath (Join-Path $directory.FullName 'compile.txt') | Sort-Object -Unique)
            nonCodeInputPaths = @(Get-Content -LiteralPath (Join-Path $directory.FullName 'non-code-inputs.txt') | Sort-Object -Unique)
            projectReferences = @(Get-Content -LiteralPath (Join-Path $directory.FullName 'references.txt') | Sort-Object -Unique)
            absentInputs = @(Get-Content -LiteralPath (Join-Path $directory.FullName 'absent.txt') | Sort-Object -Unique)
            inputs = $inputs; outputs = $outputs
        }
    })
    if ($projects.Count -eq 0) { throw 'No compiled project identities captured.' }
    foreach ($project in $projects) {
        foreach ($file in @($project.inputs) + @($project.outputs)) { Test-IfxRecordedFile $file }
        foreach ($absent in $project.absentInputs) { if (Test-Path -LiteralPath $absent) { throw "Added build input: $absent" } }
    }
    # Test output copies must be the exact freshly compiled framework assemblies.
    $payloads = @{}
    foreach ($project in $projects) {
        foreach ($file in $project.outputs | Where-Object { $_.path -ceq $project.targetPath -or $_.path -ceq [IO.Path]::ChangeExtension($project.targetPath, '.pdb') }) {
            $payloads[[IO.Path]::GetFileName($file.path)] = $file.sha256
        }
    }
    foreach ($project in $projects) {
        foreach ($file in $project.outputs) {
            $name = [IO.Path]::GetFileName($file.path)
            if ($payloads.ContainsKey($name) -and $payloads[$name] -cne $file.sha256) { throw "Copied assembly differs from fresh build: $($file.path)" }
        }
    }
    $document = [ordered]@{ schemaVersion = 1; repositoryRoot = [IO.Path]::GetFullPath($RepositoryRoot); revision = $Revision
        configuration = $Configuration; testSelection = $Selection; projects = $projects }
    $document | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $path -Encoding utf8
    return [pscustomobject]@{ path = [IO.Path]::GetFullPath($path); sha256 = (Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant() }
}

function Assert-IfxBuildProvenance {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $BuildDirectory, [string] $CoverageRunDirectory,
        [string[]] $RequiredProject = @(), [string] $Configuration, [object] $Selection)
    $path = Join-Path $BuildDirectory 'build-identity.json'
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw 'Missing build identity; capture a fresh build before NoBuild.' }
    $document = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    $hash = (Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant()
    $buildCompletion = Get-Content -LiteralPath (Join-Path $BuildDirectory 'build-complete.json') -Raw | ConvertFrom-Json
    if ($buildCompletion.buildIdentity.sha256 -cne $hash) { throw 'Build identity does not match completed build.' }
    if ($document.schemaVersion -ne 1 -or @($document.projects).Count -eq 0) { throw 'Unsupported or empty build identity.' }
    if ($Configuration -and $document.configuration -cne $Configuration) { throw 'Build identity configuration mismatch.' }
    if ($null -ne $Selection -and $document.testSelection.Fingerprint -cne $Selection.Fingerprint) { throw 'Build identity test selection mismatch.' }
    $revision = & git -C $document.repositoryRoot rev-parse HEAD
    if ($LASTEXITCODE -ne 0 -or $revision -cne $document.revision) { throw 'Build identity revision mismatch.' }
    foreach ($required in $RequiredProject) {
        if ([IO.Path]::GetFullPath($required) -cnotin @($document.projects | ForEach-Object projectPath)) { throw "Project was not compiled in this build identity: $required" }
    }
    foreach ($project in $document.projects) {
        $outputPaths = @($project.outputs | ForEach-Object path)
        if ($project.targetPath -cnotin $outputPaths -or @($project.inputs).Count -eq 0) { throw "Incomplete project identity: $($project.name)" }
        if ($project.settings.DebugType -in @('portable','full','pdbonly') -and [IO.Path]::ChangeExtension($project.targetPath, '.pdb') -cnotin $outputPaths) { throw "Missing compiled PDB identity: $($project.name)" }
        foreach ($file in @($project.inputs) + @($project.outputs)) { Test-IfxRecordedFile $file }
        foreach ($absent in $project.absentInputs) { if (Test-Path -LiteralPath $absent) { throw "Added build input: $absent" } }
        $currentOutputPaths = @(Get-ChildItem -LiteralPath $project.settings.TargetDir -Recurse -File -Force | ForEach-Object FullName | Sort-Object -Unique)
        if (@(Compare-Object @($outputPaths | Sort-Object -Unique) $currentOutputPaths -CaseSensitive).Count) { throw "Changed build output inventory: $($project.name)" }
        & {
            $arguments = @('msbuild', $project.projectPath, '-t:GetIfxBuildSettings', '-getItem:IfxBuildSetting,Compile,IfxDeclaredProjectReference,EmbeddedResource,Protobuf', '-m:1', '-p:BuildInParallel=false',
                "-p:Configuration=$($document.configuration)") + @($document.testSelection.Arguments)
            $json = & dotnet @arguments
            if ($LASTEXITCODE -ne 0) { throw "Cannot evaluate current build settings: $($project.name)" }
            $current = $json | ConvertFrom-Json
            foreach ($setting in $current.Items.IfxBuildSetting) {
                if ((ConvertTo-IfxCanonicalSetting $setting.Value) -cne $project.settings.($setting.Identity)) { throw "Changed build setting $($setting.Identity): $($project.name)" }
            }
            $recordedCompile = @($project.compilePaths | Where-Object { $_ -notmatch '[/\\](obj|bin)[/\\]' } | Sort-Object -Unique)
            $currentCompile = @($current.Items.Compile | ForEach-Object FullPath | Where-Object { $_ -notmatch '[/\\](obj|bin)[/\\]' } | Sort-Object -Unique)
            if (@(Compare-Object $recordedCompile $currentCompile -CaseSensitive).Count) { throw "Changed compile source inventory: $($project.name)" }
            $currentNonCode = @(@($current.Items.EmbeddedResource) + @($current.Items.Protobuf) | ForEach-Object FullPath | Sort-Object -Unique)
            if (@(Compare-Object @($project.nonCodeInputPaths | Sort-Object -Unique) $currentNonCode -CaseSensitive).Count) { throw "Changed non-code input inventory: $($project.name)" }
            if (@(Compare-Object @($project.projectReferences | Sort-Object -Unique) @($current.Items.IfxDeclaredProjectReference | ForEach-Object FullPath | Sort-Object -Unique) -CaseSensitive).Count) { throw "Changed project reference inventory: $($project.name)" }
        }
    }
    if ($CoverageRunDirectory) {
        $context = Get-Content -LiteralPath (Join-Path $CoverageRunDirectory 'run-context.json') -Raw | ConvertFrom-Json
        $consumed = Join-Path $CoverageRunDirectory 'consumed-build-identity.json'
        if (-not $context.noBuild -or $context.buildIdentity.sha256 -cne $hash -or
            -not (Test-Path -LiteralPath $consumed -PathType Leaf) -or (Get-FileHash -LiteralPath $consumed).Hash.ToLowerInvariant() -cne $hash) {
            throw 'Coverage is not bound to this tested build identity.'
        }
        $completion = Get-Content -LiteralPath (Join-Path $CoverageRunDirectory 'build-identity-verified.json') -Raw | ConvertFrom-Json
        if ($completion.sha256 -cne $hash -or -not $completion.restoredOutputsVerified) { throw 'Coverage did not verify restored build outputs.' }
    }
    return [pscustomobject]@{ path = [IO.Path]::GetFullPath($path); sha256 = $hash; manifest = $document }
}

Export-ModuleMember -Function Complete-IfxBuildProvenance, Assert-IfxBuildProvenance
