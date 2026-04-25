using Microsoft.CodeAnalysis;

namespace GeneratePublicApi;

/// <summary>
/// Recopila las rutas de ficheros que pertenecen exclusivamente a un proyecto:
/// <list type="bullet">
///   <item>Documentos C# normales (.cs) del proyecto.</item>
///   <item>Ficheros generados devueltos por la API de Roslyn
///         (<c>GetSourceGeneratedDocumentsAsync</c>).</item>
///   <item>Ficheros generados encontrados en disco bajo <c>obj/*/generated/</c>
///         que Roslyn <b>no</b> devolvió (fallback para Razor y generadores propios
///         que no funcionan correctamente en MSBuildWorkspace).</item>
/// </list>
///
/// <para>
/// <b>Por qué es necesario el fallback en disco:</b><br/>
/// El Razor Source Generator y generadores propios (<c>IIncrementalGenerator</c>)
/// que dependen de <c>AdditionalFiles</c> o <c>AnalyzerConfigOptions</c> no se
/// ejecutan en <c>MSBuildWorkspace</c> en modo design-time, por lo que
/// <c>GetSourceGeneratedDocumentsAsync</c> devuelve 0 documentos para ellos.
/// Si el proyecto se ha compilado previamente con <c>dotnet build</c>, todos sus
/// ficheros generados existen en disco bajo
/// <c>obj/{config}/{tfm}/generated/{GeneratorAssembly}/{GeneratorType}/</c>
/// y podemos leerlos y añadirlos a la compilación directamente.
/// </para>
///
/// <para>
/// <b>Deduplicación entre configuraciones y TFMs:</b><br/>
/// Un proyecto multi-target o con ambas configuraciones (Debug + Release) produce
/// el mismo fichero lógico en rutas distintas, p.ej.:
/// <c>obj/Debug/net8.0/generated/X/Y/File.cs</c> y
/// <c>obj/Release/net8.0/generated/X/Y/File.cs</c>.
/// Añadir ambas a la compilación provoca que Roslyn vea declaraciones de tipo
/// duplicadas, dejando los símbolos en estado de error e invisibles para el
/// extractor. Por eso deduplicamos por <b>ruta lógica</b> (lo que sigue a
/// <c>generated/</c>), conservando siempre la copia de mayor prioridad
/// (Release &gt; Debug &gt; cualquier otra).
/// </para>
/// </summary>
internal static class OwnFileCollector
{
    // Nombre del directorio estándar donde MSBuild/Roslyn deposita la salida
    // de los source generators: obj/{config}/{tfm}/generated/
    private const string GeneratedFolderName = "generated";

    // Fragmento de ruta del Razor Source Generator (para logging fino)
    private const string RazorGeneratorFolderFragment =
        "Microsoft.NET.Sdk.Razor.SourceGenerators";

    // Orden de preferencia al deduplicar entre configuraciones de build.
    // Si existe la misma ruta lógica en Release y Debug, tomamos Release
    // porque es la configuración que usa MSBuildWorkspace (buildProps["Configuration"]="Release").
    private static readonly string[] ConfigPriority = ["Release", "Debug"];

