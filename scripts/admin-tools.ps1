#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Herramientas de administración para releases de CdCSharp.BlazorUI
    ESTRATEGIA: Squash + Rebase, historial lineal

.DESCRIPTION
    Script para mantenedores/administradores.
    Gestiona releases, verifica calidad de PRs, y mantiene el flujo lineal.

.EXAMPLE
    ./admin-tools.ps1 status
    Muestra estado de ramas, PRs pendientes, versiones

.EXAMPLE
    ./admin-tools.ps1 check-pr <branch-name>
    Verifica si una PR cumple los requisitos (1 commit, rebase hecho)

.EXAMPLE
    ./admin-tools.ps1 rc 1.0.0
    Crea una release candidate

.EXAMPLE
    ./admin-tools.ps1 release 1.0.0
    Publica release estable (merge a main + tag)

.NOTES
    Autor: Samuel Maícas (@cdcsharp)
    Versión: 2.0.0 - Estrategia Squash+Rebase
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet("status", "check-pr", "rc", "release", "hotfix", "changelog", "cleanup")]
    [string]$Command,

    [Parameter(Position = 1)]
    [string]$Name,

    [Parameter()]
    [switch]$Force,

    [Parameter()]
    [switch]$DryRun
)

# Configuración
$Config = @{
    MainBranch = "main"
    DevelopBranch = "develop"
    Remote = "origin"
}

# Colores
$Colors = @{
    Success = "Green"
    Warning = "Yellow"
    Error = "Red"
    Info = "Cyan"
    Emphasis = "Magenta"
}

#region Funciones Auxiliares

function Write-Header {
    param([string]$Message)
    Write-Host "`n=== $Message ===" -ForegroundColor $Colors.Emphasis
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

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor $Colors.Info
}

function Test-GitRepository {
    try {
        $null = git rev-parse --git-dir 2>$null
        return $true
    }
    catch {
        return $false
    }
}

function Get-LastTag {
    try {
        $tag = git describe --tags --abbrev=0 --match "v[0-9]*.[0-9]*.[0-9]" $Config.MainBranch 2>$null
        if ($tag) { return $tag }
    }
    catch { }
    return "v0.0.0"
}

function Get-CommitsSinceTag {
    param([string]$Tag)
    $count = git rev-list --count "$Tag..HEAD" 2>$null
    if ($count) { return [int]$count }
    return 0
}

function Invoke-GitCommand {
    param(
        [string]$Command,
        [string]$Arguments,
        [switch]$IgnoreError
    )
    
    try {
        $output = git $Command $Arguments 2>&1
        if ($LASTEXITCODE -ne 0 -and -not $IgnoreError) {
            Write-Error "git $Command failed: $output"
            return $false
        }
        return $output
    }
    catch {
        if (-not $IgnoreError) {
            Write-Error "git $Command failed: $_"
        }
        return $false
    }
}

function Show-Status {
    Write-Header "Estado del Repositorio"
    
    # Información de versión
    $lastTag = Get-LastTag
    Write-Info "Último tag en main: $lastTag"
    
    $commitsSince = Get-CommitsSinceTag -Tag $lastTag
    Write-Info "Commits desde $lastTag`: $commitsSince"
    
    # Estado de ramas
    Write-Host "`n--- Ramas Principales ---" -ForegroundColor $Colors.Emphasis
    
    foreach ($branch in @($Config.MainBranch, $Config.DevelopBranch)) {
        $exists = git ls-remote --heads $Config.Remote $branch 2>$null
        if ($exists) {
            $ahead = git rev-list --count "$Config.Remote/$branch..$branch" 2>$null
            $behind = git rev-list --count "$branch..$Config.Remote/$branch" 2>$null
            
            $status = if ($ahead -gt 0) { "+$ahead local" }
                     elseif ($behind -gt 0) { "-$behind remote" }
                     else { "✓ sync" }
            
            $color = if ($ahead -gt 0 -or $behind -gt 0) { $Colors.Warning } else { $Colors.Success }
            Write-Host "$branch`: " -NoNewline
            Write-Host $status -ForegroundColor $color
        }
    }
    
    # Ramas de feature/release/hotfix
    Write-Host "`n--- Ramas Activas ---" -ForegroundColor $Colors.Emphasis
    $featureBranches = git branch -r --list "$($Config.Remote)/feature/*" "$($Config.Remote)/fix/*" 2>$null
    $releaseBranches = git branch -r --list "$($Config.Remote)/release/*" "$($Config.Remote)/hotfix/*" 2>$null
    
    if ($featureBranches) {
        Write-Host "Features/Fixes:"
        $featureBranches | ForEach-Object { Write-Host "  $_" }
    }
    
    if ($releaseBranches) {
        Write-Host "Releases/Hotfixes:"
        $releaseBranches | ForEach-Object { Write-Host "  $_" }
    }
    
    if (-not $featureBranches -and -not $releaseBranches) {
        Write-Info "No hay ramas activas"
    }
    
    # Verificar PRs listos para merge
    Write-Host "`n--- PRs Listos para Merge ---" -ForegroundColor $Colors.Emphasis
    # Esto requeriría GitHub CLI, mostramos instrucciones
    Write-Info "Usa GitHub para ver PRs: https://github.com/$($Config.Remote)/pulls"
}

