#!/usr/bin/env pwsh
# CLAUDE-03: lint TASKS.md for dangling task IDs.
#
# Scans TASKS.md for every `XXX-NN` style reference inside backticks (the
# convention used to mention another task) and asserts that a matching
# `### `XXX-NN`` header exists in the same file. Exits non-zero if any
# reference points to a task ID that was never defined.

[CmdletBinding()]
param(
    [string]$Path = (Join-Path $PSScriptRoot '..' 'TASKS.md')
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) {
    Write-Error "TASKS.md not found at $Path"
    exit 2
}

$content = Get-Content -Path $Path -Raw

# Defined task IDs: lines starting with "### `XXX-NN`"
$defined = [System.Collections.Generic.HashSet[string]]::new()
foreach ($m in [regex]::Matches($content, '(?m)^###\s+`([A-Z][A-Z0-9-]*-\d+)`')) {
    [void]$defined.Add($m.Groups[1].Value)
}

# Referenced task IDs: any backticked `XXX-NN` token. Strip the defining
# headers themselves so we only audit cross-references in body text.
$bodyOnly = [regex]::Replace($content, '(?m)^###\s+`[A-Z][A-Z0-9-]*-\d+`.*$', '')
$referenced = [System.Collections.Generic.HashSet[string]]::new()
foreach ($m in [regex]::Matches($bodyOnly, '`([A-Z][A-Z0-9-]*-\d+)`')) {
    [void]$referenced.Add($m.Groups[1].Value)
}

$missing = @($referenced | Where-Object { -not $defined.Contains($_) } | Sort-Object)

if ($missing.Count -gt 0) {
    Write-Host "::error::TASKS.md references task IDs without a matching ### header:"
    foreach ($id in $missing) {
        Write-Host "  - $id"
    }
    exit 1
}

Write-Host "OK — $($defined.Count) defined, $($referenced.Count) referenced, 0 dangling."
