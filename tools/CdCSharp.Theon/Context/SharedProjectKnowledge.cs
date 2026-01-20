using CdCSharp.Theon.Analysis;
using System.Text.RegularExpressions;

namespace CdCSharp.Theon.Context;

public sealed class SharedProjectKnowledge
{
    private readonly IProjectContext _projectContext;
    private ProjectInfo? _cachedProject;
    private Dictionary<string, FileSummary>? _fileIndex;

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

        foreach (AssemblyInfo assembly in project.Assemblies)
        {
            foreach (FileSummary file in assembly.Files)
            {
                _fileIndex[file.Path] = file;
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