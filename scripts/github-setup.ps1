#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Configures a GitHub repository with labels and branch protection

.DESCRIPTION
    Reusable script to configure:
    - Severity labels (blocker, critical, major, minor, polish)
    - Branch protection for master and develop
    - Squash merge only, linear history

.PARAMETER Owner
    Repository owner (e.g.: smaicas)

.PARAMETER Repo
    Repository name (e.g.: CdCSharp.BlazorUI)

.PARAMETER Token
    GitHub Personal Access Token with permissions: repo, workflow

.PARAMETER SetupLabels
    Create severity labels

.PARAMETER SetupBranchProtection
    Configure branch protection

.PARAMETER KeepDefaultLabels
    Keep default GitHub labels (bug, enhancement, etc.)
    By default, these are removed and replaced with severity labels

.EXAMPLE
    ./github-setup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token ghp_xxx -SetupLabels -SetupBranchProtection

.EXAMPLE
    ./github-setup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN -SetupLabels

.EXAMPLE
    ./github-setup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token ghp_xxx -SetupLabels -KeepDefaultLabels

.NOTES
    The token needs admin permissions for branch protection.
    If you don't have admin permissions, use -SetupLabels only.
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
    [switch]$SetupLabels,
    
    [Parameter()]
    [switch]$SetupBranchProtection,
    
    [Parameter()]
    [switch]$KeepDefaultLabels
)

# Configuration
$Config = @{
    ApiBase = "https://api.github.com"
    Headers = @{
        Authorization = "token $Token"
        Accept = "application/vnd.github+json"
        "X-GitHub-Api-Version" = "2022-11-28"
    }
    DefaultBranch = $null  # Auto-detected
}

# Colors for output
$Colors = @{
    Success = "Green"
    Warning = "Yellow"
    Error = "Red"
    Info = "Cyan"
}

function Write-Step {
    param([string]$Message)
    Write-Host "`n=== $Message ===" -ForegroundColor $Colors.Info
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $Colors.Success
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor $Colors.Warning
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $Colors.Error
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
        $response = Invoke-RestMethod @params
        return $response
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $errorMessage = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
        
        if ($statusCode -eq 404) {
            return $null  # Resource does not exist
        }
        
        Write-Error "API Error ${statusCode}: $($errorMessage.message)"
        return $false
    }
}

function Test-RepositoryAccess {
    Write-Step "Verifying access to repository $Owner/$Repo"
    
    $repo = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo"
    
    if (-not $repo) {
        Write-Error "Cannot access repository. Verify:"
        Write-Host "  - Owner and repository name are correct"
        Write-Host "  - Token has 'repo' permissions"
        exit 1
    }
    
    # Store default branch
    $Config.DefaultBranch = $repo.default_branch
    
    Write-Success "Access verified: $($repo.full_name)"
    Write-Host "  Private: $($repo.private)"
    Write-Host "  Default branch: $($Config.DefaultBranch)"
}

function Initialize-DevelopBranch {
    Write-Step "Verifying develop branch"
    
    # Check if develop exists
    $develop = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/branches/develop"
    
    if ($develop) {
        Write-Success "Develop branch already exists"
        return $true
    }
    
    Write-Warning "Develop branch does not exist. Creating from $($Config.DefaultBranch)..."
    
    # Get SHA of latest commit on default branch
    $defaultBranchRef = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/git/refs/heads/$($Config.DefaultBranch)"
    
    if (-not $defaultBranchRef) {
        Write-Error "Could not get reference for $($Config.DefaultBranch)"
        return $false
    }
    
    $sha = $defaultBranchRef.object.sha
    
    # Create develop branch
    $body = @{
        ref = "refs/heads/develop"
        sha = $sha
    }
    
    $result = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/refs" -Body $body
    
    if ($result -eq $false) {
        Write-Error "Could not create develop branch"
        return $false
    }
    
    Write-Success "Develop branch created from $($Config.DefaultBranch)"
    return $true
}

function Remove-BugLabel {
    param([switch]$KeepBugLabel)
    
    if ($KeepBugLabel) {
        return
    }
    
    $encodedName = [Uri]::EscapeDataString("bug")
    $existing = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/labels/$encodedName"
    
    if ($existing) {
        Write-Host "Removing default 'bug' label"
        $result = Invoke-GitHubApi -Method "DELETE" -Endpoint "/repos/$Owner/$Repo/labels/$encodedName"
        if ($result -eq $false) {
            Write-Warning "Could not remove 'bug' label"
        }
    }
}

function Initialize-Labels {
    Write-Step "Configuring Labels"
    
    # Remove default 'bug' label (unless -KeepDefaultLabels specified)
    Remove-BugLabel -KeepBugLabel:$KeepDefaultLabels
    
    $labels = @(
        @{ Name = "severity/blocker"; Color = "d9534f"; Description = "Blocks release - must be resolved before publishing" }
        @{ Name = "severity/critical"; Color = "e74c3c"; Description = "Critical issue - high priority" }
        @{ Name = "severity/major"; Color = "f39c12"; Description = "Major issue - affects functionality" }
        @{ Name = "severity/minor"; Color = "f1c40f"; Description = "Minor issue - nice to have improvement" }
        @{ Name = "severity/polish"; Color = "2ecc71"; Description = "Polish - quality of life improvement" }
        @{ Name = "changes/public-api"; Color = "5319e7"; Description = "Changes to public API - triggers MINOR version bump on release" }
    )
    
    foreach ($label in $labels) {
        # Check if exists
        $existing = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/labels/$($label.Name -replace '/', '%2F')"
        
        $body = @{
            name = $label.Name
            color = $label.Color
            description = $label.Description
        }
        
        if ($existing) {
            Write-Host "Updating label: $($label.Name)"
            $result = Invoke-GitHubApi -Method "PATCH" -Endpoint "/repos/$Owner/$Repo/labels/$($label.Name -replace '/', '%2F')" -Body $body
        }
        else {
            Write-Host "Creating label: $($label.Name)"
            $result = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/labels" -Body $body
        }
        
        if ($result -eq $false) {
            Write-Warning "Could not configure label: $($label.Name)"
        }
    }
    
    Write-Success "Labels configured"
}