    public static async Task<OwnFiles> CollectAsync(Project project, string? baseIntermediateOutputPath = null)
    {
        HashSet<string> paths = new(StringComparer.OrdinalIgnoreCase);

        // ── 1. Documentos C# explícitos del proyecto ─────────────────────
        foreach (Document doc in project.Documents)
            if (doc.FilePath is not null)
                paths.Add(doc.FilePath);

        // ── 2. Documentos de source generators via API de Roslyn ─────────
        //       Funciona para generators bien integrados con MSBuildWorkspace.
        //       Para Razor e IIncrementalGenerators con AdditionalFiles,
        //       habitualmente devuelve 0 → cubierto por el paso 3.
        IEnumerable<SourceGeneratedDocument> generatedDocs =
            await project.GetSourceGeneratedDocumentsAsync();
        int roslynGeneratedCount = 0;
        foreach (SourceGeneratedDocument doc in generatedDocs)
        {
            if (doc.FilePath is null) continue;
            paths.Add(doc.FilePath);
            roslynGeneratedCount++;
        }

        // ── 3. Fallback: escanear obj/*/generated/ en disco ──────────────
        //
        //   Paso 3a – Recopilar candidatos (todos los .cs en *cualquier*
        //             carpeta "generated" bajo obj/, salvo los que Roslyn
        //             ya conoce).
        //
        //   Paso 3b – Deduplicar por ruta lógica (segmento posterior a
        //             "generated/"), prefiriendo Release > Debug > resto.
        //             Sin este paso, un mismo fichero lógico aparece dos
        //             veces en la compilación (Debug + Release) y Roslyn
        //             reporta "duplicate type", dejando los símbolos
        //             inutilizables.
        //
        //   Paso 3c – Añadir los paths definitivos a `paths` y
        //             `diskGeneratedPaths`.

        HashSet<string> diskGeneratedPaths = new(StringComparer.OrdinalIgnoreCase);

        if (project.FilePath is not null)
        {
            string objDir;

            if (!string.IsNullOrEmpty(baseIntermediateOutputPath))
            {
                // 🔥 obj aislado (nuevo comportamiento)
                string projectName = Path.GetFileNameWithoutExtension(project.FilePath);

                objDir = Path.Combine(baseIntermediateOutputPath, projectName, "obj");
            }
            else
            {
                // comportamiento original
                objDir = Path.Combine(
                    Path.GetDirectoryName(project.FilePath)!, "obj");
            }

            if (Directory.Exists(objDir))
            {
                // 3a – Candidatos
                List<string> candidates = [];
                foreach (string genFolder in Directory.EnumerateDirectories(
                    objDir, GeneratedFolderName, SearchOption.AllDirectories))
                {
                    foreach (string file in Directory.EnumerateFiles(
                        genFolder, "*.cs", SearchOption.AllDirectories))
                    {
                        if (paths.Contains(file)) continue; // Roslyn ya lo tiene
                        candidates.Add(file);
                    }
                }

                // 3b – Deduplicar por ruta lógica
                //      "Ruta lógica" = parte de la ruta tras el segmento "generated/":
                //      obj/Release/net8.0/generated/MyGen/MyType/File.cs
                //                                   └─────────────────────┘  lógica
                Dictionary<string, string> logicalToPhysical =
                    new(StringComparer.OrdinalIgnoreCase);

                foreach (string file in candidates)
                {
                    string logical = GetLogicalPath(file);

                    if (!logicalToPhysical.TryGetValue(logical, out string? existing))
                    {
                        logicalToPhysical[logical] = file;
                    }
                    else
                    {
                        // Preferir según ConfigPriority
                        int newPrio = GetConfigPriority(file);
                        int existingPrio = GetConfigPriority(existing);
                        if (newPrio < existingPrio) // menor índice = mayor prioridad
                            logicalToPhysical[logical] = file;
                    }
                }

                // 3c – Añadir definitivos
                foreach (string file in logicalToPhysical.Values)
                {
                    paths.Add(file);
                    diskGeneratedPaths.Add(file);
                }

                // Diagnóstico si no se encontró nada
                if (diskGeneratedPaths.Count == 0 && roslynGeneratedCount == 0)
                {
                    bool hasGeneratedFolder = Directory.EnumerateDirectories(
                        objDir, GeneratedFolderName, SearchOption.AllDirectories).Any();

                    if (!hasGeneratedFolder)
                    {
                        Console.WriteLine(
                            "  [WARN] No se encontró la carpeta obj/generated/. " +
                            "Ejecuta 'dotnet build' primero para que los source " +
                            "generators produzcan sus ficheros.");
                    }
                    else
                    {
                        Console.WriteLine(
                            "  [WARN] Se encontró obj/generated/ pero todos sus .cs " +
                            "ya estaban en los documentos del proyecto (Roslyn los conoce).");
                    }
                }
            }
        }

        int diskRazor = diskGeneratedPaths.Count(IsRazorGenerated);
        int diskOther = diskGeneratedPaths.Count - diskRazor;

        return new OwnFiles(paths, roslynGeneratedCount, diskGeneratedPaths, diskRazor, diskOther);
    }

