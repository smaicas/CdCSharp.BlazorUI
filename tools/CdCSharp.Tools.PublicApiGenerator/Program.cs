using GeneratePublicApi;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

// ── MSBuild debe registrarse ANTES de cualquier uso de tipos de Roslyn ──────
MSBuildLocator.RegisterDefaults();

CliOptions? options = CliOptions.Parse(args);
if (options is null) return 1;

Console.WriteLine($"[generate-public-api] Solución: {options.SolutionPath}");
Console.WriteLine($"[generate-public-api] Modo overwrite: {(options.Overwrite ? "sí" : "no")}");

// ── Workspace ────────────────────────────────────────────────────────────────
// DesignTimeBuild=false → fuerza compilación real para obtener símbolos
// BuildingProject=false  → no ejecuta tareas de build pesadas
Dictionary<string, string> buildProps = new()
{
    ["DesignTimeBuild"] = "false",
    ["BuildingProject"] = "false",
    ["SkipCompilerExecution"] = "false",
    // Configuration=Release evita capturar miembros bajo `#if DEBUG`
    // que no formarían parte de la API shipped (p.ej. TrackPerformanceEnabled).
    ["Configuration"] = "Release",
};

using MSBuildWorkspace workspace = MSBuildWorkspace.Create(buildProps);

workspace.WorkspaceFailed += (_, e) =>
{
    string level = e.Diagnostic.Kind == Microsoft.CodeAnalysis.WorkspaceDiagnosticKind.Failure
        ? "ERROR" : "WARN";
    Console.Error.WriteLine($"  [{level}] {e.Diagnostic.Message}");
};

Console.WriteLine("\nCargando solución…");
var solution = await workspace.OpenSolutionAsync(
    options.SolutionPath,
    new ConsoleProgressReporter(),
    CancellationToken.None);

// ── Procesar proyectos ───────────────────────────────────────────────────────
int processed = 0;
int skipped = 0;

foreach (var project in solution.Projects.OrderBy(p => p.Name))
{
    if (string.IsNullOrEmpty(project.FilePath)) continue;

    // 1. ¿El proyecto usa PublicApiAnalyzers?
    if (!ProjectDetector.UsesPublicApiAnalyzers(project.FilePath))
    {
        Console.WriteLine($"\n[SKIP] {project.Name}  — no usa PublicApiAnalyzers");
        skipped++;
        continue;
    }

    string outputFile = Path.Combine(
        Path.GetDirectoryName(project.FilePath)!,
        "PublicAPI.Unshipped.txt");

    if (File.Exists(outputFile) && !options.Overwrite)
    {
        Console.WriteLine($"\n[SKIP] {project.Name}  — PublicAPI.Unshipped.txt ya existe (usa --overwrite)");
        skipped++;
        continue;
    }

    Console.WriteLine($"\n[PROC] {project.Name}");

    // 2. Compilación
    var compilation = await project.GetCompilationAsync();
    if (compilation is null)
    {
        Console.Error.WriteLine("  [ERROR] No se pudo obtener la compilación.");
        continue;
    }

    // 3. Rutas de ficheros propios del proyecto (C# + generated de Razor)
    var ownFiles = await OwnFileCollector.CollectAsync(project);
    Console.WriteLine(
        $"  Ficheros propios : {ownFiles.Count} cs  |  " +
        $"Razor via Roslyn: {ownFiles.RoslynRazor}  |  " +
        $"Razor via obj/  : {ownFiles.DiskRazor}");

    // 4. Extraer API pública
    var apiLines = PublicApiExtractor.Extract(compilation, ownFiles.Paths, options.NullableEnable);
    Console.WriteLine($"  Símbolos públicos: {apiLines.Count - 1}");  // -1 por la cabecera

    // 5. Escribir fichero
    await File.WriteAllTextAsync(outputFile, string.Join("\n", apiLines) + "\n");
    Console.WriteLine($"  ✓ Escrito: {outputFile}");
    processed++;
}

Console.WriteLine($"\n══════════════════════════════════════════");
Console.WriteLine($"  Procesados : {processed}");
Console.WriteLine($"  Omitidos   : {skipped}");
Console.WriteLine($"══════════════════════════════════════════");

return 0;

// ── Tipos auxiliares inline ──────────────────────────────────────────────────

/// <summary>Reporta el progreso de carga de la solución en consola.</summary>
class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
{
    public void Report(ProjectLoadProgress value)
    {
        string op = value.Operation switch
        {
            ProjectLoadOperation.Evaluate => "Evaluate",
            ProjectLoadOperation.Build => "Build   ",
            ProjectLoadOperation.Resolve => "Resolve ",
            _ => "        "
        };
        Console.WriteLine($"  {op} {Path.GetFileName(value.FilePath)}");
    }
}