# GitHub Repository Setup Guide

Guide for configuring the CdCSharp.BlazorUI repository (or any other with the same structure).

## Option 1: PowerShell Script (Recommended)

### 1. Create GitHub Personal Access Token

1. Go to https://github.com/settings/tokens
2. Click **Generate new token (classic)**
3. Select scopes: `repo` (full control)
4. Copy the token (shown only once)

### 2. Run the Script

```powershell
# From repository root
./scripts/github-setup.ps1 `
    -Owner smaicas `
    -Repo CdCSharp.BlazorUI `
    -Token ghp_your_token_here `
    -SetupLabels `
    -SetupBranchProtection
```

Or labels only (if you don't have admin permissions):

```powershell
./scripts/github-setup.ps1 `
    -Owner smaicas `
    -Repo CdCSharp.BlazorUI `
    -Token ghp_your_token_here `
    -SetupLabels
```

Keep default GitHub labels (bug, enhancement, etc.):

```powershell
./scripts/github-setup.ps1 `
    -Owner smaicas `
    -Repo CdCSharp.BlazorUI `
    -Token ghp_your_token_here `
    -SetupLabels `
    -KeepDefaultLabels
```

### 3. Configure NUGET_API_KEY Secret

```powershell
# Using GitHub CLI (optional)
gh secret set NUGET_API_KEY --body "your-api-key-here"
```

Or manually at: **Settings > Secrets and variables > Actions > New repository secret**

---

## Option 2: Manual Configuration

### Step 1: Create Labels

Go to **Issues > Labels > New label** and create:

| Name | Color | Description |
|------|-------|-------------|
| `severity/blocker` | `#d9534f` | Blocks release - must be resolved before publishing |
| `severity/critical` | `#e74c3c` | Critical issue - high priority |
| `severity/major` | `#f39c12` | Major issue - affects functionality |
| `severity/minor` | `#f1c40f` | Minor issue - nice to have improvement |
| `severity/polish` | `#2ecc71` | Polish - quality of life improvement |

### Step 2: Create `develop` Branch (if it doesn't exist)

```bash
# From your local repo
git checkout master
git pull origin master
git checkout -b develop
git push -u origin develop
```

Or the script will create it automatically.

### Step 3: Branch Protection for `master`

Go to **Settings > Branches > Add rule**

**Branch name pattern:** `master`

#### Protect matching branches

- ✅ **Require a pull request before merging**
  - ✅ Require approvals: `1`
  - ✅ Dismiss stale PR approvals when new commits are pushed
  - ⬜ Require review from Code Owners

- ✅ **Require status checks to pass before merging**
  - ✅ Require branches to be up to date before merging
  - Status checks that are required:
    - `Release Gate / Build Check`
    - `Release Gate / Check Blocking Issues`
    - `Release Gate / Check Public API`

- ⬜ Require conversation resolution before merging
- ⬜ Require signed commits
- ✅ **Require linear history** ← IMPORTANT
- ⬜ Require deployments to succeed before merging
- ⬜ Lock branch
- ⬜ Do not allow bypassing the above settings

#### Rules applied to everyone including administrators

- ⬜ Allow force pushes
- ⬜ Allow deletions

### Step 4: Branch Protection for `develop`

Go to **Settings > Branches > Add rule**

**Branch name pattern:** `develop`

#### Protect matching branches

- ✅ **Require a pull request before merging**
  - ✅ Require approvals: `1`
  - ✅ Dismiss stale PR approvals when new commits are pushed

- ✅ **Require status checks to pass before merging**
  - ✅ Require branches to be up to date before merging
  - Status checks that are required:
    - `Preview Gate / Build and Test`
    - `Preview Gate / Code Coverage`
    - `Preview Gate / Check Public API`

- ⬜ Require conversation resolution before merging
- ⬜ Require signed commits
- ✅ **Require linear history** ← IMPORTANT
- ⬜ Require deployments to succeed before merging
- ⬜ Lock branch
- ⬜ Do not allow bypassing the above settings

#### Rules applied to everyone including administrators

- ⬜ Allow force pushes
- ⬜ Allow deletions

### Step 5: Configure Merge

Go to **Settings > General > Pull Requests**

#### Merge Button

- ⬜ Allow merge commits ❌ **Disable**
- ✅ **Allow squash merging** ✅ **Enable (default)**
  - Default commit message: `Default message`
- ⬜ Allow rebase merging ❌ **Disable**

### Step 6: Configure NUGET_API_KEY Secret

Go to **Settings > Secrets and variables > Actions > New repository secret**

- **Name:** `NUGET_API_KEY`
- **Secret:** Your NuGet.org API key

To get the API key:
1. Go to https://www.nuget.org/
2. Login > Account > API Keys
3. Create new key
4. Select scope: Push
5. Select the CdCSharp.BlazorUI package
6. Copy the key

---

## Verification

After configuration, verify:

1. **Labels:** https://github.com/OWNER/REPO/labels (should show 5 severity labels)
2. **Branch protection:** Settings > Branches (should show 2 rules)
3. **Secrets:** Settings > Secrets (should have `NUGET_API_KEY`)
4. **Merge settings:** Squash merge only enabled

---

## Reusing in Other Repositories

The `github-setup.ps1` script is reusable. Just change the parameters:

```powershell
./scripts/github-setup.ps1 -Owner my-org -Repo my-repo -Token ghp_xxx -SetupLabels -SetupBranchProtection
```

**Note:** The status check names (`Release Gate / Build Check`, etc.) must match the workflows in the repository. If you use different names, modify the script or configure manually.
