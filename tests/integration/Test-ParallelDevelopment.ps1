#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Tests parallel development scenario (two developers, same file)

.DESCRIPTION
    Simulates two developers working on the same file:
    1. Developer A creates feature branch and commits
    2. Developer B creates feature branch from same base and commits
    3. Both create PRs
    4. First PR merged
    5. Second PR needs rebase (conflict simulation)

.PARAMETER Owner
    Repository owner

.PARAMETER Repo
    Repository name

.PARAMETER Token
    GitHub Personal Access Token

.EXAMPLE
    ./Test-ParallelDevelopment.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN
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

$Timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$BranchA = "test-parallel-a-$Timestamp"
$BranchB = "test-parallel-b-$Timestamp"
$TestFile = "tests/parallel-test.md"

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

function Write-Warning {
    param([string]$Message)
    Write-Host "  ⚠️  $Message" -ForegroundColor Yellow
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

function New-Commit {
    param(
        [string]$Branch,
        [string]$ParentSha,
        [string]$Content,
        [string]$Message
    )
    
    $blob = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/blobs" -Body @{
        content = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($Content))
        encoding = "base64"
    }
    
    $parentCommit = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/git/commits/$ParentSha"
    
    $tree = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/trees" -Body @{
        base_tree = $parentCommit.tree.sha
        tree = @(
            @{
                path = $TestFile
                mode = "100644"
                type = "blob"
                sha = $blob.sha
            }
        )
    }
    
    $commit = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/commits" -Body @{
        message = $Message
        tree = $tree.sha
        parents = @($ParentSha)
    }
    
    Invoke-GitHubApi -Method "PATCH" -Endpoint "/repos/$Owner/$Repo/git/refs/heads/$Branch" -Body @{
        sha = $commit.sha
    }
    
    return $commit
}

# ============ TEST ============

Write-Host "================================" -ForegroundColor Blue
Write-Host "Parallel Development Test" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue
Write-Host "Developer A: $BranchA"
Write-Host "Developer B: $BranchB"
Write-Host "Test File: $TestFile"

# Step 1: Get master SHA
Write-Step "Getting master branch reference"
$masterRef = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/git/refs/heads/master"
$masterSha = $masterRef.object.sha
Write-Success "Master SHA: $masterSha"

# Step 2: Developer A creates branch and commits
Write-Step "Developer A: Creating branch and committing"
Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/refs" -Body @{
    ref = "refs/heads/$BranchA"
    sha = $masterSha
}
Write-Success "Branch A created"

$commitA = New-Commit -Branch $BranchA -ParentSha $masterSha `
    -Content "# Parallel Test`n`n## Developer A`n`nThis is developer A's contribution." `
    -Message "feat(test): add developer A contribution`n`nDeveloper A added content to test file.`n`nFixes #test-a"
Write-Success "Developer A committed: $($commitA.sha.Substring(0,7))"

# Step 3: Developer B creates branch (from same base) and commits
Write-Step "Developer B: Creating branch and committing"
Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/refs" -Body @{
    ref = "refs/heads/$BranchB"
    sha = $masterSha
}
Write-Success "Branch B created"

$commitB = New-Commit -Branch $BranchB -ParentSha $masterSha `
    -Content "# Parallel Test`n`n## Developer B`n`nThis is developer B's contribution." `
    -Message "feat(test): add developer B contribution`n`nDeveloper B added content to test file.`n`nFixes #test-b"
Write-Success "Developer B committed: $($commitB.sha.Substring(0,7))"

# Step 4: Both create PRs
Write-Step "Creating Pull Requests"

$prA = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/pulls" -Body @{
    title = "TEST: Developer A contribution"
    body = "This PR adds Developer A's contribution.`n`n## Changes`n- Added content to test file`n`nFixes #test-a"
    head = $BranchA
    base = "develop"
}
Write-Success "PR A created: #$($prA.number)"

$prB = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/pulls" -Body @{
    title = "TEST: Developer B contribution"
    body = "This PR adds Developer B's contribution.`n`n## Changes`n- Added content to test file`n`nFixes #test-b"
    head = $BranchB
    base = "develop"
}
Write-Success "PR B created: #$($prB.number)"

# Step 5: Show scenario
Write-Step "Parallel Development Scenario Created"
Write-Host ""
Write-Host "Current state:" -ForegroundColor Yellow
Write-Host "  Both PRs are based on the same master commit"
Write-Host "  Both modify the same file: $TestFile"
Write-Host "  Both target: develop"
Write-Host ""
Write-Host "Expected behavior:" -ForegroundColor Yellow
Write-Host "  1. First PR merged (squash) → Preview Publish runs"
Write-Host "  2. Second PR becomes 'behind' develop"
Write-Host "  3. Second PR shows 'Update branch' button in GitHub"
Write-Host "  4. Developer B must rebase: git rebase origin/develop"
Write-Host "  5. Developer B will have CONFLICTS to resolve"
Write-Host ""

# Step 6: Instructions for manual testing
Write-Host "Manual Test Steps:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Open PR A: $($prA.html_url)"
Write-Host "   - Verify Preview Gate runs"
Write-Host "   - Merge PR A (squash)"
Write-Host ""
Write-Host "2. Open PR B: $($prB.html_url)"
Write-Host "   - Should show 'This branch is out-of-date'"
Write-Host "   - Should have 'Update branch' button"
Write-Host ""
Write-Host "3. Simulate Developer B rebase:"
Write-Host "   git fetch origin"
Write-Host "   git checkout $BranchB"
Write-Host "   git rebase origin/develop"
Write-Host "   # CONFLICT! Resolve manually"
Write-Host "   git add ."
Write-Host "   git rebase --continue"
Write-Host "   git push --force-with-lease"
Write-Host ""
Write-Host "4. After rebase, PR B should be mergeable"
Write-Host ""

# ============ SUMMARY ============

Write-Host "================================" -ForegroundColor Blue
Write-Host "Test Summary" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue
Write-Host ""
Write-Host "Branches:" -ForegroundColor Yellow
Write-Host "  A: $BranchA"
Write-Host "  B: $BranchB"
Write-Host ""
Write-Host "Pull Requests:" -ForegroundColor Yellow
Write-Host "  A: #$($prA.number) - $($prA.html_url)"
Write-Host "  B: #$($prB.number) - $($prB.html_url)"
Write-Host ""
Write-Host "Cleanup (after testing):" -ForegroundColor Yellow
Write-Host "  ./Test-Cleanup.ps1 -Owner $Owner -Repo $Repo -Token `$Token -BranchName $BranchA"
Write-Host "  ./Test-Cleanup.ps1 -Owner $Owner -Repo $Repo -Token `$Token -BranchName $BranchB"
