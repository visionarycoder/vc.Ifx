# Master Migration Orchestration Script
# Guides you through the complete domain contracts migration

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   Domain Contracts Migration Wizard" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will guide you through the complete migration process." -ForegroundColor Yellow
Write-Host ""

# Check prerequisites
Write-Host "Checking Prerequisites..." -ForegroundColor Green
Write-Host "-----------------------------------" -ForegroundColor Green

$basePath = "C:\Dev\VisionaryCoder\App.Framework\main"

if (-not (Test-Path $basePath)) {
    Write-Host "✗ ERROR: Workspace not found at $basePath" -ForegroundColor Red
    exit 1
}

Set-Location $basePath

# Check Git status
try {
    $gitStatus = git status --porcelain 2>&1
    if ($gitStatus) {
        Write-Host "⚠ WARNING: You have uncommitted changes" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Uncommitted files:" -ForegroundColor Yellow
        Write-Host $gitStatus
        Write-Host ""
        $continue = Read-Host "Continue anyway? (yes/no)"
        if ($continue -ne "yes") {
            Write-Host "Exiting. Please commit your changes first." -ForegroundColor Red
            exit 0
        }
    }
    else {
        Write-Host "  ✓ Git working directory is clean" -ForegroundColor Green
    }
}
catch {
    Write-Host "  ⊘ Git check skipped (not a git repository or git not installed)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Migration Phases:" -ForegroundColor Cyan
Write-Host "-----------------------------------" -ForegroundColor Cyan
Write-Host "  Phase 1: Rename Folders (remove VisionaryCoder prefix)" -ForegroundColor White
Write-Host "  Phase 2: Create Contracts Projects" -ForegroundColor White
Write-Host "  Phase 3: Move Files to Contracts" -ForegroundColor White
Write-Host "  Phase 4: Update Project References" -ForegroundColor White
Write-Host "  Phase 5: Update Namespaces" -ForegroundColor White
Write-Host "  Phase 6: Clean Up Old Files" -ForegroundColor White
Write-Host "  Phase 7: Build and Test" -ForegroundColor White
Write-Host "  Phase 8: Generate READMEs" -ForegroundColor White
Write-Host ""
Write-Host "⏱ Estimated Time: 4-6 hours" -ForegroundColor Yellow
Write-Host ""

$proceed = Read-Host "Ready to begin? (yes/no)"
if ($proceed -ne "yes") {
    Write-Host "Exiting. Run this script again when ready." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Phase 1: Rename Folders" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠ IMPORTANT: Close Visual Studio before proceeding!" -ForegroundColor Yellow
Write-Host ""
$phase1 = Read-Host "Run Phase 1? (yes/no/skip)"

if ($phase1 -eq "yes") {
    & ".\scripts\Phase1-Rename-Folders.ps1"
    Write-Host ""
    $checkPhase1 = Read-Host "Did Phase 1 complete successfully? (yes/no)"
    if ($checkPhase1 -ne "yes") {
        Write-Host "Please fix Phase 1 errors before continuing." -ForegroundColor Red
        exit 1
    }
}
elseif ($phase1 -eq "skip") {
    Write-Host "  ⊘ Phase 1 skipped" -ForegroundColor Gray
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Phase 2-4: Create Contracts Projects" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This phase requires manual steps. Please follow:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Create Framework.Resilience.Contracts project" -ForegroundColor White
Write-Host "2. Create Framework.Observability.Contracts project" -ForegroundColor White
Write-Host "3. Create Framework.Messaging.Contracts project" -ForegroundColor White
Write-Host "4. Create Framework.Security.Contracts project" -ForegroundColor White
Write-Host ""
Write-Host "Refer to MIGRATION-IMPLEMENTATION-GUIDE.md for detailed steps." -ForegroundColor Cyan
Write-Host ""
$phase2 = Read-Host "Have you completed Phases 2-4? (yes/no/skip)"

if ($phase2 -ne "yes" -and $phase2 -ne "skip") {
    Write-Host "Please complete Phases 2-4 before continuing." -ForegroundColor Red
    exit 0
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Phase 5: Update Namespaces" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
$phase5 = Read-Host "Run Phase 5? (yes/no/skip)"

if ($phase5 -eq "yes") {
    & ".\scripts\Phase5-Update-Namespaces.ps1"
    Write-Host ""
    $checkPhase5 = Read-Host "Did Phase 5 complete successfully? (yes/no)"
    if ($checkPhase5 -ne "yes") {
        Write-Host "Please fix Phase 5 errors before continuing." -ForegroundColor Red
        exit 1
    }
}
elseif ($phase5 -eq "skip") {
    Write-Host "  ⊘ Phase 5 skipped" -ForegroundColor Gray
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Phase 7: Build and Test" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Building solution..." -ForegroundColor Green

try {
    dotnet clean
    dotnet build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Build successful!" -ForegroundColor Green
        
        Write-Host ""
        Write-Host "Running tests..." -ForegroundColor Green
        dotnet test --no-build
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ All tests passed!" -ForegroundColor Green
        }
        else {
            Write-Host "  ✗ Some tests failed" -ForegroundColor Red
            Write-Host "  Review test failures before proceeding" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "  ✗ Build failed" -ForegroundColor Red
        Write-Host "  Review build errors before proceeding" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "  ✗ Build/Test error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Phase 8: Generate READMEs" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
$phase8 = Read-Host "Run Phase 8? (yes/no/skip)"

if ($phase8 -eq "yes") {
    & ".\scripts\Phase8-Generate-READMEs.ps1"
}
elseif ($phase8 -eq "skip") {
    Write-Host "  ⊘ Phase 8 skipped" -ForegroundColor Gray
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Migration Complete!" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Final Steps:" -ForegroundColor Yellow
Write-Host "1. Review all changes" -ForegroundColor White
Write-Host "2. Run full test suite" -ForegroundColor White
Write-Host "3. Verify 100% code coverage" -ForegroundColor White
Write-Host "4. Commit changes to Git" -ForegroundColor White
Write-Host ""
Write-Host "Recommended Git commit message:" -ForegroundColor Cyan
Write-Host "-----------------------------------" -ForegroundColor Cyan
Write-Host @"
feat: Migrate to domain-specific Contracts architecture

- Remove VisionaryCoder prefix from folder names
- Create domain-specific Contracts projects (Resilience, Observability, Messaging, Security)
- Move domain-specific abstractions from Framework.Abstractions to domain Contracts
- Update all namespace references across solution
- Generate comprehensive README files for all packages
- Maintain 100% test coverage throughout migration

BREAKING CHANGE: Namespace changes require consuming projects to update using statements
- VisionaryCoder.Framework.Abstractions.Proxy -> VisionaryCoder.Framework.Resilience.Contracts.Proxy
- VisionaryCoder.Framework.Abstractions.Secrets -> VisionaryCoder.Framework.Security.Contracts.Secrets
- VisionaryCoder.Framework.Abstractions.Messaging -> VisionaryCoder.Framework.Messaging.Contracts

Build status: [X] errors resolved, all tests passing
Coverage: 100%
"@ -ForegroundColor Gray
Write-Host ""
