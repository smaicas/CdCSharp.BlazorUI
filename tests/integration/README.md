# Integration Tests

PowerShell integration tests for the CdCSharp.BlazorUI development workflow.

## Prerequisites

- PowerShell 7.0+
- GitHub Personal Access Token with permissions:
  - `repo` (full control)
  - `workflow` (for workflow runs)

## Quick Start

```powershell
# Set your token
$env:GITHUB_TOKEN = "ghp_your_token_here"

# Run all tests
./Run-AllTests.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN

# Run specific test
./Run-AllTests.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN -Test Setup
```

## Test Scripts

### Test-RepositorySetup.ps1

Verifies repository configuration:
- Repository accessibility
- Severity labels exist (`severity/blocker`, `severity/critical`, etc.)
- Default `bug` label removed
- Branch protection on `master` and `develop`
- Workflows exist
- `develop` branch exists

```powershell
./Test-RepositorySetup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN
```

### Test-FeatureWorkflow.ps1

Simulates a developer working on a feature:
1. Creates feature branch from `master`
2. Makes 3 commits (simulating WIP workflow)
3. Squashes to 1 commit
4. Creates PR to `develop`
5. Waits for Preview Gate

```powershell
./Test-FeatureWorkflow.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN
```

**Output:**
- Branch name created
- PR number and URL
- Instructions for manual verification

### Test-ParallelDevelopment.ps1

Simulates two developers working in parallel:
1. Developer A creates branch and commits
2. Developer B creates branch (from same base) and commits
3. Both create PRs
4. Shows expected conflict when first PR is merged

```powershell
./Test-ParallelDevelopment.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN
```

**Manual steps required:**
1. Merge first PR
2. Observe second PR shows "out-of-date"
3. Rebase second branch
4. Resolve conflicts
5. Merge second PR

### Test-ReleaseGate.ps1

Tests release gate blocking behavior:
1. Creates issues with different severities:
   - `severity/blocker` (should block)
   - `severity/critical` (should block)
   - `severity/major` (should NOT block)
   - `severity/minor` (should NOT block)
2. Creates PR to `master`
3. Verifies Release Gate fails with blocker issues

```powershell
./Test-ReleaseGate.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN
```

**Manual steps required:**
1. Check PR fails Release Gate
2. Close blocker issue
3. Re-run Release Gate (should still fail)
4. Close critical issue
5. Re-run Release Gate (should pass)

### Test-Cleanup.ps1

Cleans up test artifacts:

```powershell
# Delete specific branch
./Test-Cleanup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN -BranchName "test-feature-123"

# Close specific issue
./Test-Cleanup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN -IssueNumber 42

# Close specific PR
./Test-Cleanup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN -PrNumber 10

# Cleanup ALL test artifacts
./Test-Cleanup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN -CleanupAll
```

## Test Execution Order

Recommended order for complete validation:

1. **Setup Test** - Verify repository is ready
2. **Feature Workflow Test** - Basic flow validation
3. **Parallel Development Test** - Conflict scenario
4. **Release Gate Test** - Blocking validation
5. **Cleanup** - Remove all test artifacts

## Expected Behavior

### Repository Setup
- ✅ All severity labels exist
- ✅ Default `bug` label removed
- ✅ Branch protection enabled on `master` and `develop`
- ✅ Required status checks configured
- ✅ Linear history enforced

### Feature Workflow
- ✅ Feature branch created from `master`
- ✅ Multiple commits squashed to 1
- ✅ PR created to `develop`
- ✅ Preview Gate runs automatically
- ✅ Preview Gate passes

### Parallel Development
- ✅ Two branches from same base
- ✅ First PR merges cleanly
- ✅ Second PR shows "out-of-date"
- ✅ Rebase required with conflicts

### Release Gate
- ✅ Release Gate fails with `severity/blocker` open
- ✅ Release Gate fails with `severity/critical` open
- ✅ Release Gate passes when both closed
- ✅ `severity/major` and `severity/minor` don't block

## Troubleshooting

### "API call failed"
- Verify token has `repo` permission
- Check token is not expired

### "Branch protection not found"
- Run `github-setup.ps1` first
- Or configure manually in GitHub settings

### "Workflow not running"
- Check Actions are enabled in repository
- Verify workflow files exist in `.github/workflows/`

## Notes

- Tests create real branches, issues, and PRs
- Clean up after testing with `Test-Cleanup.ps1`
- Tests are idempotent (can run multiple times)
- Some tests require manual verification in GitHub UI
