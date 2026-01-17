using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;

namespace CdCSharp.Theon.Context;

public interface IScopeFactory
{
    ProjectScope CreateProjectScope();
    Task<AssemblyScope?> CreateAssemblyScopeAsync(string assemblyName, CancellationToken ct = default);
    Task<FileScope?> CreateFileScopeAsync(string relativePath, CancellationToken ct = default);
    Task<FolderScope?> CreateFolderScopeAsync(string folderPath, CancellationToken ct = default);
    Task<MultiFileScope?> CreateMultiFileScopeAsync(IReadOnlyList<string> paths, CancellationToken ct = default);
}

public sealed class ScopeFactory : IScopeFactory
{
    private readonly IProjectAnalysis _analysis;
    private readonly IFileSystem _fileSystem;
    private readonly ILlmClient _llmClient;
    private readonly ITheonLogger _logger;

    private int _maxContextTokens;
    private const int ReservedTokens = 4000;

    public ScopeFactory(
        IProjectAnalysis analysis,
        IFileSystem fileSystem,
        ILlmClient llmClient,
        ITheonLogger logger)
    {
        _analysis = analysis;
        _fileSystem = fileSystem;
        _llmClient = llmClient;
        _logger = logger;
    }

    private async Task EnsureContextLimitAsync(CancellationToken ct)
    {
        if (_maxContextTokens == 0)
        {
            ModelInfo info = await _llmClient.GetModelInfoAsync(ct);
            _maxContextTokens = info.ContextLength - ReservedTokens;
            _logger.Debug($"Context limit set to {_maxContextTokens} tokens");
        }
    }

    public ProjectScope CreateProjectScope()
    {
        if (_analysis.Project == null)
            throw new InvalidOperationException("Project not analyzed");

        return new ProjectScope(_analysis.Project);
    }

    public async Task<AssemblyScope?> CreateAssemblyScopeAsync(string assemblyName, CancellationToken ct = default)
    {
        if (_analysis.Project == null) return null;

        AssemblyInfo? assembly = _analysis.Project.Assemblies
            .FirstOrDefault(a => a.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));

        if (assembly == null)
        {
            assembly = _analysis.Project.Assemblies
                .FirstOrDefault(a => a.Name.Contains(assemblyName, StringComparison.OrdinalIgnoreCase));
        }

        if (assembly == null)
        {
            _logger.Warning($"Assembly not found: {assemblyName}");
            return null;
        }

        return new AssemblyScope(assembly);
    }

    public async Task<FileScope?> CreateFileScopeAsync(string relativePath, CancellationToken ct = default)
    {
        string? content = await _fileSystem.ReadFileAsync(relativePath, ct);
        if (content == null) return null;

        return new FileScope(relativePath, content);
    }

    public async Task<FolderScope?> CreateFolderScopeAsync(string folderPath, CancellationToken ct = default)
    {
        await EnsureContextLimitAsync(ct);

        if (!_fileSystem.DirectoryExists(folderPath))
        {
            _logger.Warning($"Folder not found: {folderPath}");
            return null;
        }

        List<string> files = _fileSystem
            .EnumerateFiles(folderPath, "*.*")
            .Where(f => f.EndsWith(".cs") || f.EndsWith(".razor"))
            .ToList();

        Dictionary<string, string> contents = [];
        foreach (string file in files)
        {
            string? content = await _fileSystem.ReadFileAsync(file, ct);
            if (content != null)
                contents[file] = content;
        }

        return new FolderScope(folderPath, contents, _maxContextTokens);
    }

    public async Task<MultiFileScope?> CreateMultiFileScopeAsync(IReadOnlyList<string> paths, CancellationToken ct = default)
    {
        Dictionary<string, string> contents = [];

        foreach (string path in paths)
        {
            string? content = await _fileSystem.ReadFileAsync(path, ct);
            if (content != null)
                contents[path] = content;
        }

        if (contents.Count == 0)
        {
            _logger.Warning($"No files found for paths: {string.Join(", ", paths)}");
            return null;
        }

        return new MultiFileScope(paths, contents);
    }
}