#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Herramientas de desarrollo para colaboradores de CdCSharp.BlazorUI
    ESTRATEGIA: Cada PR = 1 commit (squash) + rebase antes de mergear

.DESCRIPTION
    Script para desarrolladores colaboradores.
    Facilita el flujo: feature → squash a 1 commit → rebase → PR

.EXAMPLE
    ./dev-tools.ps1 feature css-tokens
    Crea una nueva feature branch desde develop

.EXAMPLE
    ./dev-tools.ps1 commit "feat(css): add scrollbar tokens"
    Commit con convención + referencia a issue

.EXAMPLE
    ./dev-tools.ps1 squash
    Colapsa todos los commits de la feature en 1

.EXAMPLE
    ./dev-tools.ps1 ready
    Prepara para PR: rebase + verificación de 1 commit

.EXAMPLE
    ./dev-tools.ps1 fix-conflict
    Después de resolver conflictos, squash de los fixups

.NOTES
    Autor: Samuel Maícas (@cdcsharp)
    Versión: 2.0.0 - Estrategia Squash+Rebase
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet("sync", "feature", "fix", "commit", "squash", "ready", "fix-conflict", "push", "pr", "cleanup", "status")]
    [string]$Command,

    [Parameter(Position = 1)]
    [string]$Name,

    [Parameter(Position = 2)]
    [string]$Description,

    [Parameter()]
    [switch]$Force
)

