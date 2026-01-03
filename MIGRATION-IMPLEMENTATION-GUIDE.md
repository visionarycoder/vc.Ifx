# Domain Contracts Migration Implementation Guide

**Execution Time:** 4-6 hours  
**Risk Level:** 🟡 MEDIUM (extensive refactoring, but reversible)  
**Prerequisites:** Clean working directory, all changes committed

---

## 🚨 CRITICAL: Pre-Migration Checklist

Before starting, ensure:

- [ ] All current work is committed to Git
- [ ] Create a backup branch: `git checkout -b backup-before-contracts-migration`
- [ ] Create working branch: `git checkout -b feature/domain-contracts-migration`
- [ ] Document current test pass rate (should be 100%)
- [ ] Note current build error count (currently 303)

---

## Phase 1: Folder Renaming (Remove VisionaryCoder Prefix)

### Step 1.1: Rename Source Folders

```powershell
# Navigate to src directory
cd C:\Dev\VisionaryCoder\App.Framework\main\src

# Rename all framework folders
Rename-Item "VisionaryCoder.Framework.Abstractions" "Framework.Abstractions"
Rename-Item "VisionaryCoder.Framework.Core" "Framework.Core"
Rename-Item "VisionaryCoder.Framework.Patterns" "Framework.Patterns"
Rename-Item "VisionaryCoder.Framework.DataAccess" "Framework.DataAccess"
Rename-Item "VisionaryCoder.Framework.EntityFrameworkCore" "Framework.EntityFrameworkCore"
Rename-Item "VisionaryCoder.Framework.Messaging" "Framework.Messaging"
Rename-Item "VisionaryCoder.Framework.Storage" "Framework.Storage"
Rename-Item "VisionaryCoder.Framework.Observability" "Framework.Observability"
Rename-Item "VisionaryCoder.Framework.Resilience" "Framework.Resilience"
Rename-Item "VisionaryCoder.Framework.Security" "Framework.Security"
Rename-Item "VisionaryCoder.Framework.Identity" "Framework.Identity"
```

### Step 1.2: Rename Test Folders

```powershell
# Navigate to tests directory
cd C:\Dev\VisionaryCoder\App.Framework\main\tests

# Rename all test folders
Rename-Item "VisionaryCoder.Framework.Tests" "Framework.Tests"
Rename-Item "VisionaryCoder.Framework.Abstractions.Tests" "Framework.Abstractions.Tests"
Rename-Item "VisionaryCoder.Framework.Core.Tests" "Framework.Core.Tests"
Rename-Item "VisionaryCoder.Framework.Patterns.Tests" "Framework.Patterns.Tests"
Rename-Item "VisionaryCoder.Framework.DataAccess.Tests" "Framework.DataAccess.Tests"
Rename-Item "VisionaryCoder.Framework.EntityFrameworkCore.Tests" "Framework.EntityFrameworkCore.Tests"
Rename-Item "VisionaryCoder.Framework.Messaging.Tests" "Framework.Messaging.Tests"
Rename-Item "VisionaryCoder.Framework.Observability.Tests" "Framework.Observability.Tests"
Rename-Item "VisionaryCoder.Framework.Resilience.Tests" "Framework.Resilience.Tests"
Rename-Item "VisionaryCoder.Framework.Security.Tests" "Framework.Security.Tests"
```

### Step 1.3: Update Solution File

```powershell
# Edit Framework.sln to update all project paths
# Replace: VisionaryCoder.Framework. → Framework.
(Get-Content Framework.sln) -replace 'VisionaryCoder\.Framework\.', 'Framework.' | Set-Content Framework.sln
```

### Step 1.4: Verify Solution Loads

```powershell
# Test solution loads correctly
dotnet sln Framework.sln list
```

---

## Phase 2: Create Contracts Projects

### Step 2.1: Create Framework.Resilience.Contracts

