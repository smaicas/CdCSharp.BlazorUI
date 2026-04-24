#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Tests repository setup (labels, branch protection, workflows)

.DESCRIPTION
    Verifies that the repository is correctly configured for the
    squash+rebase workflow with linear history.

.PARAMETER Owner
    Repository owner

.PARAMETER Repo
    Repository name

.PARAMETER Token
    GitHub Personal Access Token

.EXAMPLE
    ./Test-RepositorySetup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Owner,
    
    [Parameter(Mandatory = $true)]
    [string]$Repo,
    
    [Parameter(Mandatory = $true)]
    [string]$Token
)

$ErrorActionPreference = "Stop"

# Configuration
$Config = @{
    ApiBase = "https://api.github.com"
    Headers = @{
        Authorization = "token $Token"
        Accept = "application/vnd.github+json"
        "X-GitHub-Api-Version" = "2022-11-28"
    }
}

# Test results
$TestResults = @{
    Passed = 0
    Failed = 0
    Tests = @()
}

function Write-TestHeader {
    param([string]$Name)
    Write-Host "`n[Test] $Name" -ForegroundColor Cyan
}

function Write-TestResult {
    param(
        [string]$Name,
        [bool]$Passed,
        [string]$Message = ""
    )
    
    $status = if ($Passed) { "✅ PASS" } else { "❌ FAIL" }
    $color = if ($Passed) { "Green" } else { "Red" }
    
    Write-Host "  $status $Name" -ForegroundColor $color
    if ($Message) {
        Write-Host "    $Message" -ForegroundColor Gray
    }
    
    $TestResults.Tests += @{
        Name = $Name
        Passed = $Passed
        Message = $Message
    }
    
    if ($Passed) {
        $TestResults.Passed++
    }
    else {
        $TestResults.Failed++
    }
}

function Invoke-GitHubApi {
    param(
        [string]$Method = "GET",
        [string]$Endpoint
    )
    
    try {
        $uri = "$($Config.ApiBase)$Endpoint"
        $response = Invoke-RestMethod -Uri $uri -Method $Method -Headers $Config.Headers
        return $response
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 404) {
            return $null
        }
        throw
    }
}

# ============ TESTS ============

Write-Host "================================" -ForegroundColor Blue
Write-Host "Repository Setup Tests" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue
Write-Host "Repository: $Owner/$Repo"

# Test 1: Repository Access
Write-TestHeader "Repository Access"
try {
    $repo = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo"
    if ($repo) {
        Write-TestResult -Name "Repository accessible" -Passed $true -Message "Found: $($repo.full_name)"
        Write-TestResult -Name "Default branch is master" -Passed ($repo.default_branch -eq "master") -Message "Branch: $($repo.default_branch)"
    }
    else {
        Write-TestResult -Name "Repository accessible" -Passed $false -Message "Could not access repository"
    }
}
catch {
    Write-TestResult -Name "Repository accessible" -Passed $false -Message $_.Exception.Message
}

# Test 2: Severity Labels
Write-TestHeader "Severity Labels"
$expectedLabels = @(
    "severity/blocker"
    "severity/critical"
    "severity/major"
    "severity/minor"
    "severity/polish"
)

$labels = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/labels?per_page=100"

foreach ($labelName in $expectedLabels) {
    $found = $labels | Where-Object { $_.name -eq $labelName }
    Write-TestResult -Name "Label exists: $labelName" -Passed ($null -ne $found) -Message $(if ($found) { "Color: #$($found.color)" } else { "Not found" })
}

# Test 3: Default 'bug' label removed
Write-TestHeader "Default Labels"
$bugLabel = $labels | Where-Object { $_.name -eq "bug" }
Write-TestResult -Name "Default 'bug' label removed" -Passed ($null -eq $bugLabel) -Message $(if ($bugLabel) { "Still exists - run github-setup.ps1" } else { "Correctly removed" })

