#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Generates PublicAPI.Shipped.txt and PublicAPI.Unshipped.txt files automatically

.DESCRIPTION
    Builds projects and extracts RS0016 (missing public API) diagnostics to generate
    PublicAPI.Unshipped.txt files. This handles Razor components, source generators,
    and all public APIs correctly.

    Run this script after adding new public APIs to update the PublicAPI.Unshipped.txt files.
    During release, use the -Shipped flag to move Unshipped APIs to Shipped.

.PARAMETER Shipped
    Move Unshipped APIs to Shipped (for release)

.PARAMETER Configuration
    Build configuration (Debug/Release)

.PARAMETER Project
    Specific project to update (optional). If not specified, updates all projects with PublicApiAnalyzers.

.EXAMPLE
    ./scripts/auto-publicapi.ps1
    Generates PublicAPI.Unshipped.txt for all projects

.EXAMPLE
    ./scripts/auto-publicapi.ps1 -Project CdCSharp.BlazorUI.Core
    Generates only for the specified project

.EXAMPLE
    ./scripts/auto-publicapi.ps1 -Shipped
    Moves Unshipped APIs to Shipped (for release)

.NOTES
    This script captures all RS0016 diagnostics from the build output. Due to build output
    limitations, it may need to be run multiple times to capture all APIs.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$Shipped,
    
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter()]
    [string]$Project
)

$ErrorActionPreference = "Stop"

# ============ UTILITIES ============

function Write-Step($msg) { Write-Host "`n=== $msg ===" -ForegroundColor Magenta }
function Write-Success($msg) { Write-Host "✅ $msg" -ForegroundColor Green }
function Write-Warning($msg) { Write-Host "⚠️  $msg" -ForegroundColor Yellow }
function Write-Info($msg) { Write-Host "ℹ️  $msg" -ForegroundColor Cyan }
function Write-Error($msg) { Write-Host "❌ $msg" -ForegroundColor Red }

# ============ API EXTRACTION FROM BUILD ============

function Get-PublicApiFromBuild {
    param([string]$ProjectPath, [string]$ProjectDir)
    
    Write-Info "Building with analyzers (this may take a while)..."
    
    Push-Location $ProjectDir
    try {
        # Build with analyzers enabled - capture all diagnostics
        $output = & dotnet build -c $Configuration --no-incremental `
            -p:TreatWarningsAsErrors=false `
            -p:RunAnalyzersDuringBuild=true `
            2>&1
        
        # Parse RS0016 diagnostics from build output
        # Format: "error RS0016: Symbol 'X' is not part of the declared API"
        $apis = [System.Collections.Generic.List[string]]::new()
        
        foreach ($line in $output) {
            $lineStr = $line.ToString()
            
            # Match RS0016 errors - extract the symbol
            if ($lineStr -match "RS0016.*Symbol '([^']+)' is not part") {
                $symbol = $Matches[1]
                $apis.Add($symbol)
            }
        }
        
        return $apis | Sort-Object -Unique
    }
    finally {
        Pop-Location
    }
}

# ============ PROJECT PROCESSING ============

function Update-Project {
    param([string]$ProjectPath, [switch]$MoveToShipped)
    
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath)
    $projectDir = [System.IO.Path]::GetDirectoryName($ProjectPath)
    
    Write-Step "Processing: $projectName"
    
    $shippedPath = Join-Path $projectDir "PublicAPI.Shipped.txt"
    $unshippedPath = Join-Path $projectDir "PublicAPI.Unshipped.txt"
    
    # Ensure files exist
    if (-not (Test-Path $shippedPath)) { "" | Set-Content $shippedPath -Encoding UTF8 }
    if (-not (Test-Path $unshippedPath)) { "" | Set-Content $unshippedPath -Encoding UTF8 }
    
    if ($MoveToShipped) {
        $unshipped = Get-Content $unshippedPath -Raw -ErrorAction SilentlyContinue
        if ($unshipped -and $unshipped.Trim()) {
            $shipped = if (Test-Path $shippedPath) { Get-Content $shippedPath -Raw -ErrorAction SilentlyContinue } else { "" }
            (($shipped ?? "") + "`n" + $unshipped).Trim() | Set-Content $shippedPath -Encoding UTF8 -NoNewline
            $count = ($unshipped -split "`r?`n" | Where-Object { $_.Trim() }).Count
            Write-Success "Moved $count APIs to Shipped"
        }
        "" | Set-Content $unshippedPath -Encoding UTF8
        return
    }
    
    # Get APIs from build diagnostics
    $detectedApis = Get-PublicApiFromBuild -ProjectPath $ProjectPath -ProjectDir $projectDir
    
    if (-not $detectedApis) {
        Write-Info "No new APIs detected"
        return
    }
    
    # Read existing shipped APIs
    $shippedApis = Get-Content $shippedPath -ErrorAction SilentlyContinue | 
        Where-Object { $_.Trim() -and -not $_.StartsWith("#") }
    
    # Filter out already shipped
    $newApis = $detectedApis | Where-Object { $_ -notin $shippedApis }
    
    if ($newApis) {
        $newApis | Set-Content $unshippedPath -Encoding UTF8
        Write-Success "Generated with $($newApis.Count) new APIs"
    }
    else {
        "" | Set-Content $unshippedPath -Encoding UTF8
        Write-Info "All APIs already shipped"
    }
}

# ============ MAIN ============

Write-Host "================================" -ForegroundColor Blue
Write-Host "Auto Public API Generator" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue

if ($Shipped) { Write-Warning "RELEASE MODE: Shipping APIs" }

# Find projects
if ($Project) {
    $projPath = if (Test-Path $Project) { $Project } else { "src\$Project\$Project.csproj" }
    if (-not (Test-Path $projPath)) {
        Write-Error "Project not found: $projPath"
        exit 1
    }
    $projects = @(Get-Item $projPath)
}
else {
    $projects = Get-ChildItem -Path "src" -Recurse -Filter "*.csproj" | 
        Where-Object { (Get-Content $_.FullName -Raw) -match "PublicApiAnalyzers" }
}

Write-Info "Found $($projects.Count) project(s) with PublicApiAnalyzers"

if ($projects.Count -eq 0) {
    Write-Warning "No projects found"
    exit 0
}

foreach ($proj in $projects) {
    Update-Project -ProjectPath $proj.FullName -MoveToShipped:$Shipped
}

Write-Host "`n================================" -ForegroundColor Blue
Write-Host "Done" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue

if (-not $Shipped) {
    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Host "  1. Review the generated PublicAPI.Unshipped.txt files"
    Write-Host "  2. Build to verify: dotnet build"
    Write-Host "  3. Commit: git add -A && git commit -m \"Update public API\""
    Write-Host "`nNote: If build still shows RS0016 warnings, run this script again."
}