```powershell
cd C:\Dev\VisionaryCoder\App.Framework\main\src

# Create project
dotnet new classlib -n Framework.Resilience.Contracts -f net10.0

# Move to project directory
cd Framework.Resilience.Contracts

# Remove default Class1.cs
Remove-Item Class1.cs

# Create directory structure
New-Item -ItemType Directory -Path "Proxy"
New-Item -ItemType Directory -Path "Proxy\Exceptions"
New-Item -ItemType Directory -Path "Pipeline"
```

#### Update Framework.Resilience.Contracts.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <RootNamespace>VisionaryCoder.Framework.Resilience.Contracts</RootNamespace>
    
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
    
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest-all</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    
    <PackageId>VisionaryCoder.Framework.Resilience.Contracts</PackageId>
    <Version>1.0.0</Version>
    <Authors>VisionaryCoder</Authors>
    <Company>VisionaryCoder</Company>
    <Product>VisionaryCoder Framework Resilience Contracts</Product>
    <Description>Contract interfaces for VisionaryCoder Framework Resilience package including proxy patterns, interceptors, and pipeline abstractions.</Description>
    <PackageTags>framework;resilience;contracts;proxy;interceptor;pipeline;net10</PackageTags>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/visionarycoder/Framework</RepositoryUrl>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Framework.Abstractions\Framework.Abstractions.csproj" />
  </ItemGroup>

  <ItemGroup Label="Development">
    <PackageReference Include="Microsoft.SourceLink.GitHub" PrivateAssets="All" />
  </ItemGroup>
</Project>
```

### Step 2.2: Create Framework.Observability.Contracts

```powershell
cd C:\Dev\VisionaryCoder\App.Framework\main\src

dotnet new classlib -n Framework.Observability.Contracts -f net10.0
cd Framework.Observability.Contracts
Remove-Item Class1.cs
New-Item -ItemType Directory -Path "Tracing"
```

#### Update Framework.Observability.Contracts.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <RootNamespace>VisionaryCoder.Framework.Observability.Contracts</RootNamespace>
    
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
    
    <PackageId>VisionaryCoder.Framework.Observability.Contracts</PackageId>
    <Version>1.0.0</Version>
    <Authors>VisionaryCoder</Authors>
    <Description>Contract interfaces for VisionaryCoder Framework Observability package including tracing abstractions.</Description>
    <PackageTags>framework;observability;contracts;tracing;opentelemetry;net10</PackageTags>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Framework.Abstractions\Framework.Abstractions.csproj" />
  </ItemGroup>
</Project>
```

### Step 2.3: Create Framework.Messaging.Contracts

```powershell
cd C:\Dev\VisionaryCoder\App.Framework\main\src

dotnet new classlib -n Framework.Messaging.Contracts -f net10.0
cd Framework.Messaging.Contracts
Remove-Item Class1.cs
```

#### Update Framework.Messaging.Contracts.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>VisionaryCoder.Framework.Messaging.Contracts</RootNamespace>
    
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>VisionaryCoder.Framework.Messaging.Contracts</PackageId>
    <Version>1.0.0</Version>
    <Description>Contract interfaces for VisionaryCoder Framework Messaging package.</Description>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Framework.Abstractions\Framework.Abstractions.csproj" />
  </ItemGroup>
</Project>
```

### Step 2.4: Create Framework.Security.Contracts

```powershell
cd C:\Dev\VisionaryCoder\App.Framework\main\src

dotnet new classlib -n Framework.Security.Contracts -f net10.0
cd Framework.Security.Contracts
Remove-Item Class1.cs
New-Item -ItemType Directory -Path "Secrets"
```

#### Update Framework.Security.Contracts.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>VisionaryCoder.Framework.Security.Contracts</RootNamespace>
    
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>VisionaryCoder.Framework.Security.Contracts</PackageId>
    <Version>1.0.0</Version>
    <Description>Contract interfaces for VisionaryCoder Framework Security package.</Description>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Framework.Abstractions\Framework.Abstractions.csproj" />
  </ItemGroup>
</Project>
```

### Step 2.5: Add Projects to Solution

