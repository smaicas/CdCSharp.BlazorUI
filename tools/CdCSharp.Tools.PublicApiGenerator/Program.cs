using GeneratePublicApi;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;

// ── MSBuild debe registrarse ANTES ───────────────────────────────────────────
MSBuildLocator.RegisterDefaults();

CliOptions? options = CliOptions.Parse(args);
if (options is null) return 1;

Console.WriteLine($"[generate-public-api] Solución: {options.SolutionPath}");
Console.WriteLine($"[generate-public-api] Modo overwrite: {(options.Overwrite ? "sí" : "no")}");

// 🔥 Detectar ruta de la propia herramienta (para excluirla)
string currentBaseDir = AppContext.BaseDirectory;

bool IsSelfProject(Project project)
{
    if (project.FilePath is null) return false;

    string projectDir = Path.GetDirectoryName(project.FilePath)!;
    return currentBaseDir.StartsWith(projectDir, StringComparison.OrdinalIgnoreCase);
}

// ─────────────────────────────────────────────────────────────────────────────
// 🔥 1. BUILD ÚNICO de la solución
// ─────────────────────────────────────────────────────────────────────────────
//
// Razones por las que NO usamos `BaseIntermediateOutputPath` para aislar a
// `tempObj`:
//   • La propiedad se propaga a todos los proyectos referenciados como
//     global property → dependencias con TFM distinto colisionan en el
//     mismo `project.assets.json` (NETSDK1005).
//   • Aislar por proyecto creando `tempObj/<projectName>/obj/` no resuelve
//     la propagación: la dependencia hereda esa misma carpeta.
//
// Compilamos la solución una sola vez en su `obj/` por proyecto (default).
// Activamos `EmitCompilerGeneratedFiles=true` para forzar a los generators
// a depositar sus salidas en disco (Razor + ComponentInfoGenerator + …),
// que `OwnFileCollector` recoge desde `obj/*/generated/`.

Console.WriteLine("\n[build] dotnet build -c Release …");

ProcessStartInfo psi = new()
{
    FileName = "dotnet",
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false
};
psi.ArgumentList.Add("build");
psi.ArgumentList.Add(options.SolutionPath);
psi.ArgumentList.Add("-c");
psi.ArgumentList.Add("Release");
psi.ArgumentList.Add("/p:EmitCompilerGeneratedFiles=true");
psi.ArgumentList.Add("/p:CompilerGeneratedFilesOutputPath=obj/generated");

using (Process buildProcess = Process.Start(psi)!)
{
    buildProcess.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
    buildProcess.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };

    buildProcess.BeginOutputReadLine();
    buildProcess.BeginErrorReadLine();

    await buildProcess.WaitForExitAsync();

    if (buildProcess.ExitCode != 0)
    {
        Console.Error.WriteLine("[ERROR] Build de la solución falló");
        return 1;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// 🔥 2. MSBuildWorkspace (compilación)
// ─────────────────────────────────────────────────────────────────────────────

Dictionary<string, string> buildProps = new()
{
    ["DesignTimeBuild"] = "false",
    ["BuildingProject"] = "true",
    ["SkipCompilerExecution"] = "false",
    ["Configuration"] = "Release",
};

using MSBuildWorkspace workspace = MSBuildWorkspace.Create(buildProps);

workspace.WorkspaceFailed += (_, e) =>
{
    string level = e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure ? "ERROR" : "WARN";
    Console.Error.WriteLine($"  [{level}] {e.Diagnostic.Message}");
};

Console.WriteLine("\nCargando solución…");
Solution solution = await workspace.OpenSolutionAsync(
    options.SolutionPath,
    new ConsoleProgressReporter(),
    CancellationToken.None);

// ─────────────────────────────────────────────────────────────────────────────
// 🔥 3. Procesar proyectos
// ─────────────────────────────────────────────────────────────────────────────

int processed = 0;
int skipped = 0;

foreach (Project project in solution.Projects.OrderBy(p => p.Name))
{
    if (string.IsNullOrEmpty(project.FilePath)) continue;

    if (IsSelfProject(project))
    {
        Console.WriteLine($"\n[SKIP] {project.Name} — self");
        skipped++;
        continue;
    }

    if (!ProjectDetector.UsesPublicApiAnalyzers(project.FilePath))
    {
        Console.WriteLine($"\n[SKIP] {project.Name}");
        skipped++;
        continue;
    }

    string outputFile = Path.Combine(
        Path.GetDirectoryName(project.FilePath)!,
        "PublicAPI.Unshipped.txt");

    if (File.Exists(outputFile) && !options.Overwrite)
    {
        Console.WriteLine($"\n[SKIP] {project.Name} (ya existe)");
        skipped++;
        continue;
    }

    Console.WriteLine($"\n[PROC] {project.Name}");

    Compilation? baseCompilation = await project.GetCompilationAsync();
    if (baseCompilation is null)
    {
        Console.Error.WriteLine("  [ERROR] No compilation");
        continue;
    }

    IEnumerable<ISourceGenerator> generators = project.AnalyzerReferences
        .SelectMany(r => r.GetGenerators(project.Language));

    GeneratorDriver driver = CSharpGeneratorDriver.Create(
        generators,
        project.AdditionalDocuments
            .Select(d => new AdditionalTextFromDocument(d))
            .ToImmutableArray(),
        (CSharpParseOptions)project.ParseOptions!);

    driver = driver.RunGeneratorsAndUpdateCompilation(
        baseCompilation,
        out Compilation compilation,
        out _);

    Console.WriteLine($"  Trees base: {baseCompilation.SyntaxTrees.Count()}");

    // Sin baseIntermediateOutputPath: el collector lee del obj/ del propio proyecto.
    OwnFiles ownFiles = await OwnFileCollector.CollectAsync(project);

    Console.WriteLine($"  Ficheros: {ownFiles.Count} | Razor: {ownFiles.DiskRazor}");

    if (ownFiles.DiskGenerated > 0)
    {
        CSharpParseOptions parseOptions = (CSharpParseOptions)project.ParseOptions!;

        IEnumerable<SyntaxTree> trees = ownFiles.DiskGeneratedPaths.Select(p =>
            CSharpSyntaxTree.ParseText(File.ReadAllText(p), parseOptions, p));

        compilation = compilation.AddSyntaxTrees(trees);
    }

    Console.WriteLine($"  Trees total: {compilation.SyntaxTrees.Count()}");

    List<string> apiLines = PublicApiExtractor.Extract(
        compilation, ownFiles.Paths, options.NullableEnable);

    await File.WriteAllTextAsync(outputFile, string.Join("\n", apiLines) + "\n");

    Console.WriteLine($"  ✓ {outputFile}");
    processed++;
}

Console.WriteLine($"\nProcesados: {processed} | Omitidos: {skipped}");
return 0;

// ── Auxiliares ───────────────────────────────────────────────────────────────

class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
{
    public void Report(ProjectLoadProgress value) =>
        Console.WriteLine($"  {value.Operation} {Path.GetFileName(value.FilePath)}");
}

class AdditionalTextFromDocument : AdditionalText
{
    private readonly TextDocument _doc;

    public AdditionalTextFromDocument(TextDocument doc) => _doc = doc;

    public override string Path => _doc.FilePath!;

    public override SourceText? GetText(CancellationToken cancellationToken = default) =>
        _doc.GetTextAsync(cancellationToken).Result;
}