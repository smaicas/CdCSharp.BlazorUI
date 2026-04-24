#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Configura un repositorio GitHub con labels y branch protection

.DESCRIPTION
    Script reutilizable para configurar:
    - Labels de severity (blocker, critical, major, minor, polish)
    - Branch protection para main y develop
    - Squash merge only, historial lineal

.PARAMETER Owner
    Owner del repositorio (ej: smaicas)

.PARAMETER Repo
    Nombre del repositorio (ej: CdCSharp.BlazorUI)

.PARAMETER Token
    GitHub Personal Access Token con permisos: repo, workflow

.PARAMETER SetupLabels
    Crear labels de severity

.PARAMETER SetupBranchProtection
    Configurar branch protection

.EXAMPLE
    ./github-setup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token ghp_xxx -SetupLabels -SetupBranchProtection

.EXAMPLE
    ./github-setup.ps1 -Owner smaicas -Repo CdCSharp.BlazorUI -Token $env:GITHUB_TOKEN -SetupLabels

.NOTES
    El token necesita permisos de admin para branch protection.
    Si no tienes permisos de admin, usa -SetupLabels solo.
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
    [switch]$SetupBranchProtection
)

# Configuración
$Config = @{
    ApiBase = "https://api.github.com"
    Headers = @{
        Authorization = "token $Token"
        Accept = "application/vnd.github+json"
        "X-GitHub-Api-Version" = "2022-11-28"
    }
    DefaultBranch = $null  # Se detecta automáticamente
}

# Colores para output
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
            return $null  # Recurso no existe
        }
        
        Write-Error "API Error ${statusCode}: $($errorMessage.message)"
        return $false
    }
}

function Test-RepositoryAccess {
    Write-Step "Verificando acceso al repositorio $Owner/$Repo"
    
    $repo = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo"
    
    if (-not $repo) {
        Write-Error "No se puede acceder al repositorio. Verifica:"
        Write-Host "  - El owner y nombre del repo son correctos"
        Write-Host "  - El token tiene permisos 'repo'"
        exit 1
    }
    
    # Guardar rama principal
    $Config.DefaultBranch = $repo.default_branch
    
    Write-Success "Acceso verificado: $($repo.full_name)"
    Write-Host "  Privado: $($repo.private)"
    Write-Host "  Default branch: $($Config.DefaultBranch)"
}

function Initialize-DevelopBranch {
    Write-Step "Verificando rama develop"
    
    # Verificar si develop existe
    $develop = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/branches/develop"
    
    if ($develop) {
        Write-Success "Rama develop ya existe"
        return $true
    }
    
    Write-Warning "La rama develop no existe. Creándola desde $($Config.DefaultBranch)..."
    
    # Obtener el SHA del último commit de la rama principal
    $defaultBranchRef = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/git/refs/heads/$($Config.DefaultBranch)"
    
    if (-not $defaultBranchRef) {
        Write-Error "No se pudo obtener la referencia de $($Config.DefaultBranch)"
        return $false
    }
    
    $sha = $defaultBranchRef.object.sha
    
    # Crear rama develop
    $body = @{
        ref = "refs/heads/develop"
        sha = $sha
    }
    
    $result = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/git/refs" -Body $body
    
    if ($result -eq $false) {
        Write-Error "No se pudo crear la rama develop"
        return $false
    }
    
    Write-Success "Rama develop creada desde $($Config.DefaultBranch)"
    return $true
}

function Initialize-Labels {
    Write-Step "Configurando Labels"
    
    $labels = @(
        @{ Name = "severity:blocker"; Color = "d9534f"; Description = "Bloquea el release - debe resolverse antes de publicar" }
        @{ Name = "severity:critical"; Color = "e74c3c"; Description = "Problema crítico - alta prioridad" }
        @{ Name = "severity:major"; Color = "f39c12"; Description = "Problema importante - afecta funcionalidad" }
        @{ Name = "severity:minor"; Color = "f1c40f"; Description = "Problema menor - mejora deseable" }
        @{ Name = "severity:polish"; Color = "2ecc71"; Description = "Refinamiento - calidad de vida" }
    )
    
    foreach ($label in $labels) {
        # Verificar si existe
        $existing = Invoke-GitHubApi -Endpoint "/repos/$Owner/$Repo/labels/$($label.Name -replace ':', '%3A')"
        
        $body = @{
            name = $label.Name
            color = $label.Color
            description = $label.Description
        }
        
        if ($existing) {
            Write-Host "Actualizando label: $($label.Name)"
            $result = Invoke-GitHubApi -Method "PATCH" -Endpoint "/repos/$Owner/$Repo/labels/$($label.Name -replace ':', '%3A')" -Body $body
        }
        else {
            Write-Host "Creando label: $($label.Name)"
            $result = Invoke-GitHubApi -Method "POST" -Endpoint "/repos/$Owner/$Repo/labels" -Body $body
        }
        
        if ($result -eq $false) {
            Write-Warning "No se pudo configurar label: $($label.Name)"
        }
    }
    
    Write-Success "Labels configurados"
}