```powershell
cd C:\Dev\VisionaryCoder\App.Framework\main

dotnet sln add src/Framework.Resilience.Contracts/Framework.Resilience.Contracts.csproj
dotnet sln add src/Framework.Observability.Contracts/Framework.Observability.Contracts.csproj
dotnet sln add src/Framework.Messaging.Contracts/Framework.Messaging.Contracts.csproj
dotnet sln add src/Framework.Security.Contracts/Framework.Security.Contracts.csproj
```

---

## Phase 3: Move Files to Contracts Projects

### Step 3.1: Move Resilience Contracts

Due to the extensive nature of this migration, I recommend using a PowerShell script:

```powershell
# Move Proxy files
$source = "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Abstractions\Proxy"
$dest = "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Resilience.Contracts\Proxy"

Copy-Item "$source\IProxyInterceptor.cs" $dest
Copy-Item "$source\IOrderedProxyInterceptor.cs" $dest
Copy-Item "$source\ProxyContext.cs" $dest
Copy-Item "$source\ProxyResponse.cs" $dest
Copy-Item "$source\ProxyDelegate.cs" $dest
Copy-Item "$source\IProxyPipeline.cs" $dest
Copy-Item "$source\IProxyTransport.cs" $dest

# Move Proxy Exceptions
Copy-Item "$source\Exceptions\ProxyException.cs" "$dest\Exceptions\"
Copy-Item "$source\Exceptions\TransientProxyException.cs" "$dest\Exceptions\"
Copy-Item "$source\Exceptions\RetryableTransportException.cs" "$dest\Exceptions\"

# Move Pipeline files
Copy-Item "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Abstractions\Pipeline\IRequest.cs" `
  "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Resilience.Contracts\Pipeline\"
Copy-Item "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Abstractions\Pipeline\IInterceptor.cs" `
  "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Resilience.Contracts\Pipeline\"
```

### Step 3.2: Update Namespaces in Moved Files

For each moved file in Resilience.Contracts, update:

```csharp
// OLD
namespace VisionaryCoder.Framework.Abstractions.Proxy;

// NEW
namespace VisionaryCoder.Framework.Resilience.Contracts.Proxy;
```

### Step 3.3: Move Observability Contracts

```powershell
Copy-Item "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Abstractions\Pipeline\ISpan.cs" `
  "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Observability.Contracts\Tracing\"
```

Update namespace to: `VisionaryCoder.Framework.Observability.Contracts.Tracing`

### Step 3.4: Move Messaging Contracts

```powershell
$source = "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Abstractions\Messaging"
$dest = "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Messaging.Contracts"

Copy-Item "$source\IMessage.cs" $dest
Copy-Item "$source\IMessagePublisher.cs" $dest
Copy-Item "$source\IMessageConsumer.cs" $dest
Copy-Item "$source\IMessageBus.cs" $dest
```

Update namespaces to: `VisionaryCoder.Framework.Messaging.Contracts`

### Step 3.5: Move Security Contracts

```powershell
Copy-Item "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Abstractions\Secrets\ISecretProvider.cs" `
  "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Security.Contracts\Secrets\"
```

Update namespace to: `VisionaryCoder.Framework.Security.Contracts.Secrets`

---

## Phase 4: Update Project References

### Step 4.1: Update Framework.Resilience

Add to Framework.Resilience.csproj:

```xml
<ItemGroup Label="Project References">
  <ProjectReference Include="..\Framework.Abstractions\Framework.Abstractions.csproj" />
  <ProjectReference Include="..\Framework.Patterns\Framework.Patterns.csproj" />
  <ProjectReference Include="..\Framework.Resilience.Contracts\Framework.Resilience.Contracts.csproj" />
</ItemGroup>
```

### Step 4.2: Update Framework.Observability

```xml
<ItemGroup Label="Project References">
  <ProjectReference Include="..\Framework.Abstractions\Framework.Abstractions.csproj" />
  <ProjectReference Include="..\Framework.Patterns\Framework.Patterns.csproj" />
  <ProjectReference Include="..\Framework.Observability.Contracts\Framework.Observability.Contracts.csproj" />
  <ProjectReference Include="..\Framework.Resilience.Contracts\Framework.Resilience.Contracts.csproj" />
</ItemGroup>
```

