using CdCSharp.Theon.Analysis;
using System.Text;
using System.Text.RegularExpressions;

namespace CdCSharp.Theon.Context;

/// <summary>
/// Conocimiento compartido del proyecto accesible por todos los contextos.
/// Proporciona índices optimizados para búsqueda rápida sin cargar archivos completos.
/// </summary>
public sealed class SharedProjectKnowledge
{
    private readonly IProjectContext _projectContext;
    private ProjectInfo? _cachedProject;
    private Dictionary<string, FileSummary>? _fileIndex;
    private Dictionary<string, List<TypeSummary>>? _typeIndex;
    private Dictionary<string, AssemblyInfo>? _assemblyByFile;

    public SharedProjectKnowledge(IProjectContext projectContext)
    {
        _projectContext = projectContext;
    }

    /// <summary>
    /// Obtiene la información completa del proyecto (con cache).
    /// </summary>
    public async Task<ProjectInfo> GetProjectAsync(CancellationToken ct = default)
    {
        if (_cachedProject == null)
        {
            _cachedProject = await _projectContext.GetProjectAsync(ct);
            BuildIndices(_cachedProject);
        }
        return _cachedProject;
    }

    /// <summary>
    /// Índice de archivos por ruta relativa.
    /// </summary>
    public IReadOnlyDictionary<string, FileSummary> FileIndex
    {
        get
        {
            EnsureInitialized();
            return _fileIndex!;
        }
    }

    /// <summary>
    /// Índice de tipos por nombre completo (Namespace.TypeName).
    /// </summary>
    public IReadOnlyDictionary<string, List<TypeSummary>> TypeIndex
    {
        get
        {
            EnsureInitialized();
            return _typeIndex!;
        }
    }

    /// <summary>
    /// Índice de ensamblado por archivo.
    /// </summary>
    public IReadOnlyDictionary<string, AssemblyInfo> AssemblyByFile
    {
        get
        {
            EnsureInitialized();
            return _assemblyByFile!;
        }
    }