function Check-PR {
    param([string]$BranchName)
    
    if (-not $BranchName) {
        Write-Error "Nombre de rama requerido. Uso: ./admin-tools.ps1 check-pr feature/nombre"
        exit 1
    }
    
    Write-Header "Verificando PR: $BranchName"
    
    # Fetch
    Invoke-GitCommand -Command "fetch" -Arguments $Config.Remote | Out-Null
    
    # Verificar que existe
    $exists = git ls-remote --heads $Config.Remote $BranchName 2>$null
    if (-not $exists) {
        Write-Error "La rama $BranchName no existe en $($Config.Remote)"
        exit 1
    }
    
    # Contar commits
    $base = git merge-base "$Config.Remote/$BranchName" "$Config.Remote/$($Config.DevelopBranch)" 2>$null
    $commitCount = git rev-list --count "$base..$Config.Remote/$BranchName" 2>$null
    
    Write-Info "Commits en la rama: $commitCount"
    
    if ($commitCount -eq 1) {
        Write-Success "✓ Solo 1 commit (squash correcto)"
    }
    else {
        Write-Error "✗ Hay $commitCount commits. Debe hacerse squash a 1."
        Write-Info "Instrucciones para el desarrollador:"
        Write-Host "  ./dev-tools.ps1 squash"
        return
    }
    
    # Verificar si está up-to-date
    $behind = git rev-list --count "$Config.Remote/$BranchName..$Config.Remote/$($Config.DevelopBranch)" 2>$null
    
    if ($behind -eq 0) {
        Write-Success "✓ Rama up-to-date con develop"
    }
    else {
        Write-Error "✗ Rama está $behind commits detrás de develop"
        Write-Info "El desarrollador debe ejecutar:"
        Write-Host "  ./dev-tools.ps1 ready"
        return
    }
    
    # Verificar mensaje de commit
    $commitMsg = git log -1 --pretty=%B "$Config.Remote/$BranchName" 2>$null
    Write-Host "`nMensaje del commit:" -ForegroundColor $Colors.Info
    Write-Host $commitMsg
    
    if ($commitMsg -match '^(feat|fix|docs|test|refactor|chore|breaking)(\([^)]+\))?:\s.+') {
        Write-Success "✓ Mensaje sigue conventional commits"
    }
    else {
        Write-Warning "⚠ Mensaje no sigue conventional commits"
    }
    
    if ($commitMsg -match 'Fixes\s+#\d+') {
        Write-Success "✓ Referencia a issue encontrada"
    }
    else {
        Write-Warning "⚠ No hay referencia a issue (Fixes #XXX)"
    }
    
    Write-Host "`n--- Resumen ---" -ForegroundColor $Colors.Emphasis
    Write-Success "La PR está lista para mergear"
    Write-Info "En GitHub: Selecciona 'Squash and merge' (aunque ya sea 1 commit, para mantener consistencia)"
}

function New-ReleaseCandidate {
    param([string]$Version)
    
    Write-Header "Creando Release Candidate $Version"
    
    # Validar formato
    if ($Version -notmatch '^\d+\.\d+\.\d+$') {
        Write-Error "Formato inválido. Use: X.Y.Z (ej: 1.0.0)"
        exit 1
    }
    
    # Verificar working directory
    $status = git status --porcelain 2>$null
    if ($status) {
        Write-Error "Working directory no está limpio"
        exit 1
    }
    
    $releaseBranch = "release/$Version"
    
    # Checkout develop
    Write-Info "Actualizando $($Config.DevelopBranch)..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.DevelopBranch
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "pull" -Arguments "$($Config.Remote) $($Config.DevelopBranch)"
    if (-not $result) { exit 1 }
    
    # Crear rama release
    Write-Info "Creando rama $releaseBranch..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments "-b $releaseBranch"
    if (-not $result) { exit 1 }
    
    # Push
    Write-Info "Push a $($Config.Remote)..."
    $result = Invoke-GitCommand -Command "push" -Arguments "-u $($Config.Remote) $releaseBranch"
    if (-not $result) { exit 1 }
    
    Write-Success "Release branch $releaseBranch creada"
    Write-Info "Ahora puedes:"
    Write-Host "  1. Hacer bugfixes en esta rama (solo fixes, no features)"
    Write-Host "  2. Publicar RC: git tag v$Version-rc.1 && git push origin v$Version-rc.1"
    Write-Host "  3. Cuando esté lista: ./admin-tools.ps1 release $Version"
}