### Step 4.3: Update Framework.Security

```xml
<ItemGroup Label="Project References">
  <ProjectReference Include="..\Framework.Abstractions\Framework.Abstractions.csproj" />
  <ProjectReference Include="..\Framework.Patterns\Framework.Patterns.csproj" />
  <ProjectReference Include="..\Framework.Security.Contracts\Framework.Security.Contracts.csproj" />
  <ProjectReference Include="..\Framework.Resilience.Contracts\Framework.Resilience.Contracts.csproj" />
</ItemGroup>
```

### Step 4.4: Update Framework.Messaging

```xml
<ItemGroup Label="Project References">
  <ProjectReference Include="..\Framework.Abstractions\Framework.Abstractions.csproj" />
  <ProjectReference Include="..\Framework.Patterns\Framework.Patterns.csproj" />
  <ProjectReference Include="..\Framework.Messaging.Contracts\Framework.Messaging.Contracts.csproj" />
</ItemGroup>
```

---

## Phase 5: Mass Namespace Update Script

Create a PowerShell script to update all using statements:

```powershell
# update-namespaces.ps1

$replacements = @{
    'using VisionaryCoder.Framework.Abstractions.Proxy;' = 'using VisionaryCoder.Framework.Resilience.Contracts.Proxy;'
    'using VisionaryCoder.Framework.Abstractions.Proxy.Exceptions;' = 'using VisionaryCoder.Framework.Resilience.Contracts.Proxy.Exceptions;'
    'using VisionaryCoder.Framework.Abstractions.Pipeline;' = 'using VisionaryCoder.Framework.Resilience.Contracts.Pipeline;'
    'using VisionaryCoder.Framework.Abstractions.Secrets;' = 'using VisionaryCoder.Framework.Security.Contracts.Secrets;'
    'using VisionaryCoder.Framework.Abstractions.Messaging;' = 'using VisionaryCoder.Framework.Messaging.Contracts;'
}

$paths = @(
    "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Observability",
    "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Resilience",
    "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Security"
)

foreach ($path in $paths) {
    $files = Get-ChildItem -Path $path -Filter "*.cs" -Recurse
    
    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw
        $modified = $false
        
        foreach ($old in $replacements.Keys) {
            if ($content -match [regex]::Escape($old)) {
                $content = $content -replace [regex]::Escape($old), $replacements[$old]
                $modified = $true
            }
        }
        
        if ($modified) {
            Set-Content $file.FullName $content -NoNewline
            Write-Host "Updated: $($file.FullName)" -ForegroundColor Green
        }
    }
}
```

Run: `.\update-namespaces.ps1`

---

## Phase 6: Clean Up Old Files

After confirming the build works:

```powershell
# Remove moved files from Abstractions
Remove-Item "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Abstractions\Proxy" -Recurse -Force
Remove-Item "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Abstractions\Pipeline" -Recurse -Force
Remove-Item "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Abstractions\Secrets" -Recurse -Force
Remove-Item "C:\Dev\VisionaryCoder\App.Framework\main\src\Framework.Abstractions\Messaging" -Recurse -Force
```

---

## Phase 7: Build and Validate

```powershell
# Clean and rebuild
dotnet clean
dotnet build

# Run tests
dotnet test --no-build

# Check code coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## Phase 8: Generate README Files

Due to length constraints, I'll create a separate script for README generation.

---

## ✅ Validation Checklist

- [ ] All 4 Contracts projects created and build successfully
- [ ] All 17 files moved with correct namespaces
- [ ] All project references updated
- [ ] All using statements updated
- [ ] Solution builds with 0 errors (or fewer than 303)
- [ ] All tests pass
- [ ] Code coverage remains at 100%
- [ ] README.md files created for each package
- [ ] Git commit created with descriptive message

---

## 🚨 Rollback Procedure

If issues arise:

```powershell
git checkout backup-before-contracts-migration
git branch -D feature/domain-contracts-migration
```

---

**Estimated Total Time:** 4-6 hours  
**Next Steps:** Execute Phase 1, validate, then proceed to Phase 2

