#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Generates PublicAPI.Shipped.txt and PublicAPI.Unshipped.txt files automatically

.DESCRIPTION
    Uses dotnet build with PublicApiAnalyzers to generate the API files.
    Temporarily enables RS0016/RS0017 as info level, builds, and extracts the API.

.PARAMETER Project
    Specific project to generate (optional). If not specified, generates for all projects with PublicApiAnalyzers.

.PARAMETER Shipped
    Generate/update PublicAPI.Shipped.txt (moves current Unshipped to Shipped)

.EXAMPLE
    ./generate-publicapi.ps1
    Generates PublicAPI.Unshipped.txt for all projects

.EXAMPLE
    ./generate-publicapi.ps1 -Project CdCSharp.BlazorUI.Core
    Generates only for specified project

.EXAMPLE
    ./generate-publicapi.ps1 -Shipped
    Moves Unshipped APIs to Shipped (for release)
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$Project,
    
    [Parameter()]
    [switch]$Shipped
)

$ErrorActionPreference = "Stop"

# Colors
$Colors = @{
    Success = "Green"
    Warning = "Yellow"
    Error = "Red"
    Info = "Cyan"
    Emphasis = "Magenta"
}

function Write-Step {
    param([string]$Message)
    Write-Host "`n=== $Message ===" -ForegroundColor $Colors.Emphasis
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $Colors.Success
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor $Colors.Warning
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor $Colors.Info
}

# Find projects with PublicApiAnalyzers
function Get-ProjectsWithAnalyzer {
    $projects = Get-ChildItem -Path "src" -Recurse -Filter "*.csproj" | Where-Object {
        $content = Get-Content $_.FullName -Raw
        $content -match "PublicApiAnalyzers"
    }
    
    return $projects
}

# Generate API file for a project using dotnet format
function Generate-PublicApi {
    param(
        [string]$ProjectPath,
        [switch]$MoveToShipped
    )
    
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath)
    $projectDir = [System.IO.Path]::GetDirectoryName($ProjectPath)
    
    Write-Step "Processing: $projectName"
    
    # Define paths
    $shippedPath = Join-Path $projectDir "PublicAPI.Shipped.txt"
    $unshippedPath = Join-Path $projectDir "PublicAPI.Unshipped.txt"
    
    if ($MoveToShipped) {
        # Move Unshipped to Shipped (for release)
        Write-Info "Moving Unshipped to Shipped (release mode)"
        
        if (Test-Path $unshippedPath) {
            $unshippedContent = Get-Content $unshippedPath -Raw -ErrorAction SilentlyContinue
            
            if (-not [string]::IsNullOrWhiteSpace($unshippedContent)) {
                # Append to Shipped
                Add-Content -Path $shippedPath -Value $unshippedContent -Encoding UTF8 -ErrorAction SilentlyContinue
                Write-Success "Appended Unshipped content to Shipped"
            }
            
            # Clear Unshipped
            "" | Set-Content -Path $unshippedPath -Encoding UTF8
            Write-Success "Cleared Unshipped.txt"
        }
        return
    }
    
    # Ensure files exist
    if (-not (Test-Path $shippedPath)) {
        "" | Set-Content -Path $shippedPath -Encoding UTF8
        Write-Info "Created empty PublicAPI.Shipped.txt"
    }
    if (-not (Test-Path $unshippedPath)) {
        "" | Set-Content -Path $unshippedPath -Encoding UTF8
        Write-Info "Created empty PublicAPI.Unshipped.txt"
    }
    
    # Use dotnet format to apply fixes
    Write-Info "Running dotnet format to generate Public API..."
    
    Push-Location $projectDir
    try {
        # First build to get analyzer diagnostics
        $buildOutput = & dotnet build --verbosity normal 2>&1
        
        # Look for RS0016 errors which indicate missing API entries
        $missingApis = $buildOutput | Select-String "RS0016.*Symbol '(.*)'" | ForEach-Object {
            if ($_ -match "Symbol '([^']+)'") {
                $matches[1]
            }
        }
        
        if ($missingApis) {
            Write-Info "Found $($missingApis.Count) missing API entries"
            
            # Add to Unshipped
            $existing = Get-Content $unshippedPath -ErrorAction SilentlyContinue
            $allApis = @($existing) + $missingApis | Where-Object { $_ } | Sort-Object -Unique
            $allApis | Set-Content -Path $unshippedPath -Encoding UTF8
            
            Write-Success "Added $($missingApis.Count) entries to PublicAPI.Unshipped.txt"
        }
        else {
            Write-Info "No missing API entries found"
        }
        
        # Try using dotnet format analyzers
        Write-Info "Running dotnet format analyzers..."
        & dotnet format analyzers --verbosity diagnostic 2>&1 | Out-Null
        
        # Check if files were updated
        $unshippedContent = Get-Content $unshippedPath -Raw -ErrorAction SilentlyContinue
        if ($unshippedContent -and $unshippedContent.Trim()) {
            $lineCount = ($unshippedContent -split "`n" | Where-Object { $_.Trim() }).Count
            Write-Success "PublicAPI.Unshipped.txt has $lineCount entries"
        }
    }
    finally {
        Pop-Location
    }
}

# ============ MAIN ============

Write-Host "================================" -ForegroundColor Blue
Write-Host "Public API Generator" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue

if ($Shipped) {
    Write-Warning "RELEASE MODE: Moving Unshipped APIs to Shipped"
}

# Find projects
if ($Project) {
    $projectPath = "src\$Project\$Project.csproj"
    if (-not (Test-Path $projectPath)) {
        Write-Error "Project not found: $projectPath"
        exit 1
    }
    $projects = @(Get-Item $projectPath)
}
else {
    Write-Step "Finding projects with PublicApiAnalyzers"
    $projects = Get-ProjectsWithAnalyzer
}

Write-Info "Found $($projects.Count) project(s) with PublicApiAnalyzers"

# Process each project
foreach ($proj in $projects) {
    Generate-PublicApi -ProjectPath $proj.FullName -MoveToShipped:$Shipped
}

Write-Host "`n================================" -ForegroundColor Blue
Write-Host "Public API Generation Complete" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue

if (-not $Shipped) {
    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Host "  1. Review the generated PublicAPI.Unshipped.txt files"
    Write-Host "  2. Build the project: dotnet build"
    Write-Host "  3. If RS0016 errors remain, run this script again"
    Write-Host "  4. Commit the changes"
    Write-Host "`nFor release:" -ForegroundColor Yellow
    Write-Host "  ./generate-publicapi.ps1 -Shipped"
}