# Configuración
$Config = @{
    DevelopBranch = "develop"
    MainBranch = "main"
    Remote = "origin"
    FeaturePrefix = "feature"
    FixPrefix = "fix"
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

function Test-WorkingDirectoryClean {
    $status = git status --porcelain 2>$null
    return [string]::IsNullOrWhiteSpace($status)
}

function Get-CurrentBranch {
    return git branch --show-current 2>$null
}

function Test-BranchExists {
    param([string]$Branch)
    $result = git branch --list $Branch 2>$null
    return -not [string]::IsNullOrWhiteSpace($result)
}

function Get-CommitCount {
    $base = git merge-base HEAD "$($Config.Remote)/$($Config.DevelopBranch)" 2>$null
    if (-not $base) { return 0 }
    $count = git rev-list --count "$base..HEAD" 2>$null
    return [int]$count
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
    
    $currentBranch = Get-CurrentBranch
    Write-Info "Rama actual: $currentBranch"
    
    # Contar commits en esta rama vs develop
    $commitCount = Get-CommitCount
    if ($commitCount -gt 0) {
        if ($commitCount -eq 1) {
            Write-Success "✓ Rama lista: 1 commit (squash hecho)"
        }
        else {
            Write-Warning "⚠ Rama tiene $commitCount commits. Usa: ./dev-tools.ps1 squash"
        }
    }
    
    # Estado del working directory
    $clean = Test-WorkingDirectoryClean
    if ($clean) {
        Write-Info "Working directory limpio"
    }
    else {
        Write-Warning "Hay cambios sin commitear:"
        git status --short
    }
    
    # Verificar si está up-to-date con develop
    $behind = git rev-list --count "HEAD..$($Config.Remote)/$($Config.DevelopBranch)" 2>$null
    if ($behind -and [int]$behind -gt 0) {
        Write-Warning "$behind commits nuevos en develop. Usa: ./dev-tools.ps1 ready"
    }
}

function Sync-Develop {
    Write-Header "Sincronizando $($Config.DevelopBranch)"
    
    $currentBranch = Get-CurrentBranch
    
    # Stash si hay cambios
    $hadChanges = $false
    if (-not (Test-WorkingDirectoryClean)) {
        Write-Warning "Hay cambios sin commitear. Haciendo stash..."
        $result = Invoke-GitCommand -Command "stash" -Arguments "push -m \"Auto-stash by dev-tools\""
        if (-not $result) { exit 1 }
        $hadChanges = $true
    }
    
    # Checkout develop
    $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.DevelopBranch
    if (-not $result) { exit 1 }
    
    # Pull
    Write-Info "Pull desde $($Config.Remote)/$($Config.DevelopBranch)..."
    $result = Invoke-GitCommand -Command "pull" -Arguments "$($Config.Remote) $($Config.DevelopBranch)"
    if (-not $result) { exit 1 }
    
    # Restore stash
    if ($hadChanges) {
        Write-Info "Restaurando cambios..."
        $result = Invoke-GitCommand -Command "stash" -Arguments "pop"
        if (-not $result) { exit 1 }
    }
    
    Write-Success "$($Config.DevelopBranch) actualizada"
}

function New-FeatureBranch {
    param(
        [string]$BranchName,
        [string]$Prefix
    )
    
    Write-Header "Creando rama $Prefix/$BranchName"
    
    # Validar nombre
    if ($BranchName -match '[\s\\/:*?"<>|]') {
        Write-Error "Nombre de rama inválido. No use espacios ni caracteres especiales."
        exit 1
    }
    
    # Verificar working directory
    if (-not (Test-WorkingDirectoryClean)) {
        Write-Warning "Hay cambios sin commitear. Se hará stash automático."
        $result = Invoke-GitCommand -Command "stash" -Arguments "push"
        if (-not $result) { exit 1 }
    }
    
    # Sync develop primero
    Sync-Develop
    
    # Crear rama
    $fullBranchName = "$Prefix/$BranchName"
    Write-Info "Creando rama $fullBranchName..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments "-b $fullBranchName"
    if (-not $result) { exit 1 }
    
    # Push con tracking
    Write-Info "Push a $($Config.Remote)..."
    $result = Invoke-GitCommand -Command "push" -Arguments "-u $($Config.Remote) $fullBranchName"
    if (-not $result) { exit 1 }
    
    Write-Success "Rama $fullBranchName creada y pushada"
    Write-Info "Trabaja en tus cambios y haz commits frecuentes."
    Write-Info "Cuando termines: ./dev-tools.ps1 squash"
}

function New-Commit {
    param([string]$Message)
    
    if (-not $Message) {
        Write-Error "Mensaje requerido. Uso: ./dev-tools.ps1 commit \"tipo(scope): descripción\""
        Write-Info "Formato: <tipo>(<scope>): <descripción>"
        Write-Info "Tipos: feat, fix, docs, test, refactor, chore, breaking"
        Write-Info "Ejemplo: feat(css): add scrollbar tokens to FeatureDefinitions"
        exit 1
    }
    
    # Verificar formato convencional
    if ($Message -notmatch '^(feat|fix|docs|test|refactor|chore|breaking)(\([^)]+\))?:\s.+') {
        Write-Warning "El mensaje no sigue conventional commits"
        Write-Info "Formato esperado: tipo(scope): descripción"
    }
    
    # Añadir issue si se proporciona
    if ($Description -match '#\d+') {
        $Message = "$Message`n`nFixes $Description"
    }
    
    $result = Invoke-GitCommand -Command "commit" -Arguments "-am \"$Message\""
    if (-not $result) { exit 1 }
    
    Write-Success "Commit creado"
    
    # Mostrar conteo
    $count = Get-CommitCount
    Write-Info "Commits en esta rama: $count"
    if ($count -gt 1) {
        Write-Info "Recuerda: antes del PR usa ./dev-tools.ps1 squash"
    }
}

function Invoke-Squash {
    Write-Header "Colapsando commits en 1"
    
    $commitCount = Get-CommitCount
    
    if ($commitCount -le 1) {
        Write-Success "Ya solo hay 1 commit. No se necesita squash."
        return
    }
    
    Write-Info "Commits a combinar: $commitCount"
    Write-Info "Últimos commits:"
    git log --oneline -$commitCount
    
    # Pedir mensaje del commit final
    Write-Host "`nEscribe el mensaje para el commit final (tipo(scope): descripción):" -ForegroundColor $Colors.Emphasis
    $defaultMsg = git log -1 --pretty=%B
    $finalMessage = Read-Host "Mensaje [$defaultMsg]"
    
    if (-not $finalMessage) {
        $finalMessage = $defaultMsg
    }
    
    # Hacer squash
    Write-Info "Haciendo squash..."
    $base = git merge-base HEAD "$($Config.Remote)/$($Config.DevelopBranch)"
    
    $result = Invoke-GitCommand -Command "reset" -Arguments "--soft $base"
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "commit" -Arguments "-m \"$finalMessage\""
    if (-not $result) { exit 1 }
    
    Write-Success "Squash completado. Ahora hay 1 commit."
    Write-Info "Usa ./dev-tools.ps1 ready para preparar el PR"
}

function Ready-ForPR {
    Write-Header "Preparando para PR (Squash + Rebase)"
    
    $currentBranch = Get-CurrentBranch
    
    if ($currentBranch -eq $Config.DevelopBranch) {
        Write-Error "Estás en $($Config.DevelopBranch). Crea una feature branch primero."
        exit 1
    }
    
    # Verificar cambios
    if (-not (Test-WorkingDirectoryClean)) {
        Write-Error "Hay cambios sin commitear. Commit o stash primero."
        exit 1
    }
    
    # Paso 1: Verificar squash
    $commitCount = Get-CommitCount
    if ($commitCount -gt 1) {
        Write-Warning "Hay $commitCount commits. Haciendo squash automático..."
        Invoke-Squash
    }
    
    # Paso 2: Fetch
    Write-Info "Fetch de $($Config.Remote)..."
    $result = Invoke-GitCommand -Command "fetch" -Arguments $Config.Remote
    if (-not $result) { exit 1 }
    
    # Paso 3: Rebase
    Write-Info "Rebase sobre $($Config.Remote)/$($Config.DevelopBranch)..."
    $result = Invoke-GitCommand -Command "rebase" -Arguments "$($Config.Remote)/$($Config.DevelopBranch)"
    
    if (-not $result) {
        Write-Error "`n❌ REBASE FALLÓ - HAY CONFLICTOS"
        Write-Info "Resuelve los conflictos manualmente:"
        Write-Host "  1. Edita los archivos con conflictos (busca <<<<<<<)"
        Write-Host "  2. git add <archivos>"
        Write-Host "  3. git rebase --continue"
        Write-Host "  4. ./dev-tools.ps1 fix-conflict"
        Write-Host "`nPara abortar: git rebase --abort"
        exit 1
    }
    
    # Paso 4: Verificar que sigue siendo 1 commit
    $commitCount = Get-CommitCount
    if ($commitCount -gt 1) {
        Write-Warning "El rebase creó $commitCount commits. Haciendo squash..."
        Invoke-Squash
    }
    
    # Paso 5: Push force-with-lease
    Write-Info "Push con force-with-lease..."
    $result = Invoke-GitCommand -Command "push" -Arguments "--force-with-lease"
    if (-not $result) { exit 1 }
    
    Write-Success "¡Rama lista para PR!"
    Write-Info "Verificación:"
    Write-Host "  ✓ 1 commit (squash)"
    Write-Host "  ✓ Rebase sobre develop actualizado"
    Write-Host "  ✓ Sin conflictos"
    Write-Host "`nCrea el PR en GitHub o usa: ./dev-tools.ps1 pr \"Título\" \"Descripción\""
}

function Fix-Conflict {
    Write-Header "Finalizando resolución de conflictos"
    
    # Verificar que no hay conflictos pendientes
    $status = git status --porcelain 2>$null
    if ($status -match "^(UU|AA|DD|AU|UA|DU|UD)") {
        Write-Error "Aún hay conflictos sin resolver. Edita los archivos y haz git add."
        git status --short
        exit 1
    }
    
    # Verificar si estamos en medio de un rebase
    $rebaseDir = git rev-parse --git-path rebase-merge 2>$null
    $rebaseApplyDir = git rev-parse --git-path rebase-apply 2>$null
    
    if ((Test-Path $rebaseDir) -or (Test-Path $rebaseApplyDir)) {
        Write-Info "Continuando rebase..."
        $result = Invoke-GitCommand -Command "rebase" -Arguments "--continue"
        if (-not $result) {
            Write-Error "Rebase sigue fallando. Revisa los conflictos."
            exit 1
        }
    }
    
    # Verificar cuántos commits tenemos ahora
    $commitCount = Get-CommitCount
    Write-Info "Commits después de resolver conflictos: $commitCount"
    
    if ($commitCount -gt 1) {
        Write-Warning "Hay $commitCount commits (incluyendo fixups de conflictos)"
        Write-Info "Haciendo squash final..."
        Invoke-Squash
    }
    
    # Push
    Write-Info "Push con force-with-lease..."
    $result = Invoke-GitCommand -Command "push" -Arguments "--force-with-lease"
    if (-not $result) { exit 1 }
    
    Write-Success "Conflictos resueltos y código actualizado"
    Write-Info "El PR debería poder mergearse ahora"
}

function Push-Changes {
    Write-Header "Push de cambios"
    
    $currentBranch = Get-CurrentBranch
    
    # Verificar cambios
    if (-not (Test-WorkingDirectoryClean)) {
        Write-Error "Hay cambios sin commitear. Commit primero."
        exit 1
    }
    
    # Verificar squash si es feature branch
    if ($currentBranch -ne $Config.DevelopBranch) {
        $commitCount = Get-CommitCount
        if ($commitCount -gt 1) {
            Write-Warning "⚠️  Tienes $commitCount commits. Recuerda hacer squash antes del PR."
        }
    }
    
    # Push
    Write-Info "Push..."
    $result = Invoke-GitCommand -Command "push" -Arguments "--force-with-lease"
    if (-not $result) { exit 1 }
    
    Write-Success "Push completado"
}

function New-PullRequest {
    param(
        [string]$Title,
        [string]$Body
    )
    
    Write-Header "Creando Pull Request"
    
    $currentBranch = Get-CurrentBranch
    
    if ($currentBranch -eq $Config.DevelopBranch) {
        Write-Error "No se puede crear PR desde $($Config.DevelopBranch)"
        exit 1
    }
    
    # Verificaciones
    $commitCount = Get-CommitCount
    if ($commitCount -gt 1) {
        Write-Error "Hay $commitCount commits. Debes hacer squash primero: ./dev-tools.ps1 squash"
        exit 1
    }
    
    $behind = git rev-list --count "HEAD..$($Config.Remote)/$($Config.DevelopBranch)" 2>$null
    if ($behind -gt 0) {
        Write-Error "Estás $behind commits detrás de develop. Actualiza primero: ./dev-tools.ps1 ready"
        exit 1
    }
    
    # Abrir URL de creación de PR
    $repoUrl = git remote get-url $Config.Remote 2>$null
    if ($repoUrl -match "github.com[:/](.+)/(.+?)(\.git)?$") {
        $owner = $matches[1]
        $repo = $matches[2]
        
        $prUrl = "https://github.com/$owner/$repo/compare/$($Config.DevelopBranch)...$currentBranch?expand=1"
        
        if ($Title) {
            $prUrl += "&title=$([Uri]::EscapeDataString($Title))"
        }
        if ($Body) {
            $prUrl += "&body=$([Uri]::EscapeDataString($Body))"
        }
        
        Write-Info "Abriendo navegador para crear PR..."
        Start-Process $prUrl
    }
    else {
        Write-Warning "No se pudo detectar URL del repo. Crea el PR manualmente."
    }
    
    Write-Success "PR listo para crear"
}

function Invoke-Cleanup {
    Write-Header "Limpiando Ramas Locales"
    
    $currentBranch = Get-CurrentBranch
    
    # Volver a develop
    if ($currentBranch -ne $Config.DevelopBranch) {
        $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.DevelopBranch
        if (-not $result) { exit 1 }
    }
    
    # Pull
    $result = Invoke-GitCommand -Command "pull" -Arguments "$($Config.Remote) $($Config.DevelopBranch)"
    if (-not $result) { exit 1 }
    
    # Eliminar ramas mergeadas
    $merged = git branch --merged $Config.DevelopBranch --format="%(refname:short)" | Where-Object { 
        $_ -notin @($Config.MainBranch, $Config.DevelopBranch) -and 
        $_ -notmatch "^\*" 
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

# Verificar que estamos en un repo git
if (-not (Test-GitRepository)) {
    Write-Error "No estás en un repositorio Git"
    exit 1
}

# Ejecutar comando
switch ($Command) {
    "status" { Show-Status }
    "sync" { Sync-Develop }
    "feature" {
        if (-not $Name) {
            Write-Error "Se requiere nombre. Uso: ./dev-tools.ps1 feature nombre-de-la-feature"
            exit 1
        }
        New-FeatureBranch -BranchName $Name -Prefix $Config.FeaturePrefix
    }
    "fix" {
        if (-not $Name) {
            Write-Error "Se requiere nombre. Uso: ./dev-tools.ps1 fix nombre-del-fix"
            exit 1
        }
        New-FeatureBranch -BranchName $Name -Prefix $Config.FixPrefix
    }
    "commit" { New-Commit -Message $Name }
    "squash" { Invoke-Squash }
    "ready" { Ready-ForPR }
    "fix-conflict" { Fix-Conflict }
    "push" { Push-Changes }
    "pr" { New-PullRequest -Title $Name -Body $Description }
    "cleanup" { Invoke-Cleanup }
}

Write-Host "`nDone!" -ForegroundColor $Colors.Success

#endregion
