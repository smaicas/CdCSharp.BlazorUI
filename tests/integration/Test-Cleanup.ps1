#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Cleans up test artifacts (branches, issues, PRs)

.DESCRIPTION
    Removes test branches, closes test issues, and closes test PRs
    created by the test scripts.

.PARAMETER Owner
    Repository owner

.PARAMETER Repo
    Repository name

.PARAMETER Token
    GitHub Personal Access Token

.PARAMETER BranchName
    Branch to delete (optional)

.PARAMETER IssueNumber
    Issue to close (optional)

.PARAMETER PrNumber
    PR to close (optional)

.PARAMETER CleanupAll
    Clean up all test artifacts (branches starting with 'test-')

.EXAMPLE
    ./Test-Cleanup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN -BranchName "test-feature-123"

.EXAMPLE
    ./Test-Cleanup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN -IssueNumber 42

.EXAMPLE
    ./Test-Cleanup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN -CleanupAll
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
    [string]$BranchName,
    
    [Parameter()]
    [int]$IssueNumber,
    
    [Parameter()]
    [int]$PrNumber,
    
    [Parameter()]
    [switch]$CleanupAll
)

$ErrorActionPreference = "Stop"

$Config = @{
    ApiBase = "https://api.github.com"
    Headers = @{
        Authorization = "token $Token"
        Accept = "application/vnd.github+json"
        "X-GitHub-Api-Version" = "2022-11-28"
    }
}

function Write-Step {
    param([string]$Message)
    Write-Host "`n[STEP] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "  ✅ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "  ❌ $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "  ℹ️  $Message" -ForegroundColor Gray
}

function Invoke-GitHubApi {
    param(
        [string]$Method = "GET",
        [string]$Endpoint,
        [object]$Body = $null
    )
    
    $uri = "$($Config.ApiBase)$Endpoint"
    $params = @{
        Uri = $uri
        Method = $Method
        Headers = $Config.Headers
    }
    
    if ($Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 10)
        $params.ContentType = "application/json"
    }
    
    try {
        return Invoke-RestMethod @params
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 404) {
            return $null
        }
        throw
    }
}

# ============ CLEANUP ============

Write-Host "================================" -ForegroundColor Blue
Write-Host "Test Cleanup" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue

if ($BranchName) {
    Write-Step "Deleting branch: $BranchName"
    
    $result = Invoke-GitHubApi -Method "DELETE" -Endpoint "/repos/$Owner/$Repo/git/refs/heads/$BranchName"
    
    if ($result -eq $null) {
        Write-Success "Branch deleted: $BranchName"
    }
    else {
        Write-Error "Could not delete branch: $BranchName"
    }
}

if ($IssueNumber) {
    Write-Step "Closing issue: #$IssueNumber"
    
    $result = Invoke-GitHubApi -Method "PATCH" -Endpoint "/repos/$Owner/$Repo/issues/$IssueNumber" -Body @{
        state = "closed"
    }
    
    if ($result) {
        Write-Success "Issue closed: #$IssueNumber"
    }
    else {
        Write-Error "Could not close issue: #$IssueNumber"
    }
}

if ($PrNumber) {
    Write-Step "Closing PR: #$PrNumber"
    
    $result = Invoke-GitHubApi -Method "PATCH" -Endpoint "/repos/$Owner/$Repo/pulls/$PrNumber" -Body @{
        state = "closed"
    }
    
    if ($result) {
        Write-Success "PR closed: #$PrNumber"
    }
    else {
        Write-Error "Could not close PR: #$PrNumber"
    }
}

if ($CleanupAll) {
    Write-Step "Cleaning up all test artifacts"
    
    # Get all branches
    $branches = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/branches?per_page=100"
    $testBranches = $branches | Where-Object { $_.name -like "test-*" }
    
    Write-Info "Found $($testBranches.Count) test branches"
    
    foreach ($branch in $testBranches) {
        $result = Invoke-GitHubApi -Method "DELETE" -Endpoint "/repos/$Owner/$Repo/git/refs/heads/$($branch.name)"
        if ($result -eq $null) {
            Write-Success "Deleted branch: $($branch.name)"
        }
        else {
            Write-Error "Could not delete: $($branch.name)"
        }
    }
    
    # Get all open issues with test label
    $issues = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/issues?state=open&per_page=100"
    $testIssues = $issues | Where-Object { $_.title -like "TEST:*" -or $_.title -like "test:*" }
    
    Write-Info "Found $($testIssues.Count) test issues"
    
    foreach ($issue in $testIssues) {
        $result = Invoke-GitHubApi -Method "PATCH" -Endpoint "/repos/$Owner/$Repo/issues/$($issue.number)" -Body @{
            state = "closed"
        }
        if ($result) {
            Write-Success "Closed issue: #$($issue.number) - $($issue.title)"
        }
        else {
            Write-Error "Could not close: #$($issue.number)"
        }
    }
    
    # Get all open PRs with test in title
    $prs = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/pulls?state=open&per_page=100"
    $testPrs = $prs | Where-Object { $_.title -like "TEST:*" -or $_.title -like "test:*" }
    
    Write-Info "Found $($testPrs.Count) test PRs"
    
    foreach ($pr in $testPrs) {
        $result = Invoke-GitHubApi -Method "PATCH" -Endpoint "/repos/$Owner/$Repo/pulls/$($pr.number)" -Body @{
            state = "closed"
        }
        if ($result) {
            Write-Success "Closed PR: #$($pr.number) - $($pr.title)"
        }
        else {
            Write-Error "Could not close: #$($pr.number)"
        }
    }
}

Write-Host "`n================================" -ForegroundColor Blue
Write-Host "Cleanup Complete" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue
