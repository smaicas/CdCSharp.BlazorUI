using CdCSharp.Theon.Analysis;
using System.Text;
using System.Text.RegularExpressions;

namespace CdCSharp.Theon.Context;

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

    public async Task<ProjectInfo> GetProjectAsync(CancellationToken ct = default)
    {
        if (_cachedProject == null)
        {
            _cachedProject = await _projectContext.GetProjectAsync(ct);
            BuildIndices(_cachedProject);
        }
        return _cachedProject;
    }

    public IReadOnlyDictionary<string, FileSummary> FileIndex
    {
        get
        {
            EnsureInitialized();
            return _fileIndex!;
        }
    }

    public IReadOnlyDictionary<string, List<TypeSummary>> TypeIndex
    {
        get
        {
            EnsureInitialized();
            return _typeIndex!;
        }
    }

    public IReadOnlyDictionary<string, AssemblyInfo> AssemblyByFile
    {
        get
        {
            EnsureInitialized();
            return _assemblyByFile!;
        }
    }

    public IEnumerable<string> FindFilesByPattern(string pattern)
    {
        EnsureInitialized();

        string regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*\\*/", ".*")
            .Replace("\\*", "[^/]*")
            .Replace("\\?", ".")
            + "$";

        Regex regex = new(regexPattern, RegexOptions.IgnoreCase);

        return _fileIndex!.Keys.Where(path => regex.IsMatch(path));
    }

    public IEnumerable<TypeSummary> FindTypesByName(string namePattern)
    {
        EnsureInitialized();

        string pattern = namePattern.ToLowerInvariant();

        return _typeIndex!
            .Where(kvp => kvp.Key.ToLowerInvariant().Contains(pattern))
            .SelectMany(kvp => kvp.Value);
    }

    public AssemblyInfo? FindAssemblyContaining(string filePath)
    {
        EnsureInitialized();
        return _assemblyByFile!.GetValueOrDefault(filePath);
    }

    public bool FileExists(string path)
    {
        EnsureInitialized();
        return _fileIndex!.ContainsKey(path);
    }

    public IEnumerable<string> FindSimilarFiles(string path, int maxResults = 5)
    {
        EnsureInitialized();

        string fileName = Path.GetFileName(path);
        string directory = Path.GetDirectoryName(path) ?? "";

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
    /// Returns the file index formatted for inclusion in prompts.
    /// Shows exact paths and estimated tokens for each file.
    /// </summary>
    public string GetFileIndex()
    {
        EnsureInitialized();

        StringBuilder sb = new();
        sb.AppendLine("## File Index (use exact paths with read_file)");
        sb.AppendLine();

        IEnumerable<IGrouping<string, KeyValuePair<string, FileSummary>>> groupedByDirectory = _fileIndex!
            .Where(kvp => kvp.Value.EstimatedTokens > 0)
            .GroupBy(kvp => Path.GetDirectoryName(kvp.Key) ?? "")
            .OrderBy(g => g.Key);

        foreach (IGrouping<string, KeyValuePair<string, FileSummary>> group in groupedByDirectory)
        {
            string dirName = string.IsNullOrEmpty(group.Key) ? "(root)" : group.Key;
            sb.AppendLine($"**{dirName}/**");

            foreach (KeyValuePair<string, FileSummary> file in group.OrderBy(f => f.Key))
            {
                string fileName = Path.GetFileName(file.Key);
                sb.AppendLine($"  - `{file.Key}` ({file.Value.EstimatedTokens} tokens)");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns a compact summary with assembly overview and type counts.
    /// </summary>
    public string GetCompactSummary()
    {
        EnsureInitialized();

        StringBuilder sb = new();
        sb.AppendLine("## Project Structure");
        sb.AppendLine();

        foreach (AssemblyInfo assembly in _cachedProject!.Assemblies.Where(a => !a.IsTestProject))
        {
            sb.AppendLine($"**{assembly.Name}** ({assembly.Files.Count} files, {assembly.Types.Count} types, ~{assembly.TotalTokens:N0} tokens)");

            IEnumerable<IGrouping<string, TypeSummary>> topNamespaces = assembly.Types
                .GroupBy(t => t.Namespace)
                .OrderByDescending(g => g.Count())
                .Take(8);

            foreach (IGrouping<string, TypeSummary> ns in topNamespaces)
            {
                sb.AppendLine($"  - {ns.Key}/ ({ns.Count()} types)");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns detailed summary with all types listed.
    /// </summary>
    public string GetDetailedSummary()
    {
        EnsureInitialized();

        StringBuilder sb = new();
        sb.AppendLine("## Project Structure (Detailed)");
        sb.AppendLine();

        foreach (AssemblyInfo assembly in _cachedProject!.Assemblies.Where(a => !a.IsTestProject))
        {
            sb.AppendLine($"**{assembly.Name}**");
            sb.AppendLine($"  Path: {assembly.RelativePath}");
            sb.AppendLine($"  Files: {assembly.Files.Count}, Types: {assembly.Types.Count}");
            sb.AppendLine();

            IOrderedEnumerable<IGrouping<string, TypeSummary>> namespaceGroups = assembly.Types
                .GroupBy(t => t.Namespace)
                .OrderBy(g => g.Key);

            foreach (IGrouping<string, TypeSummary> ns in namespaceGroups)
            {
                sb.AppendLine($"  **{ns.Key}**");

                foreach (TypeSummary type in ns.Take(20))
                {
                    string icon = type.Kind switch
                    {
                        TypeKind.Interface => "[I]",
                        TypeKind.Class => "[C]",
                        TypeKind.Record => "[R]",
                        TypeKind.Struct => "[S]",
                        TypeKind.Enum => "[E]",
                        _ => "[?]"
                    };
                    sb.AppendLine($"    {icon} {type.Name}");
                }

                if (ns.Count() > 20)
                {
                    sb.AppendLine($"    ... and {ns.Count() - 20} more types");
                }
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    public IReadOnlyList<string> GetAssemblyNames()
    {
        EnsureInitialized();
        return _cachedProject!.Assemblies
            .Where(a => !a.IsTestProject)
            .Select(a => a.Name)
            .ToList();
    }

    public IReadOnlyList<string> GetAllFilePaths()
    {
        EnsureInitialized();
        return _fileIndex!.Keys.ToList();
    }

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
            foreach (FileSummary file in assembly.Files)
            {
                _fileIndex[file.Path] = file;
                _assemblyByFile[file.Path] = assembly;
            }

            foreach (TypeSummary type in assembly.Types)
            {
                string fullName = $"{type.Namespace}.{type.Name}";

                if (!_typeIndex.ContainsKey(fullName))
                    _typeIndex[fullName] = [];

                _typeIndex[fullName].Add(type);

                if (!_typeIndex.ContainsKey(type.Name))
                    _typeIndex[type.Name] = [];

                _typeIndex[type.Name].Add(type);
            }
        }
    }

    private static int CalculateSimilarity(string candidatePath, string targetPath, string targetFileName, string targetDirectory)
    {
        int score = 0;

        string candidateFileName = Path.GetFileName(candidatePath);
        string candidateDirectory = Path.GetDirectoryName(candidatePath) ?? "";

        if (candidateFileName.Equals(targetFileName, StringComparison.OrdinalIgnoreCase))
            score += 100;
        else if (candidateFileName.Contains(targetFileName, StringComparison.OrdinalIgnoreCase))
            score += 50;
        else if (candidateDirectory.Equals(targetDirectory, StringComparison.OrdinalIgnoreCase))
            score += 30;

        string commonPrefix = GetCommonPrefix(candidateDirectory, targetDirectory);
        score += commonPrefix.Length / 2;

        return score;
    }

    private static string GetCommonPrefix(string a, string b)
    {
        int minLen = Math.Min(a.Length, b.Length);
        int i = 0;
        while (i < minLen && char.ToLowerInvariant(a[i]) == char.ToLowerInvariant(b[i]))
            i++;
        return a[..i];
    }
}