    public static bool IsRazorGenerated(string filePath)
        => filePath.EndsWith(".razor.g.cs", StringComparison.OrdinalIgnoreCase)
        || filePath.Contains(RazorGeneratorFolderFragment, StringComparison.OrdinalIgnoreCase);

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Extrae la "ruta lógica": el segmento de <paramref name="filePath"/>
    /// a continuación del directorio llamado <c>generated</c>.
    /// Ejemplo: <c>obj/Release/net8.0/generated/MyGen/MyType/File.cs</c>
    /// → <c>MyGen/MyType/File.cs</c>.
    /// Si no se encuentra el segmento, devuelve la ruta completa como fallback.
    /// </summary>
    private static string GetLogicalPath(string filePath)
    {
        // Buscamos el segmento de directorio exactamente llamado "generated"
        string sep = Path.DirectorySeparatorChar.ToString();
        string needle1 = sep + GeneratedFolderName + sep;          // /generated/
        string needle2 = sep + GeneratedFolderName;                // /generated  (final de ruta)

        int idx = filePath.IndexOf(needle1, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
            return filePath[(idx + needle1.Length)..];

        // Por si "generated" es el último segmento sin barra final
        idx = filePath.LastIndexOf(needle2, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0 && idx + needle2.Length == filePath.Length)
            return string.Empty;

        return filePath; // fallback: la ruta entera como clave única
    }

    /// <summary>
    /// Índice de prioridad del fichero según la configuración de build que
    /// contiene su ruta. Menor índice = mayor prioridad.
    /// Las rutas que no coincidan con ninguna configuración de
    /// <see cref="ConfigPriority"/> reciben el índice más alto (menor prio).
    /// </summary>
    private static int GetConfigPriority(string filePath)
    {
        string[] segments = filePath.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < OwnFileCollector.ConfigPriority.Length; i++)
        {
            string prio = OwnFileCollector.ConfigPriority[i];
            if (segments.Any(s => s.Equals(prio, StringComparison.OrdinalIgnoreCase)))
                return i;
        }

        return OwnFileCollector.ConfigPriority.Length; // sin coincidencia → prioridad mínima
    }
}

/// <summary>Resultado de <see cref="OwnFileCollector.CollectAsync"/>.</summary>
internal sealed class OwnFiles(
    HashSet<string> paths,
    int roslynGenerated,
    HashSet<string> diskGeneratedPaths,
    int diskRazor,
    int diskOther)
{
    /// <summary>Todos los paths conocidos (C# + generados via Roslyn + generados en disco).</summary>
    public HashSet<string> Paths { get; } = paths;
    public int Count => Paths.Count;

    /// <summary>Ficheros devueltos por <c>GetSourceGeneratedDocumentsAsync</c>.</summary>
    public int RoslynGenerated { get; } = roslynGenerated;

    /// <summary>
    /// Ficheros generados encontrados en <c>obj/*/generated/</c> que Roslyn
    /// <b>no</b> devolvió (ni en el paso 1 ni en el paso 2). Se añaden
    /// manualmente a la compilación en Program.cs.
    /// </summary>
    public HashSet<string> DiskGeneratedPaths { get; } = diskGeneratedPaths;

    /// <summary>Cuántos de los ficheros en disco son de Razor.</summary>
    public int DiskRazor { get; } = diskRazor;

    /// <summary>Cuántos son de otros generators (IIncrementalGenerator propios, etc.).</summary>
    public int DiskOther { get; } = diskOther;

    /// <summary>Total de ficheros generados en disco (Razor + otros).</summary>
    public int DiskGenerated => DiskRazor + DiskOther;

    public bool Contains(string path) => Paths.Contains(path);
}