# Test 4: Branch Protection for master
Write-TestHeader "Branch Protection for master"
try {
    $protection = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/branches/master/protection"
    if ($protection) {
        Write-TestResult -Name "Protection enabled on master" -Passed $true
        Write-TestResult -Name "Requires PR reviews" -Passed ($protection.required_pull_request_reviews.required_approving_review_count -gt 0) -Message "Approvals: $($protection.required_pull_request_reviews.required_approving_review_count)"
        Write-TestResult -Name "Linear history required" -Passed ($protection.required_linear_history.enabled) -Message "Linear: $($protection.required_linear_history.enabled)"
        Write-TestResult -Name "Squash merge only" -Passed (-not $protection.allow_merge_commit -and -not $protection.allow_rebase_merge) -Message "Squash: $($protection.allow_squash_merge)"
        
        $hasRequiredChecks = $protection.required_status_checks.contexts.Count -gt 0
        Write-TestResult -Name "Has required status checks" -Passed $hasRequiredChecks -Message "Checks: $($protection.required_status_checks.contexts -join ', ')"
    }
    else {
        Write-TestResult -Name "Protection enabled on master" -Passed $false -Message "No protection found"
    }
}
catch {
    Write-TestResult -Name "Protection enabled on master" -Passed $false -Message $_.Exception.Message
}

# Test 5: Branch Protection for develop
Write-TestHeader "Branch Protection for develop"
try {
    $protection = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/branches/develop/protection"
    if ($protection) {
        Write-TestResult -Name "Protection enabled on develop" -Passed $true
        Write-TestResult -Name "Requires PR reviews" -Passed ($protection.required_pull_request_reviews.required_approving_review_count -gt 0)
        Write-TestResult -Name "Linear history required" -Passed ($protection.required_linear_history.enabled)
        
        $hasRequiredChecks = $protection.required_status_checks.contexts.Count -gt 0
        Write-TestResult -Name "Has required status checks" -Passed $hasRequiredChecks -Message "Checks: $($protection.required_status_checks.contexts -join ', ')"
    }
    else {
        Write-TestResult -Name "Protection enabled on develop" -Passed $false -Message "No protection found"
    }
}
catch {
    Write-TestResult -Name "Protection enabled on develop" -Passed $false -Message $_.Exception.Message
}

# Test 6: Workflows exist
Write-TestHeader "Workflow Files"
$workflows = @(
    "preview-gate.yml"
    "preview-publish.yml"
    "release-gate.yml"
    "release-publish.yml"
)

foreach ($wf in $workflows) {
    try {
        $content = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/contents/.github/workflows/$wf"
        Write-TestResult -Name "Workflow exists: $wf" -Passed ($null -ne $content)
    }
    catch {
        Write-TestResult -Name "Workflow exists: $wf" -Passed $false -Message "Not found"
    }
}

# Test 7: Develop branch exists
Write-TestHeader "Develop Branch"
$develop = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/branches/develop"
Write-TestResult -Name "Develop branch exists" -Passed ($null -ne $develop) -Message $(if ($develop) { "Found" } else { "Missing - run github-setup.ps1" })

# ============ SUMMARY ============

Write-Host "`n================================" -ForegroundColor Blue
Write-Host "Test Summary" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue
Write-Host "Passed: $($TestResults.Passed)" -ForegroundColor Green
Write-Host "Failed: $($TestResults.Failed)" -ForegroundColor $(if ($TestResults.Failed -gt 0) { "Red" } else { "Green" })
Write-Host "Total:  $($TestResults.Passed + $TestResults.Failed)"

if ($TestResults.Failed -gt 0) {
    Write-Host "`nFailed tests:" -ForegroundColor Red
    $TestResults.Tests | Where-Object { -not $_.Passed } | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor Red
    }
    exit 1
}
else {
    Write-Host "`n✅ All tests passed!" -ForegroundColor Green
    exit 0
}
