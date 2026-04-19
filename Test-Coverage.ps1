# --- Configuración ---
# Si no pasas argumentos, usará estos dos por defecto
param (
    [string[]]$ProyectosParaCubrir = @("CdCSharp.BlazorUI", "CdCSharp.BlazorUI.Core"),
    [string]$CarpetaDestino = "ReporteFinalCobertura"
)

Write-Host "--- Iniciando proceso de cobertura ---" -ForegroundColor Cyan

# 1. Limpieza de carpetas previas
Write-Host "1. Limpiando datos anteriores..." -ForegroundColor Yellow
if (Test-Path "**/TestResults") {
    Get-ChildItem -Path . -Include TestResults -Recurse | Remove-Item -Recurse -Force
}
if (Test-Path $CarpetaDestino) {
    Remove-Item -Path $CarpetaDestino -Recurse -Force
}

# 2. Construir el filtro de inclusión para dotnet test
# Formato resultante: [Proyecto1]*,[Proyecto2]*
$filtroInclusion = ($ProyectosParaCubrir | ForEach-Object { "[$_]*" }) -join ","
Write-Host "Filtro de inclusión: $filtroInclusion" -ForegroundColor Gray

# 3. Ejecutar los tests
Write-Host "2. Ejecutando tests y recolectando cobertura (esto puede tardar)..." -ForegroundColor Yellow
dotnet test --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include="$filtroInclusion"

# 4. Generar el informe unificado con ReportGenerator
Write-Host "3. Generando informe unificado..." -ForegroundColor Yellow
# Tipos de reporte: Html para humanos, Cobertura (XML) para máquinas/herramientas
$tiposReporte = "Html;Cobertura"

reportgenerator `
  -reports:"**/TestResults/**/coverage.cobertura.xml" `
  -targetdir:"$CarpetaDestino" `
  -reporttypes:"$tiposReporte" `
  -title:"Cobertura CdCSharp"

Write-Host "--- Proceso completado ---" -ForegroundColor Green
Write-Host "Informe disponible en: $CarpetaDestino/index.html"
Write-Host "XML unificado en: $CarpetaDestino/Cobertura.xml"