function Initialize-BranchProtection {
    param(
        [string]$Branch,
        [string[]]$RequiredContexts
    )
    
    Write-Step "Configuring branch protection for: $Branch"
    
    $body = @{
        required_status_checks = @{
            strict = $true
            contexts = $RequiredContexts
        }
        enforce_admins = $false
        required_pull_request_reviews = @{
            required_approving_review_count = 1
            dismiss_stale_reviews = $true
            require_code_owner_reviews = $false
        }
        restrictions = $null
        allow_force_pushes = $false
        allow_deletions = $false
        required_linear_history = $true
        allow_merge_commit = $false
        allow_squash_merge = $true
        allow_rebase_merge = $false
    }
    
    $result = Invoke-GitHubApi -Method "PUT" -Endpoint "/repos/$Owner/$Repo/branches/$Branch/protection" -Body $body
    
    if ($result -eq $false) {
        Write-Error "Could not configure branch protection for $Branch"
        Write-Host "Possible causes:"
        Write-Host "  - You don't have admin permissions"
        Write-Host "  - Token doesn't have 'repo' scope"
        Write-Host "  - Branch $Branch doesn't exist"
        return $false
    }
    
    Write-Success "Branch protection configured for $Branch"
    return $true
}

function Show-Summary {
    Write-Step "Configuration Summary"
    
    Write-Host "Repository: $Owner/$Repo" -ForegroundColor $Colors.Info
    Write-Host ""
    
    if ($SetupLabels) {
        Write-Host "Labels configured:" -ForegroundColor $Colors.Success
        Write-Host "  - severity/blocker (🔴 Blocks release)"
        Write-Host "  - severity/critical (🔴 Critical)"
        Write-Host "  - severity/major (🟠 Major)"
        Write-Host "  - severity/minor (🟡 Minor)"
        Write-Host "  - severity/polish (🟢 Polish)"
        Write-Host ""
    }
    
    if ($SetupBranchProtection) {
        Write-Host "Branch protection:" -ForegroundColor $Colors.Success
        Write-Host "  $($Config.DefaultBranch):" -ForegroundColor $Colors.Info
        Write-Host "    - Requires PR + 1 approval"
        Write-Host "    - Requires: Release Gate / Build Check"
        Write-Host "    - Requires: Release Gate / Check Blocking Issues"
        Write-Host "    - Requires: Release Gate / Check Public API"
        Write-Host "    - Squash merge only + linear history"
        Write-Host ""
        Write-Host "  develop:" -ForegroundColor $Colors.Info
        Write-Host "    - Requires PR + 1 approval"
        Write-Host "    - Requires: Preview Gate / Build and Test"
        Write-Host "    - Requires: Preview Gate / Code Coverage"
        Write-Host "    - Requires: Preview Gate / Check Public API"
        Write-Host "    - Squash merge only + linear history"
    }
    
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor $Colors.Warning
    Write-Host "  1. Configure NUGET_API_KEY in Settings > Secrets"
    Write-Host "  2. Verify workflows in Actions"
    Write-Host "  3. Test with a sample PR"
}

# ============ MAIN ============

# Validate parameters
if (-not $SetupLabels -and -not $SetupBranchProtection) {
    Write-Error "You must specify at least one option: -SetupLabels or -SetupBranchProtection"
    Write-Host "Usage: ./github-setup.ps1 -Owner <owner> -Repo <repo> -Token <token> -SetupLabels -SetupBranchProtection"
    exit 1
}

# Verify access
Test-RepositoryAccess

# Configure labels
if ($SetupLabels) {
    Initialize-Labels
}

# Configure branch protection
if ($SetupBranchProtection) {
    # Create develop if it doesn't exist
    $developCreated = Initialize-DevelopBranch
    
    # Configure branch protection for default branch (master/main)
    $mainContexts = @(
        "Release Gate / Build Check"
        "Release Gate / Check Blocking Issues"
        "Release Gate / Check Public API"
    )
    
    $developContexts = @(
        "Preview Gate / Build and Test"
        "Preview Gate / Code Coverage"
        "Preview Gate / Check Public API"
    )
    
    $mainOk = Initialize-BranchProtection -Branch $Config.DefaultBranch -RequiredContexts $mainContexts
    $developOk = Initialize-BranchProtection -Branch "develop" -RequiredContexts $developContexts
    
    if (-not $mainOk -or -not $developOk) {
        Write-Warning "Some branch protection configurations failed."
        Write-Host "Configure manually at: https://github.com/$Owner/$Repo/settings/branches"
    }
}

# Show summary
Show-Summary

Write-Host "`nDone!" -ForegroundColor $Colors.Success