function Initialize-BranchProtection {
    param(
        [string]$Branch,
        [string[]]$RequiredContexts
    )
    
    Write-Step "Configurando branch protection para: $Branch"
    
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
        Write-Error "No se pudo configurar branch protection para $Branch"
        Write-Host "Posibles causas:"
        Write-Host "  - No tienes permisos de administrador"
        Write-Host "  - El token no tiene scope 'repo'"
        Write-Host "  - La rama $Branch no existe"
        return $false
    }
    
    Write-Success "Branch protection configurado para $Branch"
    return $true
}

function Show-Summary {
    Write-Step "Resumen de configuración"
    
    Write-Host "Repositorio: $Owner/$Repo" -ForegroundColor $Colors.Info
    Write-Host ""
    
    if ($SetupLabels) {
        Write-Host "Labels configurados:" -ForegroundColor $Colors.Success
        Write-Host "  - severity:blocker (🔴 Bloquea release)"
        Write-Host "  - severity:critical (🔴 Crítico)"
        Write-Host "  - severity:major (🟠 Importante)"
        Write-Host "  - severity:minor (🟡 Menor)"
        Write-Host "  - severity:polish (🟢 Refinamiento)"
        Write-Host ""
    }
    
    if ($SetupBranchProtection) {
        Write-Host "Branch protection:" -ForegroundColor $Colors.Success
        Write-Host "  $($Config.DefaultBranch):" -ForegroundColor $Colors.Info
        Write-Host "    - Requiere PR + 1 approval"
        Write-Host "    - Requiere: Release Gate / Build Check"
        Write-Host "    - Requiere: Release Gate / Check Blocking Issues"
        Write-Host "    - Requiere: Release Gate / Check Public API"
        Write-Host "    - Squash merge only + historial lineal"
        Write-Host ""
        Write-Host "  develop:" -ForegroundColor $Colors.Info
        Write-Host "    - Requiere PR + 1 approval"
        Write-Host "    - Requiere: Preview Gate / Build & Test"
        Write-Host "    - Requiere: Preview Gate / Code Coverage"
        Write-Host "    - Requiere: Preview Gate / Check Public API"
        Write-Host "    - Squash merge only + historial lineal"
    }
    
    Write-Host ""
    Write-Host "Próximos pasos:" -ForegroundColor $Colors.Warning
    Write-Host "  1. Configurar NUGET_API_KEY en Settings > Secrets"
    Write-Host "  2. Verificar workflows en Actions"
    Write-Host "  3. Probar con una PR de prueba"
}

# ============ MAIN ============

# Validar parámetros
if (-not $SetupLabels -and -not $SetupBranchProtection) {
    Write-Error "Debes especificar al menos una opción: -SetupLabels o -SetupBranchProtection"
    Write-Host "Uso: ./github-setup.ps1 -Owner <owner> -Repo <repo> -Token <token> -SetupLabels -SetupBranchProtection"
    exit 1
}

# Verificar acceso
Test-RepositoryAccess

# Configurar labels
if ($SetupLabels) {
    Initialize-Labels
}

# Configurar branch protection
if ($SetupBranchProtection) {
    $mainContexts = @(
        "Release Gate / Build Check"
        "Release Gate / Check Blocking Issues"
        "Release Gate / Check Public API"
    )
    
    $developContexts = @(
        "Preview Gate / Build & Test"
        "Preview Gate / Code Coverage"
        "Preview Gate / Check Public API"
    )
    
    # Crear develop si no existe
    $developCreated = Initialize-DevelopBranch
    
    # Configurar branch protection para la rama principal (master/main)
    $mainOk = Initialize-BranchProtection -Branch $Config.DefaultBranch -RequiredContexts $mainContexts
    $developOk = Initialize-BranchProtection -Branch "develop" -RequiredContexts $developContexts
    
    if (-not $mainOk -or -not $developOk) {
        Write-Warning "Algunas configuraciones de branch protection fallaron."
        Write-Host "Configura manualmente en: https://github.com/$Owner/$Repo/settings/branches"
    }
}

# Mostrar resumen
Show-Summary

Write-Host "`nDone!" -ForegroundColor $Colors.Success
