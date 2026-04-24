#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Tests the feature branch workflow (create, commit, squash, PR)

.DESCRIPTION
    Simulates a developer working on a feature:
    1. Creates feature branch
    2. Makes commits
    3. Squashes to 1 commit
    4. Creates PR
    5. Verifies Preview Gate runs

.PARAMETER Owner
    Repository owner

.PARAMETER Repo
    Repository name

.PARAMETER Token
    GitHub Personal Access Token

.PARAMETER BranchName
    Name for the test feature branch (default: test-feature-{timestamp})

.EXAMPLE
    ./Test-FeatureWorkflow.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN

.EXAMPLE
    ./Test-FeatureWorkflow.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN -BranchName "test-css-tokens"
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
    [string]$BranchName = "test-feature-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
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

function Wait-ForWorkflowRun {
    param(
        [string]$Branch,
        [string]$WorkflowName,
        [int]$TimeoutMinutes = 5
    )
    
    Write-Info "Waiting for workflow: $WorkflowName on branch $Branch"
    $startTime = Get-Date
    
    while (((Get-Date) - $startTime).TotalMinutes -lt $TimeoutMinutes) {
        $runs = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/actions/runs?branch=$Branch&per_page=5"
        $run = $runs.workflow_runs | Where-Object { $_.name -eq $WorkflowName } | Select-Object -First 1
        
        if ($run) {
            Write-Info "Found run: $($run.id) - Status: $($run.status)"
            
            if ($run.status -eq "completed") {
                return $run
            }
        }
        
        Start-Sleep -Seconds 10
    }
    
    throw "Timeout waiting for workflow"
}

# ============ TEST ============

Write-Host "================================" -ForegroundColor Blue
Write-Host "Feature Workflow Test" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue
Write-Host "Branch: $BranchName"
Write-Host ""

# Step 1: Get master SHA
Write-Step "Getting master branch reference"
$masterRef = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/git/refs/heads/master"
$masterSha = $masterRef.object.sha
Write-Success "Master SHA: $masterSha"

# Step 2: Create feature branch
Write-Step "Creating feature branch: $BranchName"
$branchBody = @{
    ref = "refs/heads/$BranchName"
    sha = $masterSha
}
$branch = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/refs" -Body $branchBody
Write-Success "Branch created: $($branch.ref)"

# Step 3: Create test file (commit 1)
Write-Step "Creating first commit"
$content1 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("# Test Feature`n`nThis is a test feature."))
$blob1 = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/blobs" -Body @{
    content = $content1
    encoding = "base64"
}

# Get current tree
$masterCommit = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/git/commits/$masterSha"
$treeBody = @{
    base_tree = $masterCommit.tree.sha
    tree = @(
        @{
            path = "tests/features/$BranchName.md"
            mode = "100644"
            type = "blob"
            sha = $blob1.sha
        }
    )
}
$tree = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/trees" -Body $treeBody

# Create commit
$commitBody = @{
    message = "feat(test): add initial test feature`n`nWIP: First iteration"
    tree = $tree.sha
    parents = @($masterSha)
}
$commit1 = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/commits" -Body $commitBody
Write-Success "Commit 1 created: $($commit1.sha.Substring(0,7))"

# Update branch
Invoke-GitHubApi -Method "PATCH" -Endpoint "/repos/$Owner/$Repo/git/refs/heads/$BranchName" -Body @{
    sha = $commit1.sha
}

# Step 4: Create second commit
Write-Step "Creating second commit (WIP)"
$content2 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("# Test Feature`n`nThis is a test feature.`n`n## Updates`n`n- Added more content`n- Fixed typos"))
$blob2 = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/blobs" -Body @{
    content = $content2
    encoding = "base64"
}

$tree2Body = @{
    base_tree = $commit1.tree.sha
    tree = @(
        @{
            path = "tests/features/$BranchName.md"
            mode = "100644"
            type = "blob"
            sha = $blob2.sha
        }
    )
}
$tree2 = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/trees" -Body $tree2Body

$commit2Body = @{
    message = "wip(test): more work on feature`n`nWIP: Second iteration"
    tree = $tree2.sha
    parents = @($commit1.sha)
}
$commit2 = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/commits" -Body $commit2Body
Write-Success "Commit 2 created: $($commit2.sha.Substring(0,7))"