function Publish-Release {
    param([string]$Version)
    
    Write-Header "Publicando Release $Version"
    
    if ($Version -notmatch '^\d+\.\d+\.\d+$') {
        Write-Error "Formato inválido. Use: X.Y.Z"
        exit 1
    }
    
    $releaseBranch = "release/$Version"
    $tag = "v$Version"
    
    # Verificar que existe release branch
    $exists = git branch --list $releaseBranch 2>$null
    if (-not $exists) {
        Write-Error "La rama $releaseBranch no existe. Crea la RC primero."
        exit 1
    }
    
    # Confirmación
    if (-not $Force) {
        Write-Warning "Esto va a:"
        Write-Host "  1. Mergear $releaseBranch a $($Config.MainBranch) (squash)"
        Write-Host "  2. Crear tag $tag"
        Write-Host "  3. Push a $($Config.Remote)"
        Write-Host "  4. Merge back a $($Config.DevelopBranch)"
        $confirm = Read-Host "`n¿Continuar? (escribe 'yes' para confirmar)"
        if ($confirm -ne "yes") {
            Write-Info "Cancelado"
            exit 0
        }
    }
    
    # Checkout release
    Write-Info "Checkout $releaseBranch..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments $releaseBranch
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "pull" -Arguments "$($Config.Remote) $releaseBranch"
    if (-not $result) { exit 1 }
    
    # Checkout main
    Write-Info "Checkout $($Config.MainBranch)..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.MainBranch
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "pull" -Arguments "$($Config.Remote) $Config.MainBranch"
    if (-not $result) { exit 1 }
    
    # Merge squash de release
    Write-Info "Merge squash de $releaseBranch..."
    $result = Invoke-GitCommand -Command "merge" -Arguments "--squash $releaseBranch"
    if (-not $result) { exit 1 }
    
    # Commit
    $result = Invoke-GitCommand -Command "commit" -Arguments "-m \"Release $Version\""
    if (-not $result) { exit 1 }
    
    # Tag
    Write-Info "Creando tag $tag..."
    $result = Invoke-GitCommand -Command "tag" -Arguments "-a $tag -m \"Release $Version\""
    if (-not $result) { exit 1 }
    
    # Push
    Write-Info "Push $($Config.MainBranch) y tag..."
    $result = Invoke-GitCommand -Command "push" -Arguments "$($Config.Remote) $Config.MainBranch"
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "push" -Arguments "$($Config.Remote) $tag"
    if (-not $result) { exit 1 }
    
    # Merge back a develop
    Write-Info "Merge back a $($Config.DevelopBranch)..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.DevelopBranch
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "pull" -Arguments "$($Config.Remote) $Config.DevelopBranch"
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "merge" -Arguments "$Config.MainBranch --no-edit"
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "push" -Arguments "$($Config.Remote) $Config.DevelopBranch"
    if (-not $result) { exit 1 }
    
    # Cleanup
    Write-Info "Limpiando..."
    Invoke-GitCommand -Command "branch" -Arguments "-d $releaseBranch" -IgnoreError | Out-Null
    Invoke-GitCommand -Command "push" -Arguments "$($Config.Remote) --delete $releaseBranch" -IgnoreError | Out-Null
    
    Write-Success "Release $Version publicada"
    Write-Info "El CI debería publicar el paquete a NuGet"
}

function New-Hotfix {
    param([string]$Version)
    
    Write-Header "Creando Hotfix $Version"
    
    if ($Version -notmatch '^\d+\.\d+\.\d+$') {
        Write-Error "Formato inválido. Use: X.Y.Z"
        exit 1
    }
    
    $hotfixBranch = "hotfix/$Version"
    
    # Checkout main
    Write-Info "Actualizando $($Config.MainBranch)..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.MainBranch
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "pull" -Arguments "$($Config.Remote) $Config.MainBranch"
    if (-not $result) { exit 1 }
    
    # Crear rama
    Write-Info "Creando rama $hotfixBranch..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments "-b $hotfixBranch"
    if (-not $result) { exit 1 }
    
    # Push
    Write-Info "Push a $($Config.Remote)..."
    $result = Invoke-GitCommand -Command "push" -Arguments "-u $($Config.Remote) $hotfixBranch"
    if (-not $result) { exit 1 }
    
    Write-Success "Hotfix branch $hotfixBranch creada"
    Write-Info "Haz el fix, commit, y luego: git tag v$Version && git push origin v$Version"
}

