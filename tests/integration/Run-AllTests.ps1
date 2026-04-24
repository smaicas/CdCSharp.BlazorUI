#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Runs all integration tests for the development workflow

.DESCRIPTION
    Master test script that runs all integration tests:
    1. Repository setup validation
    2. Feature workflow (create, squash, PR)
    3. Parallel development scenario
    4. Release gate (blocking issues)

.PARAMETER Owner
    Repository owner

.PARAMETER Repo
    Repository name

.PARAMETER Token
    GitHub Personal Access Token

.PARAMETER Test
    Specific test to run (Setup, Feature, Parallel, Release, All)

.EXAMPLE
    ./Run-AllTests.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN

.EXAMPLE
    ./Run-AllTests.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN -Test Setup
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Owner,
    
    [Parameter(Mandatory = $true)]
    [string]$Repo,
    
    [Parameter(Mandatory = $true)]
    [string]$Token,
    
    [Parameter()]
    [ValidateSet("All", "Setup", "Feature", "Parallel", "Release")]
    [string]$Test = "All"
)

$ErrorActionPreference = "Stop"

$TestResults = @{
    Total = 0
    Passed = 0
    Failed = 0
}

function Write-Header {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Blue
    Write-Host "  $Message" -ForegroundColor Blue
    Write-Host "========================================" -ForegroundColor Blue
}

function Invoke-Test {
    param(
        [string]$Name,
        [string]$ScriptPath
    )
    
    Write-Header $Name
    
    try {
        & $ScriptPath -Owner $Owner -Repo $Repo -Token $Token
        $TestResults.Passed++
        Write-Host "✅ $Name PASSED" -ForegroundColor Green
    }
    catch {
        $TestResults.Failed++
        Write-Host "❌ $Name FAILED" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
    }
    
    $TestResults.Total++
}

# ============ MAIN ============

Write-Host "`n========================================" -ForegroundColor Blue
Write-Host "  Integration Test Suite" -ForegroundColor Blue
Write-Host "========================================" -ForegroundColor Blue
Write-Host "Repository: $Owner/$Repo"
Write-Host ""

$scriptDir = $PSScriptRoot

if ($Test -eq "All" -or $Test -eq "Setup") {
    Invoke-Test -Name "Repository Setup Test" -ScriptPath "$scriptDir\Test-RepositorySetup.ps1"
}

if ($Test -eq "All" -or $Test -eq "Feature") {
    Invoke-Test -Name "Feature Workflow Test" -ScriptPath "$scriptDir\Test-FeatureWorkflow.ps1"
}

if ($Test -eq "All" -or $Test -eq "Parallel") {
    Invoke-Test -Name "Parallel Development Test" -ScriptPath "$scriptDir\Test-ParallelDevelopment.ps1"
}

if ($Test -eq "All" -or $Test -eq "Release") {
    Invoke-Test -Name "Release Gate Test" -ScriptPath "$scriptDir\Test-ReleaseGate.ps1"
}

# ============ SUMMARY ============

Write-Host "`n========================================" -ForegroundColor Blue
Write-Host "  Test Summary" -ForegroundColor Blue
Write-Host "========================================" -ForegroundColor Blue
Write-Host "Total:  $($TestResults.Total)" -ForegroundColor White
Write-Host "Passed: $($TestResults.Passed)" -ForegroundColor Green
Write-Host "Failed: $($TestResults.Failed)" -ForegroundColor $(if ($TestResults.Failed -gt 0) { "Red" } else { "Green" })

if ($TestResults.Failed -gt 0) {
    Write-Host "`nSome tests failed. Check output above for details." -ForegroundColor Red
    exit 1
}
else {
    Write-Host "`n✅ All tests passed!" -ForegroundColor Green
    exit 0
}
