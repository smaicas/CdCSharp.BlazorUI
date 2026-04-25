#!/usr/bin/env pwsh
# REL-03: release gate enforcement.
#
# Parses TASKS.md, walks every `### \`XXX-NN\`` block, and fails when any
# task with severity Blocker or Critical does not carry an `Estado:`
# line containing "Resuelto". Designed to run in release-publish.yml
# before the actual publish step.
#
# Severity Major and below are reported as a soft warning so the operator
# can see the residual surface but does not block on them.
#
# Exit codes:
#   0  no Blocker/Critical pending
#   1  Blocker/Critical pending
#   2  TASKS.md missing or unparseable

[CmdletBinding()]
param(
    [string]$Path = (Join-Path $PSScriptRoot '..' 'TASKS.md'),
    [switch]$IncludeMajor
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) {
    Write-Error "TASKS.md not found at $Path"
    exit 2
}

# Parse blocks: split on `### \`XXX-NN\`` boundaries.
$content = Get-Content -Path $Path -Raw
$blocks = [regex]::Split($content, '(?m)^(?=###\s+`[A-Z][A-Z0-9-]*-\d+`)')

$blocking = New-Object System.Collections.Generic.List[hashtable]
$majorPending = New-Object System.Collections.Generic.List[hashtable]
$counts = @{ Blocker = 0; Critical = 0; Major = 0; Minor = 0; Polish = 0; Resolved = 0; Unknown = 0 }

foreach ($block in $blocks) {
    $header = [regex]::Match($block, '^###\s+`([A-Z][A-Z0-9-]*-\d+)`\s+—?\s*(.*?)$', 'Multiline')
    if (-not $header.Success) { continue }
    $id = $header.Groups[1].Value
    $title = $header.Groups[2].Value.Trim()

    $sevMatch = [regex]::Match($block, '(?m)^\-\s*\*\*Severidad\*\*:\s*([A-Za-z]+)')
    $stateMatch = [regex]::Match($block, '(?m)^\-\s*\*\*Estado\*\*:\s*(.+)$')

    $severity = if ($sevMatch.Success) { $sevMatch.Groups[1].Value } else { 'Unknown' }
    $stateLine = if ($stateMatch.Success) { $stateMatch.Groups[1].Value } else { $null }
    $resolved = $stateLine -and ($stateLine -match 'Resuelto|RESUELTO|Wontfix|Won''t fix')

    if ($counts.ContainsKey($severity)) { $counts[$severity]++ } else { $counts['Unknown']++ }
    if ($resolved) { $counts['Resolved']++ }

    $entry = @{ Id = $id; Title = $title; Severity = $severity; State = $stateLine }

    if (-not $resolved) {
        if ($severity -eq 'Blocker' -or $severity -eq 'Critical') {
            $blocking.Add($entry)
        }
        elseif ($severity -eq 'Major') {
            $majorPending.Add($entry)
        }
    }
}

Write-Host "TASKS.md parse summary:"
Write-Host ("  Blocker:  {0}" -f $counts.Blocker)
Write-Host ("  Critical: {0}" -f $counts.Critical)
Write-Host ("  Major:    {0}" -f $counts.Major)
Write-Host ("  Minor:    {0}" -f $counts.Minor)
Write-Host ("  Polish:   {0}" -f $counts.Polish)
Write-Host ("  Unknown:  {0}" -f $counts.Unknown)
Write-Host ("  Resolved or Wontfix: {0}" -f $counts.Resolved)
Write-Host ''

if ($IncludeMajor.IsPresent -and $majorPending.Count -gt 0) {
    Write-Host "Pending Major tasks (informational):"
    foreach ($e in $majorPending) {
        Write-Host ("  [{0}] {1}" -f $e.Id, $e.Title)
    }
    Write-Host ''
}

if ($blocking.Count -gt 0) {
    Write-Host "::error::Release gate failed — $($blocking.Count) Blocker/Critical task(s) unresolved:"
    foreach ($e in $blocking) {
        Write-Host ("  [{0}] ({1}) {2}" -f $e.Id, $e.Severity, $e.Title)
    }
    exit 1
}

Write-Host "OK — no Blocker/Critical pending."
