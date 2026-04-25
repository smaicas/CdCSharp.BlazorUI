namespace GeneratePublicApi;

using Microsoft.CodeAnalysis;

/// <summary>
/// Recopila las rutas de ficheros que pertenecen exclusivamente a un proyecto:
/// <list type="bullet">
///   <item>Documentos C# normales (.cs) del proyecto.</item>
///   <item>Ficheros <c>.razor.g.cs</c> generados por el Razor Source Generator,
///         buscados en el directorio <c>obj/</c> del proyecto.</item>
/// </list>
///
/// <para>
/// <b>Por qué no usamos <c>GetSourceGeneratedDocumentsAsync()</c>:</b><br/>
/// El Razor Source Generator necesita que los ficheros <c>.razor</c> estén
/// disponibles como <c>AdditionalFiles</c> y que el pipeline de Roslyn esté
/// configurado como en un build real. En un <c>MSBuildWorkspace</c> en modo
/// design-time esto no siempre se cumple, por lo que la API devuelve 0 documentos
/// generados de Razor. En cambio, si el proyecto se ha compilado previamente
/// con <c>dotnet build</c>, los ficheros generados ya existen en disco bajo
/// <c>obj/{config}/{tfm}/generated/Microsoft.NET.Sdk.Razor.SourceGenerators/</c>
/// y podemos leerlos directamente.
/// </para>
/// </summary>
internal static class OwnFileCollector
{
    // Nombre del directorio de salida del generador de Razor dentro de obj/
    private const string RazorGeneratorFolderFragment =
        "Microsoft.NET.Sdk.Razor.SourceGenerators";

    public static async Task<OwnFiles> CollectAsync(Project project)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // ── 1. Documentos C# explícitos del proyecto ─────────────────────
        foreach (var doc in project.Documents)
            if (doc.FilePath is not null)
                paths.Add(doc.FilePath);

        // ── 2. Documentos de otros source generators via API de Roslyn ───
        //       (funciona para generators que no sean Razor, p.ej. AutoMapper,
        //        System.Text.Json, etc.)
        var generatedDocs = await project.GetSourceGeneratedDocumentsAsync();
        var roslynGeneratedCount = 0;
        var roslynRazorCount = 0;
        foreach (var doc in generatedDocs)
        {
            if (doc.FilePath is null) continue;
            paths.Add(doc.FilePath);
            roslynGeneratedCount++;
            if (IsRazorGenerated(doc.FilePath)) roslynRazorCount++;
        }

        // ── 3. Fallback para Razor: escanear obj/ en disco ───────────────
        //       Si GetSourceGeneratedDocumentsAsync no devolvió ficheros .razor.g.cs
        //       (lo habitual en MSBuildWorkspace), los buscamos directamente.
        //       Requiere que el proyecto se haya compilado al menos una vez.
        var diskRazorCount = 0;
        if (roslynRazorCount == 0 && project.FilePath is not null)
        {
            var objDir = Path.Combine(
                Path.GetDirectoryName(project.FilePath)!, "obj");

            if (Directory.Exists(objDir))
            {
                foreach (var file in Directory.EnumerateFiles(
                    objDir, "*.razor.g.cs", SearchOption.AllDirectories))
                {
                    // Solo los que están bajo la carpeta del Razor Source Generator
                    // para evitar incluir ficheros de otros generadores o artefactos
                    // de builds antiguos con nombre similar.
                    if (file.Contains(RazorGeneratorFolderFragment,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        paths.Add(file);
                        diskRazorCount++;
                    }
                }

                if (diskRazorCount == 0)
                {
                    // El generador aún no ha corrido: avisar al usuario.
                    Console.WriteLine(
                        "  [WARN] No se encontraron ficheros .razor.g.cs en obj/. " +
                        "Ejecuta 'dotnet build' primero para que el Razor Source Generator " +
                        "produzca los ficheros generados.");
                }
            }
        }

        return new OwnFiles(paths, roslynGeneratedCount, roslynRazorCount, diskRazorCount);
    }

    public static bool IsRazorGenerated(string filePath)
        => filePath.EndsWith(".razor.g.cs", StringComparison.OrdinalIgnoreCase)
        || filePath.Contains(RazorGeneratorFolderFragment, StringComparison.OrdinalIgnoreCase);
}

/// <summary>Resultado de <see cref="OwnFileCollector.CollectAsync"/>.</summary>
internal sealed class OwnFiles(
    HashSet<string> paths,
    int roslynGenerated,
    int roslynRazor,
    int diskRazor)
{
    public HashSet<string> Paths          { get; } = paths;
    public int             Count          => Paths.Count;
    public int             RoslynGenerated { get; } = roslynGenerated;
    public int             RoslynRazor    { get; } = roslynRazor;
    public int             DiskRazor      { get; } = diskRazor;
    public int             TotalRazor     => RoslynRazor + DiskRazor;

    public bool Contains(string path) => Paths.Contains(path);
}