function Show-Changelog {
    Write-Header "Changelog Pendiente"
    
    $lastTag = Get-LastTag
    Write-Info "Último tag: $lastTag"
    
    $commits = git log "$lastTag..$($Config.Remote)/$($Config.DevelopBranch)" --pretty=format:"%h %s" --no-merges 2>$null
    
    if (-not $commits) {
        Write-Info "No hay commits nuevos en develop"
        return
    }
    
    Write-Host "`nCommits desde $lastTag`:" -ForegroundColor $Colors.Emphasis
    
    # Agrupar por tipo
    $types = @{
        'feat' = @()
        'fix' = @()
        'docs' = @()
        'test' = @()
        'refactor' = @()
        'chore' = @()
        'breaking' = @()
        'other' = @()
    }
    
    $commits -split "`n" | ForEach-Object {
        if ($_ -match '^(\w+)(\([^)]+\))?:\s*(.+)$') {
            $type = $matches[1]
            $msg = $matches[3]
            if ($types.ContainsKey($type)) {
                $types[$type] += $msg
            }
            else {
                $types['other'] += $msg
            }
        }
    }
    
    $labels = @{
        'feat' = '✨ Features'
        'fix' = '🐛 Bug Fixes'
        'docs' = '📚 Documentation'
        'test' = '🧪 Tests'
        'refactor' = '♻️ Refactoring'
        'chore' = '🔧 Maintenance'
        'breaking' = '⚠️ Breaking Changes'
        'other' = '📝 Other'
    }
    
    foreach ($type in $types.Keys) {
        if ($types[$type].Count -gt 0) {
            Write-Host "`n### $($labels[$type])" -ForegroundColor $Colors.Emphasis
            $types[$type] | ForEach-Object { Write-Host "  - $_" }
        }
    }
}

function Invoke-Cleanup {
    Write-Header "Limpiando Ramas"
    
    # Checkout develop
    $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.DevelopBranch
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "pull" -Arguments "$($Config.Remote) $Config.DevelopBranch"
    if (-not $result) { exit 1 }
    
    # Eliminar ramas mergeadas
    $merged = git branch --merged $Config.DevelopBranch --format="%(refname:short)" | Where-Object { 
        $_ -notin @($Config.MainBranch, $Config.DevelopBranch) -and $_ -notmatch "^\*"
    }
    
    if ($merged) {
        Write-Info "Eliminando ramas mergeadas:"
        $merged | ForEach-Object {
            Write-Host "  - $_"
            Invoke-GitCommand -Command "branch" -Arguments "-d $_" -IgnoreError | Out-Null
        }
    }
    
    # Prune
    Invoke-GitCommand -Command "remote" -Arguments "prune $($Config.Remote)" -IgnoreError | Out-Null
    
    Write-Success "Limpieza completada"
}

#endregion

#region Main

if (-not (Test-GitRepository)) {
    Write-Error "No estás en un repositorio Git"
    exit 1
}

switch ($Command) {
    "status" { Show-Status }
    "check-pr" {
        if (-not $Name) {
            Write-Error "Nombre de rama requerido. Uso: ./admin-tools.ps1 check-pr feature/nombre"
            exit 1
        }
        Check-PR -BranchName $Name
    }
    "rc" {
        if (-not $Name) {
            Write-Error "Versión requerida. Uso: ./admin-tools.ps1 rc 1.0.0"
            exit 1
        }
        New-ReleaseCandidate -Version $Name
    }
    "release" {
        if (-not $Name) {
            Write-Error "Versión requerida. Uso: ./admin-tools.ps1 release 1.0.0"
            exit 1
        }
        Publish-Release -Version $Name
    }
    "hotfix" {
        if (-not $Name) {
            Write-Error "Versión requerida. Uso: ./admin-tools.ps1 hotfix 1.0.1"
            exit 1
        }
        New-Hotfix -Version $Name
    }
    "changelog" { Show-Changelog }
    "cleanup" { Invoke-Cleanup }
}

Write-Host "`nDone!" -ForegroundColor $Colors.Success

#endregion