Invoke-GitHubApi -Method "PATCH" -Endpoint "/repos/$Owner/$Repo/git/refs/heads/$BranchName" -Body @{
    sha = $commit2.sha
}

# Step 5: Create third commit
Write-Step "Creating third commit (WIP)"
$content3 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("# Test Feature`n`nThis is a test feature.`n`n## Updates`n`n- Added more content`n- Fixed typos`n- Final polish"))
$blob3 = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/blobs" -Body @{
    content = $content3
    encoding = "base64"
}

$tree3Body = @{
    base_tree = $commit2.tree.sha
    tree = @(
        @{
            path = "tests/features/$BranchName.md"
            mode = "100644"
            type = "blob"
            sha = $blob3.sha
        }
    )
}
$tree3 = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/trees" -Body $tree3Body

$commit3Body = @{
    message = "wip(test): final polish`n`nWIP: Ready for squash"
    tree = $tree3.sha
    parents = @($commit2.sha)
}
$commit3 = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/commits" -Body $commit3Body
Write-Success "Commit 3 created: $($commit3.sha.Substring(0,7))"

Invoke-GitHubApi -Method "PATCH" -Endpoint "/repos/$Owner/$Repo/git/refs/heads/$BranchName" -Body @{
    sha = $commit3.sha
}

Write-Success "Branch now has 3 commits (simulating WIP workflow)"

# Step 6: Squash to 1 commit
Write-Step "Squashing to 1 commit"
$treeFinal = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/git/trees/$($commit3.tree.sha)"

$squashCommitBody = @{
    message = "feat(test): add test feature for workflow validation`n`n- Added test documentation`n- Implemented feature logic`n- Applied final polish`n`nFixes #test"
    tree = $commit3.tree.sha
    parents = @($masterSha)
}
$squashCommit = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/commits" -Body $squashCommitBody
Write-Success "Squash commit created: $($squashCommit.sha.Substring(0,7))"

# Force update branch (squash changes history, not fast-forward)
$updateUri = "$($Config.ApiBase)/repos/$Owner/$Repo/git/refs/heads/$BranchName"
$updateBody = @{ sha = $squashCommit.sha; force = $true } | ConvertTo-Json
Invoke-RestMethod -Uri $updateUri -Method "PATCH" -Headers $Config.Headers -Body $updateBody -ContentType "application/json"
Write-Success "Branch now has 1 commit (squashed)"

# Step 7: Create PR
Write-Step "Creating Pull Request"
$prBody = @{
    title = "TEST: Feature workflow validation"
    body = "This is a test PR to validate the feature workflow.`n`n## Changes`n- Added test feature`n- Validated squash workflow`n`n## Checklist`n- [x] Commits squashed to 1`n- [x] Follows conventional commits`n- [x] References issue`n`nFixes #test"
    head = $BranchName
    base = "develop"
}
$pr = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/pulls" -Body $prBody
Write-Success "PR created: #$($pr.number) - $($pr.html_url)"

# Step 8: Wait for Preview Gate
Write-Step "Waiting for Preview Gate workflow"
try {
    $run = Wait-ForWorkflowRun -Branch $BranchName -WorkflowName "Preview Gate" -TimeoutMinutes 5
    
    if ($run.conclusion -eq "success") {
        Write-Success "Preview Gate passed!"
    }
    else {
        Write-Error "Preview Gate failed: $($run.conclusion)"
        Write-Info "Check: $($run.html_url)"
    }
}
catch {
    Write-Error "Could not verify Preview Gate: $_"
}

# ============ SUMMARY ============

Write-Host "`n================================" -ForegroundColor Blue
Write-Host "Test Summary" -ForegroundColor Blue
Write-Host "================================" -ForegroundColor Blue
Write-Host "Branch: $BranchName"
Write-Host "PR: #$($pr.number)"
Write-Host "URL: $($pr.html_url)"
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Check PR in GitHub"
Write-Host "  2. Verify Preview Gate checks"
Write-Host "  3. Merge PR (squash)"
Write-Host "  4. Verify Preview Publish runs"
Write-Host ""
Write-Host "Cleanup:" -ForegroundColor Yellow
Write-Host "  ./Test-Cleanup.ps1 -Owner $Owner -Repo $Repo -Token `$Token -BranchName $BranchName"
