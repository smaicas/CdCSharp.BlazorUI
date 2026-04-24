#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Limpia el cache de Razor y VS para resolver problemas de intellisense

.DESCRIPTION
    Elimina archivos generados por el compilador Razor, cache de VS,
    y fuerza una reconstrucción limpia del proyecto.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$Project = "docs/CdCSharp.BlazorUI.Docs.Wasm"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Limpiando cache de Razor ===" -ForegroundColor Cyan

# 1. Limpiar carpetas obj y bin
Write-Host "1. Eliminando obj/ y bin/..." -ForegroundColor Yellow
$folders = @("obj", "bin")
foreach ($folder in $folders) {
    $path = Join-Path $Project $folder
    if (Test-Path $path) {
        Remove-Item -Path $path -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "   Eliminado: $path"
    }
}

# 2. Limpiar cache de VS
Write-Host "2. Eliminando cache de Visual Studio..." -ForegroundColor Yellow
$vsCache = ".vs"
if (Test-Path $vsCache) {
    Remove-Item -Path $vsCache -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "   Eliminado: $vsCache"
}

# 3. Limpiar archivos .g.cs generados por Razor
Write-Host "3. Buscando archivos .g.cs generados..." -ForegroundColor Yellow
$generatedFiles = Get-ChildItem -Path $Project -Recurse -Filter "*.g.cs" -ErrorAction SilentlyContinue
foreach ($file in $generatedFiles) {
    Remove-Item -Path $file.FullName -Force -ErrorAction SilentlyContinue
    Write-Host "   Eliminado: $($file.FullName)"
}

# 4. Verificar encoding de archivos .razor
Write-Host "4. Verificando encoding de archivos .razor..." -ForegroundColor Yellow
$razorFiles = Get-ChildItem -Path $Project -Recurse -Filter "*.razor"
$fixedCount = 0
foreach ($file in $razorFiles) {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName) | Select-Object -First 3
    # Verificar si tiene BOM (EF BB BF)
    if ($bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
        $content = $content -replace "^\uFEFF", ""
        [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.UTF8Encoding]::new($false))
        Write-Host "   BOM removido: $($file.Name)" -ForegroundColor Magenta
        $fixedCount++
    }
}
if ($fixedCount -eq 0) {
    Write-Host "   Todos los archivos OK (sin BOM)"
}

Write-Host "`n=== Limpieza completada ===" -ForegroundColor Green
Write-Host "Ahora ejecuta: dotnet build $Project" -ForegroundColor Yellow