    /// <summary>
    /// Busca archivos por patrón glob simplificado.
    /// </summary>
    public IEnumerable<string> FindFilesByPattern(string pattern)
    {
        EnsureInitialized();

        // Convertir patrón simple a regex
        string regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*\\*/", ".*")  // **/ matches any directory depth
            .Replace("\\*", "[^/]*")    // * matches anything except /
            .Replace("\\?", ".")        // ? matches single char
            + "$";

        Regex regex = new(regexPattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return _fileIndex!.Keys.Where(path => regex.IsMatch(path));
    }

    /// <summary>
    /// Busca tipos por nombre (parcial, case-insensitive).
    /// </summary>
    public IEnumerable<TypeSummary> FindTypesByName(string namePattern)
    {
        EnsureInitialized();

        string pattern = namePattern.ToLowerInvariant();

        return _typeIndex!
            .Where(kvp => kvp.Key.ToLowerInvariant().Contains(pattern))
            .SelectMany(kvp => kvp.Value);
    }

    /// <summary>
    /// Encuentra el ensamblado que contiene un archivo.
    /// </summary>
    public AssemblyInfo? FindAssemblyContaining(string filePath)
    {
        EnsureInitialized();
        return _assemblyByFile!.GetValueOrDefault(filePath);
    }

    /// <summary>
    /// Verifica si un archivo existe en el proyecto.
    /// </summary>
    public bool FileExists(string path)
    {
        EnsureInitialized();
        return _fileIndex!.ContainsKey(path);
    }

    /// <summary>
    /// Encuentra archivos similares (útil para sugerencias).
    /// </summary>
    public IEnumerable<string> FindSimilarFiles(string path, int maxResults = 5)
    {
        EnsureInitialized();

        string fileName = Path.GetFileName(path);
        string directory = Path.GetDirectoryName(path) ?? "";

        // Buscar por similitud de nombre de archivo y directorio
        return _fileIndex!.Keys
            .Select(p => new
            {
                Path = p,
                Score = CalculateSimilarity(p, path, fileName, directory)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(maxResults)
            .Select(x => x.Path);
    }

    /// <summary>
    /// Obtiene resumen compacto de la estructura del proyecto.
    /// </summary>
    public string GetCompactSummary()
    {
        EnsureInitialized();

        StringBuilder sb = new();
        sb.AppendLine("## Project Structure");
        sb.AppendLine();

        foreach (AssemblyInfo? assembly in _cachedProject!.Assemblies.Where(a => !a.IsTestProject))
        {
            sb.AppendLine($"📦 **{assembly.Name}** ({assembly.Files.Count} files, {assembly.Types.Count} types)");

            // Top 8 namespaces más poblados
            IEnumerable<IGrouping<string, TypeSummary>> topNamespaces = assembly.Types
                .GroupBy(t => t.Namespace)
                .OrderByDescending(g => g.Count())
                .Take(8);

            foreach (IGrouping<string, TypeSummary>? ns in topNamespaces)
            {
                sb.AppendLine($"   └─ {ns.Key}/ ({ns.Count()} types)");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Obtiene resumen detallado con tipos.
    /// </summary>
    public string GetDetailedSummary()
    {
        EnsureInitialized();

        StringBuilder sb = new();
        sb.AppendLine("## Project Structure (Detailed)");
        sb.AppendLine();

        foreach (AssemblyInfo? assembly in _cachedProject!.Assemblies.Where(a => !a.IsTestProject))
        {
            sb.AppendLine($"📦 **{assembly.Name}**");
            sb.AppendLine($"   Path: {assembly.RelativePath}");
            sb.AppendLine($"   Files: {assembly.Files.Count}, Types: {assembly.Types.Count}");
            sb.AppendLine();

            IOrderedEnumerable<IGrouping<string, TypeSummary>> namespaceGroups = assembly.Types
                .GroupBy(t => t.Namespace)
                .OrderBy(g => g.Key);

            foreach (IGrouping<string, TypeSummary>? ns in namespaceGroups)
            {
                sb.AppendLine($"   **{ns.Key}**");

                foreach (TypeSummary? type in ns.Take(20))
                {
                    string icon = type.Kind switch
                    {
                        TypeKind.Interface => "🔷",
                        TypeKind.Class => "📘",
                        TypeKind.Record => "📗",
                        TypeKind.Struct => "📙",
                        TypeKind.Enum => "📊",
                        _ => "📄"
                    };
                    sb.AppendLine($"      {icon} {type.Name} ({type.Kind})");
                }

                if (ns.Count() > 20)
                {
                    sb.AppendLine($"      ... and {ns.Count() - 20} more types");
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Obtiene lista de todos los ensamblados (excluyendo tests).
    /// </summary>
    public IReadOnlyList<string> GetAssemblyNames()
    {
        EnsureInitialized();
        return _cachedProject!.Assemblies
            .Where(a => !a.IsTestProject)
            .Select(a => a.Name)
            .ToList();
    }

    /// <summary>
    /// Invalida el cache cuando el proyecto cambia.
    /// </summary>
    public void InvalidateCache()
    {
        _cachedProject = null;
        _fileIndex = null;
        _typeIndex = null;
        _assemblyByFile = null;
    }

    private void EnsureInitialized()
    {
        if (_cachedProject == null)
        {
            _cachedProject = _projectContext.GetProjectAsync().GetAwaiter().GetResult();
            BuildIndices(_cachedProject);
        }
    }

    private void BuildIndices(ProjectInfo project)
    {
        _fileIndex = new Dictionary<string, FileSummary>(StringComparer.OrdinalIgnoreCase);
        _typeIndex = new Dictionary<string, List<TypeSummary>>(StringComparer.OrdinalIgnoreCase);
        _assemblyByFile = new Dictionary<string, AssemblyInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (AssemblyInfo assembly in project.Assemblies)
        {
            // Índice de archivos
            foreach (FileSummary file in assembly.Files)
            {
                _fileIndex[file.Path] = file;
                _assemblyByFile[file.Path] = assembly;
            }

            // Índice de tipos
            foreach (TypeSummary type in assembly.Types)
            {
                string fullName = $"{type.Namespace}.{type.Name}";

                if (!_typeIndex.ContainsKey(fullName))
                {
                    _typeIndex[fullName] = [];
                }

                _typeIndex[fullName].Add(type);

                // También indexar por nombre simple
                if (!_typeIndex.ContainsKey(type.Name))
                {
                    _typeIndex[type.Name] = [];
                }
                _typeIndex[type.Name].Add(type);
            }
        }
    }

    private static int CalculateSimilarity(string candidatePath, string targetPath, string targetFileName, string targetDirectory)
    {
        int score = 0;

        string candidateFileName = Path.GetFileName(candidatePath);
        string candidateDirectory = Path.GetDirectoryName(candidatePath) ?? "";

        // Nombre de archivo coincide exactamente
        if (candidateFileName.Equals(targetFileName, StringComparison.OrdinalIgnoreCase))
        {
            score += 100;
        }
        // Nombre de archivo contiene el target
        else if (candidateFileName.Contains(targetFileName, StringComparison.OrdinalIgnoreCase))
        {
            score += 50;
        }
        // Mismo directorio
        else if (candidateDirectory.Equals(targetDirectory, StringComparison.OrdinalIgnoreCase))
        {
            score += 30;
        }

        // Directorio similar (prefijo común)
        var commonPrefix = GetCommonPrefix(candidateDirectory, targetDirectory);
        score += commonPrefix.Length / 2;

        return score;
    }

    private static string GetCommonPrefix(string a, string b)
    {
        int minLen = Math.Min(a.Length, b.Length);
        int i = 0;
        while (i < minLen && char.ToLowerInvariant(a[i]) == char.ToLowerInvariant(b[i]))
        {
            i++;
        }
        return a[..i];
    }
}