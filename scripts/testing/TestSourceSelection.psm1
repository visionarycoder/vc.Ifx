#Requires -Version 7.0
Set-StrictMode -Version Latest

function Resolve-IfxTestSelection {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $ProjectPath, [string] $SourceScope, [string] $TestPackage)

    if ($TestPackage -and $TestPackage -cnotmatch '^vc\.Ifx(?:\.[A-Za-z0-9]+)*$') { throw 'Invalid TestPackage.' }
    if ($TestPackage -and -not $SourceScope) { throw 'TestPackage requires TestSourceScope; full tests retain all references.' }
    $scopes = [Collections.Generic.SortedSet[string]]::new([StringComparer]::Ordinal)
    $directory = [IO.Path]::GetDirectoryName([IO.Path]::GetFullPath($ProjectPath))
    if ($SourceScope) {
        if ([IO.Path]::GetFileName($ProjectPath) -cnotin @('vc.Ifx.UnitTests.csproj', 'vc.Ifx.IntegrationTests.csproj')) {
            throw 'Test source selection requires a centralized test project.'
        }
        foreach ($raw in $SourceScope.Split(';')) {
            $scope = $raw.Trim().Replace('\', '/')
            if ($scope -cnotmatch '^[A-Za-z0-9][A-Za-z0-9_.-]*(/[A-Za-z0-9][A-Za-z0-9_.-]*)*$' -or
                $scope -match '(^|/)(bin|obj)(/|$)') { throw "Invalid literal test source scope: $raw" }
            $path = $directory
            foreach ($segment in $scope.Split('/')) {
                $path = Join-Path $path $segment
                if (-not (Test-Path -LiteralPath $path)) { throw "Test source scope does not exist: $scope" }
                $item = Get-Item -LiteralPath $path -Force
                if ($item.Name -cne $segment) { throw "Use exact path casing for test source scope: $scope" }
                if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Symbolic links are not allowed in test source scopes: $scope" }
            }
            if ($item.PSIsContainer) {
                if ($scope.EndsWith('.cs', [StringComparison]::OrdinalIgnoreCase)) { throw 'A .cs selector must name a root source file, not a directory.' }
                if (@(Get-ChildItem -LiteralPath $path -Recurse -Force | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint }).Count) {
                    throw "Symbolic links are not allowed within test source scopes: $scope"
                }
            }
            elseif ($scope.Contains('/') -or -not $scope.EndsWith('.cs', [StringComparison]::Ordinal)) {
                throw 'Literal file selectors must name a root .cs test file.'
            }
            $null = $scopes.Add($scope)
        }
        if ($TestPackage) {
            [xml] $project = Get-Content -LiteralPath $ProjectPath -Raw
            $references = @($project.SelectNodes('//ProjectReference') | ForEach-Object { [IO.Path]::GetFileNameWithoutExtension($_.Include.Replace('\', '/')) })
            if ($TestPackage -cnotin $references) { throw "No direct project reference matches TestPackage=$TestPackage." }
        }
    }
    $canonical = [string]::Join(';', $scopes)
    $key = ''
    $fingerprint = ''
    if ($scopes.Count) {
        $fingerprint = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData(
            [Text.Encoding]::UTF8.GetBytes("ifx-test-selection-v1`n$canonical`n$TestPackage"))).ToLowerInvariant()
        $prefix = if ($scopes.Count -eq 1 -and -not $canonical.EndsWith('.cs', [StringComparison]::Ordinal)) { "$canonical/selection-" } else { 'multi-' }
        $key = $prefix + $fingerprint.Substring(0, 32)
    }
    [pscustomobject]@{
        Version = 1; SourceScope = $canonical; Scopes = @($scopes); TestPackage = $TestPackage
        OutputKey = $key; Fingerprint = $fingerprint
        Arguments = @("-p:IfxTestSourceScope=$($canonical.Replace(';', '%3B'))", "-p:IfxTestPackage=$TestPackage", "-p:IfxTestOutputKey=$key")
    }
}

function Assert-IfxTestSelectionContext {
    [CmdletBinding()]
    param([Parameter(Mandatory)][object] $Context, [switch] $RequireFull)
    $property = $Context.PSObject.Properties['testSelection']
    if ($null -eq $property) { return } # Historical evidence uses the original top-level scope fields.
    $selection = $property.Value
    $package = $Context.PSObject.Properties['testPackage']
    $expectedPackage = if ($null -eq $package) { '' } else { [string]$package.Value }
    if ($selection.Version -ne 1 -or $selection.SourceScope -cne [string]$Context.testSourceScope -or
        $selection.TestPackage -cne $expectedPackage -or
        [string]::Join(';', [string[]]$selection.Scopes) -cne $selection.SourceScope) {
        throw 'Test selection provenance does not match its context.'
    }
    if ($RequireFull -and ($selection.SourceScope -or $selection.TestPackage -or $selection.OutputKey -or $selection.Fingerprint)) {
        throw 'Full reporting forbids scoped test selection provenance.'
    }
    $expected = ''
    $expectedKey = ''
    if ($selection.SourceScope) {
        $expected = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData(
            [Text.Encoding]::UTF8.GetBytes("ifx-test-selection-v1`n$($selection.SourceScope)`n$expectedPackage"))).ToLowerInvariant()
        $prefix = if ($selection.Scopes.Count -eq 1 -and -not $selection.SourceScope.EndsWith('.cs', [StringComparison]::Ordinal)) { "$($selection.SourceScope)/selection-" } else { 'multi-' }
        $expectedKey = $prefix + $expected.Substring(0, 32)
    }
    if ($selection.Fingerprint -cne $expected) { throw 'Test selection fingerprint mismatch.' }
    if ($selection.OutputKey -cne $expectedKey) { throw 'Test selection output key mismatch.' }
}

Export-ModuleMember -Function Resolve-IfxTestSelection, Assert-IfxTestSelectionContext
