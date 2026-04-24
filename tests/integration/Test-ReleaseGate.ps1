#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Tests the release gate (blocking issues detection)

.DESCRIPTION
    Creates issues with different severity labels and verifies
    that the release gate correctly blocks/unblocks based on
    severity/blocker and severity/critical labels.

.PARAMETER Owner
    Repository owner

.PARAMETER Repo
    Repository name

.PARAMETER Token
    GitHub Personal Access Token

.EXAMPLE
    ./Test-ReleaseGate.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN
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

$Config = @{
    ApiBase = "https://api.github.com"
    Headers = @{
        Authorization = "token $Token"
        Accept = "application/vnd.github+json"
        "X-GitHub-Api-Version" = "2022-11-28"
    }
}

$TestIssues = @()

function Write-Step {
    param([string]$Message)
    Write-Host "`n[STEP] $Message" -ForegroundColor Cyan
}

function Write-Info {
    param([string]$Message)
    Write-Host "  ℹ️  $Message" -ForegroundColor Gray
}

function Write-Success {
    param([string]$Message)
    Write-Host "  ✅ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "  ❌ $Message" -ForegroundColor Red
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
        Write-Error "API call failed: $($_.Exception.Message)"
        throw
    }
    }
}

# ============ TEST ============

Write-Host "================================" -ForegroundColor Blue
Write-Host "Release Gate Test" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue
Write-Host ""

# Step 1: Create test issues with different severities
Write-Step "Creating test issues with different severity labels"

$issuesToCreate = @(
    @{
        Title = "TEST: Blocker issue - blocks release"
        Body = "This is a test blocker issue.`n`nShould block release."
        Label = "severity/blocker"
    }
    @{
        Title = "TEST: Critical issue - blocks release"
        Body = "This is a test critical issue.`n`nShould block release."
        Label = "severity/critical"
    }
    @{
        Title = "TEST: Major issue - does not block"
        Body = "This is a test major issue.`n`nShould NOT block release."
        Label = "severity/major"
    }
    @{
        Title = "TEST: Minor issue - does not block"
        Body = "This is a test minor issue.`n`nShould NOT block release."
        Label = "severity/minor"
    }
)

foreach ($issueDef in $issuesToCreate) {
    $issue = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/issues" -Body @{
        title = $issueDef.Title
        body = $issueDef.Body
        labels = @($issueDef.Label)
    }
    
    $TestIssues += $issue.number
    Write-Success "Created issue #$($issue.number): $($issueDef.Label)"
}

# Step 2: Verify issues are open
Write-Step "Verifying issues are open"
$openIssues = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/issues?state=open&labels=severity/blocker,severity/critical,severity/major,severity/minor"

$blockerCount = ($openIssues | Where-Object { $_.labels.name -contains "severity/blocker" }).Count
$criticalCount = ($openIssues | Where-Object { $_.labels.name -contains "severity/critical" }).Count

Write-Info "Open blocking issues (blocker): $blockerCount"
Write-Info "Open blocking issues (critical): $criticalCount"
Write-Info "Total blocking: $($blockerCount + $criticalCount)"

if (($blockerCount + $criticalCount) -gt 0) {
    Write-Success "Release gate SHOULD BLOCK release"
}
else {
    Write-Error "Expected blocking issues but found none"
}

# Step 3: Create PR to master (will trigger Release Gate)
Write-Step "Creating test PR to master (will trigger Release Gate)"

$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$branchName = "test-release-gate-$timestamp"

# Get master SHA
$masterRef = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/git/refs/heads/master"
$masterSha = $masterRef.object.sha

# Create branch
Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/refs" -Body @{
    ref = "refs/heads/$branchName"
    sha = $masterSha
}

# Create commit
$content = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("# Release Gate Test`n`nThis tests the release gate."))
$blob = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/blobs" -Body @{
    content = $content
    encoding = "base64"
}

$masterCommit = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/git/commits/$masterSha"
$tree = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/trees" -Body @{
    base_tree = $masterCommit.tree.sha
    tree = @(@{ path = "tests/release-gate-test.md"; mode = "100644"; type = "blob"; sha = $blob.sha })
}

$commit = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/commits" -Body @{
    message = "test: add release gate test file`n`nThis tests the release gate blocking.`n`nFixes #test"
    tree = $tree.sha
    parents = @($masterSha)
}

Invoke-GitHubApi -Method "PATCH" -Endpoint "/repos/$Owner/$Repo/git/refs/heads/$branchName" -Body @{
    sha = $commit.sha
}

# Create PR
$pr = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/pulls" -Body @{
    title = "TEST: Release Gate validation"
    body = "This PR tests the release gate.`n`nExpected: Release Gate should FAIL due to blocker issues.`n`n## Test Issues`n- severity/blocker: Should block`n- severity/critical: Should block`n- severity/major: Should NOT block`n- severity/minor: Should NOT block`n`nFixes #test-release"
    head = $branchName
    base = "master"
}

Write-Success "PR created: #$($pr.number)"

# Step 4: Instructions
Write-Step "Test Instructions"
Write-Host ""
Write-Host "1. Open PR: $($pr.html_url)" -ForegroundColor Yellow
Write-Host ""
Write-Host "2. Wait for Release Gate workflow to run" -ForegroundColor Yellow
Write-Host "   Expected: Release Gate should FAIL"
Write-Host "   Reason: severity/blocker and severity/critical issues are open"
Write-Host ""
Write-Host "3. Close the blocker issue to test unblocking:" -ForegroundColor Yellow
Write-Host "   - Go to issue #$($TestIssues[0]) (severity/blocker)"
Write-Host "   - Close it"
Write-Host "   - Re-run Release Gate"
Write-Host "   - Should still FAIL (severity/critical still open)"
Write-Host ""
Write-Host "4. Close the critical issue:" -ForegroundColor Yellow
Write-Host "   - Go to issue #$($TestIssues[1]) (severity/critical)"
Write-Host "   - Close it"
Write-Host "   - Re-run Release Gate"
Write-Host "   - Should PASS now"
Write-Host ""
Write-Host "5. Cleanup (after testing):" -ForegroundColor Yellow
Write-Host "   Close all test issues:"
foreach ($issueNum in $TestIssues) {
    Write-Host "     ./Test-Cleanup.ps1 -Owner $Owner -Repo $Repo -Token `$Token -IssueNumber $issueNum"
}
Write-Host "   Delete branch:"
Write-Host "     ./Test-Cleanup.ps1 -Owner $Owner -Repo $Repo -Token `$Token -BranchName $branchName"

# ============ SUMMARY ============

Write-Host "`n================================" -ForegroundColor Blue
Write-Host "Test Summary" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue
Write-Host ""
Write-Host "Created Issues:" -ForegroundColor Yellow
for ($i = 0; $i -lt $TestIssues.Count; $i++) {
    Write-Host "  #$($TestIssues[$i]): $($issuesToCreate[$i].Label)"
}
Write-Host ""
Write-Host "Pull Request: #$($pr.number)" -ForegroundColor Yellow
Write-Host "URL: $($pr.html_url)"
Write-Host ""
Write-Host "Expected Release Gate Behavior:" -ForegroundColor Yellow
Write-Host "  With blocker/critical open: ❌ FAIL"
Write-Host "  After closing both:         ✅ PASS"
