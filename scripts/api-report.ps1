#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Generates public API reports and tracks changes

.DESCRIPTION
    Analyzes PublicAPI.Unshipped.txt and PublicAPI.Shipped.txt files
    to generate reports of API changes between versions.

.PARAMETER Tag
    Tag to compare against (default: last tag)

.PARAMETER OutputFormat
    Output format: markdown, json, or console

.PARAMETER OutputPath
    Path to save the report

.EXAMPLE
    ./api-report.ps1 -Tag v1.0.0 -OutputFormat markdown -OutputPath ./api-changes.md

.EXAMPLE
    ./api-report.ps1 -OutputFormat console
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$Tag,

    [Parameter()]
    [ValidateSet("markdown", "json", "console")]
    [string]$OutputFormat = "console",

    [Parameter()]
    [string]$OutputPath
)

# Colors
$Colors = @{
    Success = "Green"
    Warning = "Yellow"
    Error = "Red"
    Info = "Cyan"
    Emphasis = "Magenta"
}

function Write-Header {
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

function Get-LastTag {
    try {
        $tag = git describe --tags --abbrev=0 2>$null
        if ($tag) { return $tag }
    }
    catch { }
    return "v0.0.0"
}

function Get-ApiFiles {
    param([string]$Path = ".")
    
    Get-ChildItem -Path $Path -Recurse -Filter "PublicAPI.*.txt" | ForEach-Object {
        [PSCustomObject]@{
            Path = $_.FullName
            RelativePath = $_.FullName.Substring((Get-Location).Path.Length + 1)
            Project = $_.Directory.Parent.Name
            Type = $_.Name -replace "PublicAPI\.(\w+)\.txt", '$1'
        }
    }
}

function Get-ApiChanges {
    param(
        [string]$FromTag,
        [string]$ToRef = "HEAD"
    )
    
    Write-Info "Comparing $FromTag..$ToRef"
    
    # Get changed PublicAPI files
    $changedFiles = git diff --name-only "$FromTag..$ToRef" | Where-Object { $_ -match "PublicAPI\.(Unshipped|Shipped)\.txt$" }
    
    $changes = @()
    
    foreach ($file in $changedFiles) {
        $project = (Split-Path $file -Parent | Split-Path -Parent | Split-Path -Leaf)
        $type = if ($file -match "Unshipped") { "Unshipped" } else { "Shipped" }
        
        # Get added lines
        $diff = git diff "$FromTag..$ToRef" -- "$file"
        $added = $diff | Select-String "^\+" | Where-Object { $_ -notmatch "^\+\+\+" -and $_ -notmatch "^\+//" } | ForEach-Object { $_ -replace "^\+", "" }
        $removed = $diff | Select-String "^-" | Where-Object { $_ -notmatch "^---" -and $_ -notmatch "^-//" } | ForEach-Object { $_ -replace "^-", "" }
        
        $changes += [PSCustomObject]@{
            Project = $project
            File = $file
            Type = $type
            Added = $added
            Removed = $removed
            AddedCount = ($added | Measure-Object).Count
            RemovedCount = ($removed | Measure-Object).Count
        }
    }
    
    return $changes
}

function Export-MarkdownReport {
    param(
        [array]$Changes,
        [string]$FromTag,
        [string]$ToRef,
        [string]$Path
    )
    
    $report = @()
    $report += "# Public API Changes Report"
    $report += ""
    $report += "**From:** $FromTag"
    $report += "**To:** $ToRef"
    $report += "**Generated:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    $report += ""
    
    if ($Changes.Count -eq 0) {
        $report += "*No public API changes detected.*"
    }
    else {
        $report += "## Summary"
        $report += ""
        $report += "| Project | File | Added | Removed |"
        $report += "|---------|------|-------|---------|"
        
        foreach ($change in $Changes) {
            $report += "| $($change.Project) | $($change.Type) | $($change.AddedCount) | $($change.RemovedCount) |"
        }
        
        $report += ""
        $report += "## Detailed Changes"
        $report += ""
        
        foreach ($change in $Changes) {
            $report += "### $($change.Project) - $($change.Type)"
            $report += ""
            
            if ($change.AddedCount -gt 0) {
                $report += "**Added:**"
                $report += '```csharp'
                $report += $change.Added
                $report += '```'
                $report += ""
            }
            
            if ($change.RemovedCount -gt 0) {
                $report += "**Removed:**"
                $report += '```csharp'
                $report += $change.Removed
                $report += '```'
                $report += ""
            }
        }
    }
    
    if ($Path) {
        $report -join "`n" | Out-File -FilePath $Path -Encoding UTF8
        Write-Success "Report saved to: $Path"
    }
    else {
        $report -join "`n"
    }
}

function Export-JsonReport {
    param(
        [array]$Changes,
        [string]$FromTag,
        [string]$ToRef,
        [string]$Path
    )
    
    $report = [PSCustomObject]@{
        FromTag = $FromTag
        ToRef = $ToRef
        GeneratedAt = Get-Date -Format 'o'
        TotalChanges = $Changes.Count
        Changes = $Changes | ForEach-Object {
            [PSCustomObject]@{
                Project = $_.Project
                Type = $_.Type
                Added = @($_.Added)
                Removed = @($_.Removed)
                AddedCount = $_.AddedCount
                RemovedCount = $_.RemovedCount
            }
        }
    }
    
    $json = $report | ConvertTo-Json -Depth 10
    
    if ($Path) {
        $json | Out-File -FilePath $Path -Encoding UTF8
        Write-Success "Report saved to: $Path"
    }
    else {
        $json
    }
}

function Show-ConsoleReport {
    param(
        [array]$Changes,
        [string]$FromTag,
        [string]$ToRef
    )
    
    Write-Header "Public API Changes: $FromTag -> $ToRef"
    
    if ($Changes.Count -eq 0) {
        Write-Info "No public API changes detected."
        return
    }
    
    Write-Info "Found $($Changes.Count) changed API file(s):"
    
    foreach ($change in $Changes) {
        Write-Host "`n  📦 $($change.Project) [$($change.Type)]" -ForegroundColor $Colors.Emphasis
        
        if ($change.AddedCount -gt 0) {
            Write-Host "     +$($change.AddedCount) added" -ForegroundColor $Colors.Success
            $change.Added | Select-Object -First 5 | ForEach-Object {
                Write-Host "       $_" -ForegroundColor Gray
            }
            if ($change.AddedCount -gt 5) {
                Write-Host "       ... and $($change.AddedCount - 5) more" -ForegroundColor Gray
            }
        }
        
        if ($change.RemovedCount -gt 0) {
            Write-Host "     -$($change.RemovedCount) removed" -ForegroundColor $Colors.Error
        }
    }
    
    # Summary
    $totalAdded = ($Changes | Measure-Object -Property AddedCount -Sum).Sum
    $totalRemoved = ($Changes | Measure-Object -Property RemovedCount -Sum).Sum
    
    Write-Host "`n  Summary: +$totalAdded / -$totalRemoved" -ForegroundColor $Colors.Info
}

# ============ MAIN ============

# Determine tags
if (-not $Tag) {
    $Tag = Get-LastTag
}

$currentRef = git rev-parse HEAD

Write-Info "Analyzing API changes..."
Write-Info "From: $Tag"
Write-Info "To: $currentRef"

# Get changes
$changes = Get-ApiChanges -FromTag $Tag -ToRef $currentRef

# Export based on format
switch ($OutputFormat) {
    "markdown" {
        Export-MarkdownReport -Changes $changes -FromTag $Tag -ToRef $currentRef -Path $OutputPath
    }
    "json" {
        Export-JsonReport -Changes $changes -FromTag $Tag -ToRef $currentRef -Path $OutputPath
    }
    "console" {
        Show-ConsoleReport -Changes $changes -FromTag $Tag -ToRef $currentRef
    }
}

# Exit code based on changes
if ($changes.Count -gt 0) {
    exit 0  # Changes detected (normal)
}
else {
    exit 0  # No changes (also normal)
}
