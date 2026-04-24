#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Elimina entradas <Content Remove="..."> de archivos .csproj

.DESCRIPTION
    Visual Studio a veces añade automáticamente entradas Content Remove
    al crear componentes Razor. Este script las elimina.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$ProjectPath = "docs/CdCSharp.BlazorUI.Docs.Wasm/CdCSharp.BlazorUI.Docs.Wasm.csproj"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Limpiando Content Remove de $ProjectPath ===" -ForegroundColor Cyan

if (-not (Test-Path $ProjectPath)) {
    Write-Error "Proyecto no encontrado: $ProjectPath"
    exit 1
}

$csproj = [xml](Get-Content $ProjectPath -Raw)
$ns = @{ msb = "http://schemas.microsoft.com/developer/msbuild/2003" }

# Buscar ItemGroup que contenga Content Remove
$itemGroups = $csproj.Project.ItemGroup | Where-Object { 
    $_.Content -and $_.Content.Remove 
}

$removedCount = 0
foreach ($ig in $itemGroups) {
    $removes = $ig.Content | Where-Object { $_.Remove }
    foreach ($r in $removes) {
        Write-Host "Eliminando: Content Remove='$($r.Remove)'" -ForegroundColor Yellow
        $ig.RemoveChild($r) | Out-Null
        $removedCount++
    }
    
    # Si el ItemGroup quedó vacío, eliminarlo
    if ($ig.ChildNodes.Count -eq 0) {
        $csproj.Project.RemoveChild($ig) | Out-Null
    }
}

if ($removedCount -gt 0) {
    $csproj.Save($ProjectPath)
    Write-Host "`n$removedCount entradas eliminadas" -ForegroundColor Green
}
else {
    Write-Host "No se encontraron entradas Content Remove" -ForegroundColor Green
}
