# Phase 1: Complete Folder Rename Script
# Run this script AFTER closing Visual Studio

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Phase 1: Folder Rename Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "PREREQUISITES:" -ForegroundColor Yellow
Write-Host "1. Close Visual Studio" -ForegroundColor Yellow
Write-Host "2. Stop any running build processes" -ForegroundColor Yellow
Write-Host "3. Commit any pending changes to Git" -ForegroundColor Yellow
Write-Host ""

$continue = Read-Host "Have you completed the prerequisites? (yes/no)"
if ($continue -ne "yes") {
    Write-Host "Exiting. Please complete prerequisites and run again." -ForegroundColor Red
    exit
}

# Set base paths
$srcPath = "C:\Dev\VisionaryCoder\App.Framework\main\src"
$testPath = "C:\Dev\VisionaryCoder\App.Framework\main\tests"
$slnPath = "C:\Dev\VisionaryCoder\App.Framework\main\Framework.sln"

Write-Host ""
Write-Host "Step 1: Renaming Source Folders..." -ForegroundColor Green
Write-Host "-----------------------------------" -ForegroundColor Green

$srcFolders = @(
    "VisionaryCoder.Framework.Abstractions",
    "VisionaryCoder.Framework.Core",
    "VisionaryCoder.Framework.Patterns",
    "VisionaryCoder.Framework.DataAccess",
    "VisionaryCoder.Framework.EntityFrameworkCore",
    "VisionaryCoder.Framework.Messaging",
    "VisionaryCoder.Framework.Storage",
    "VisionaryCoder.Framework.Observability",
    "VisionaryCoder.Framework.Resilience",
    "VisionaryCoder.Framework.Security",
    "VisionaryCoder.Framework.Identity"
)

foreach ($folder in $srcFolders) {
    $oldPath = Join-Path $srcPath $folder
    $newName = $folder -replace "VisionaryCoder\.", ""
    $newPath = Join-Path $srcPath $newName
    
    if (Test-Path $oldPath) {
        try {
            Rename-Item -Path $oldPath -NewName $newName -ErrorAction Stop
            Write-Host "  ✓ Renamed: $folder → $newName" -ForegroundColor Green
        }
        catch {
            Write-Host "  ✗ FAILED: $folder - $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "    Try closing all programs and running again" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "  ⊘ Skipped: $folder (not found)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Step 2: Renaming Test Folders..." -ForegroundColor Green
Write-Host "-----------------------------------" -ForegroundColor Green

$testFolders = @(
    "VisionaryCoder.Framework.Tests",
    "VisionaryCoder.Framework.Abstractions.Tests",
    "VisionaryCoder.Framework.Core.Tests",
    "VisionaryCoder.Framework.Patterns.Tests",
    "VisionaryCoder.Framework.DataAccess.Tests",
    "VisionaryCoder.Framework.EntityFrameworkCore.Tests",
    "VisionaryCoder.Framework.Messaging.Tests",
    "VisionaryCoder.Framework.Observability.Tests",
    "VisionaryCoder.Framework.Resilience.Tests",
    "VisionaryCoder.Framework.Security.Tests"
)

foreach ($folder in $testFolders) {
    $oldPath = Join-Path $testPath $folder
    $newName = $folder -replace "VisionaryCoder\.", ""
    $newPath = Join-Path $testPath $newName
    
    if (Test-Path $oldPath) {
        try {
            Rename-Item -Path $oldPath -NewName $newName -ErrorAction Stop
            Write-Host "  ✓ Renamed: $folder → $newName" -ForegroundColor Green
        }
        catch {
            Write-Host "  ✗ FAILED: $folder - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "  ⊘ Skipped: $folder (not found)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Step 3: Updating Solution File..." -ForegroundColor Green
Write-Host "-----------------------------------" -ForegroundColor Green

if (Test-Path $slnPath) {
    try {
        # Backup solution file
        $backupPath = "$slnPath.backup"
        Copy-Item $slnPath $backupPath
        Write-Host "  ✓ Created backup: Framework.sln.backup" -ForegroundColor Green
        
        # Update all paths in solution file
        $content = Get-Content $slnPath -Raw
        $updated = $content -replace 'VisionaryCoder\.Framework\.', 'Framework.'
        Set-Content $slnPath $updated -NoNewline
        
        Write-Host "  ✓ Updated all project paths in solution file" -ForegroundColor Green
    }
    catch {
        Write-Host "  ✗ FAILED to update solution file: $($_.Exception.Message)" -ForegroundColor Red
        # Restore backup if update failed
        if (Test-Path $backupPath) {
            Copy-Item $backupPath $slnPath -Force
            Write-Host "  ✓ Restored solution file from backup" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "Step 4: Verifying Solution..." -ForegroundColor Green
Write-Host "-----------------------------------" -ForegroundColor Green

try {
    Set-Location "C:\Dev\VisionaryCoder\App.Framework\main"
    $projects = dotnet sln list 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        $projectCount = ($projects | Measure-Object -Line).Lines - 2 # Exclude header lines
        Write-Host "  ✓ Solution loads successfully" -ForegroundColor Green
        Write-Host "  ✓ Found $projectCount projects" -ForegroundColor Green
    }
    else {
        Write-Host "  ✗ Solution has errors" -ForegroundColor Red
        Write-Host $projects -ForegroundColor Yellow
    }
}
catch {
    Write-Host "  ✗ Could not verify solution: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Phase 1 Complete!" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Open Visual Studio and verify solution loads" -ForegroundColor White
Write-Host "2. Build solution to check for errors" -ForegroundColor White
Write-Host "3. Run Phase 2 script to create Contracts projects" -ForegroundColor White
Write-Host ""
Write-Host "If there were errors, restore from backup:" -ForegroundColor Yellow
Write-Host "  Copy-Item Framework.sln.backup Framework.sln -Force" -ForegroundColor Gray
Write-Host ""
