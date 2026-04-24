#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Verifica y corrige el encoding de archivos .razor a UTF-8 sin BOM

.DESCRIPTION
    Escanea todos los archivos .razor en el proyecto docs y los guarda
    en UTF-8 sin BOM, corrigiendo Windows-1252 u otros encodings.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$Path = "."
)

$ErrorActionPreference = "Stop"

Write-Host "=== Verificando encoding de archivos .razor ===" -ForegroundColor Cyan

$files = Get-ChildItem -Path $Path -Recurse -Filter "*.razor" | 
    Where-Object { $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\bin\\" }
$fixedCount = 0
$utf8WithBom = [System.Text.UTF8Encoding]::new($true)

foreach ($file in $files) {
    # Leer el archivo como bytes para detectar el encoding
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    
    # Detectar encoding actual
    $encoding = "unknown"
    $hasBom = $false
    
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        $encoding = "UTF-8 with BOM"
        $hasBom = $true
    }
    elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
        $encoding = "UTF-16 LE"
    }
    elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) {
        $encoding = "UTF-16 BE"
    }
    elseif ($bytes.Length -ge 0) {
        # Intentar detectar UTF-8 válido sin BOM
        try {
            $contentTest = [System.Text.Encoding]::UTF8.GetString($bytes)
            $bytesReencoded = [System.Text.Encoding]::UTF8.GetBytes($contentTest)
            if ([System.Linq.Enumerable]::SequenceEqual($bytes, $bytesReencoded)) {
                $encoding = "UTF-8 without BOM"
            }
            else {
                $encoding = "Windows-1252 or other"
            }
        }
        catch {
            $encoding = "Windows-1252 or other"
        }
    }
    
    # Si no es UTF-8 con BOM, convertirlo
    if ($encoding -ne "UTF-8 with BOM") {
        Write-Host "Procesando: $($file.FullName)" -ForegroundColor Yellow
        Write-Host "  Encoding detectado: $encoding"
        
        # Leer el contenido con el encoding correcto
        $content = $null
        if ($hasBom) {
            $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
            $content = $content -replace "^\uFEFF", ""  # Remover BOM si existe
        }
        else {
            # Intentar UTF-8 primero, si falla usar Windows-1252
            try {
                $content = [System.Text.Encoding]::UTF8.GetString($bytes)
                # Verificar que sea válido
                $testBytes = [System.Text.Encoding]::UTF8.GetBytes($content)
                if (-not [System.Linq.Enumerable]::SequenceEqual($bytes, $testBytes)) {
                    throw "Not valid UTF-8"
                }
            }
            catch {
                # Usar Windows-1252 (ISO-8859-1)
                $content = [System.Text.Encoding]::GetEncoding(1252).GetString($bytes)
            }
        }
        
        # Guardar como UTF-8 con BOM
        [System.IO.File]::WriteAllText($file.FullName, $content, $utf8WithBom)
        Write-Host "  -> Convertido a UTF-8 con BOM" -ForegroundColor Green
        $fixedCount++
    }
}

Write-Host "`n=== Resumen ===" -ForegroundColor Cyan
if ($fixedCount -eq 0) {
    Write-Host "Todos los archivos ya están en UTF-8 con BOM" -ForegroundColor Green
}
else {
    Write-Host "$fixedCount archivos corregidos" -ForegroundColor Green
}
