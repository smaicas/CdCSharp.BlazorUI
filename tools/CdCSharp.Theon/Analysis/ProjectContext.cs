using CdCSharp.Theon.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml.Linq;

namespace CdCSharp.Theon.Analysis;

public interface IProjectContext
{
    Task<ProjectInfo> GetProjectAsync(CancellationToken ct = default);
    int GetFileTokens(string path);
}

public class ProjectContext : IProjectContext, IFileSystemObserver, IDisposable
{
    private readonly IFileSystem _fileSystem;
    private readonly ITheonLogger _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private ProjectInfo? _cachedProject;
    private readonly Dictionary<string, AssemblyInfo> _assembliesByPath = [];
    private readonly Dictionary<string, List<TypeSummary>> _typesByFile = [];
    private readonly Dictionary<string, FileSummary> _fileSummaries = [];

    private static readonly string[] TestIndicators = ["xunit", "nunit", "mstest", "Test.Sdk"];

    public ProjectContext(IFileSystem fileSystem, ITheonLogger logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _fileSystem.RegisterObserver(this);
    }

    public async Task<ProjectInfo> GetProjectAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_cachedProject == null)
            {
                _cachedProject = await AnalyzeProjectInfo(ct);
            }
            return _cachedProject;
        }
        finally
        {
            _lock.Release();
        }
    }

    public int GetFileTokens(string path)
    {
        if (_fileSummaries.TryGetValue(path, out FileSummary? summary))
        {
            return summary.EstimatedTokens;
        }
        return 0;
    }

    /// <summary>
    /// Thread-safe observer callback for file changes.
    /// Enqueues changes and processes them asynchronously with proper locking.
    /// </summary>
    public void OnFileChanged(string relativePath, FileChangeType changeType)
    {
        // Process asynchronously to avoid blocking the caller
        _ = Task.Run(async () =>
        {
            try
            {
                await _lock.WaitAsync();
                try
                {
                    await HandleFileChangeAsync(relativePath, changeType);
                }
                finally
                {
                    _lock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error handling file change for {relativePath}", ex);
            }
        });
    }

    private async Task HandleFileChangeAsync(string relativePath, FileChangeType changeType)
    {
        _logger.Debug($"File {changeType}: {relativePath}");

        if (_cachedProject == null) return;

        string ext = Path.GetExtension(relativePath).ToLowerInvariant();

        if (ext == ".csproj")
        {
            await HandleCsprojChangeAsync(relativePath, changeType);
        }
        else if (ext == ".cs")
        {
            await HandleCsFileChangeAsync(relativePath, changeType);
        }
        else if (ext == ".razor")
        {
            await HandleRazorFileChangeAsync(relativePath, changeType);
        }
    }

    private async Task HandleCsprojChangeAsync(string csprojPath, FileChangeType changeType)
    {
        string assemblyDir = Path.GetDirectoryName(csprojPath) ?? "";

        if (changeType == FileChangeType.Deleted)
        {
            _assembliesByPath.Remove(assemblyDir);
            RebuildProjectInfo();
            _logger.Debug($"Assembly removed from cache: {assemblyDir}");
            return;
        }

        AssemblyInfo? newAssembly = await AnalyzeAssemblyAsync(csprojPath, default);
        if (newAssembly != null)
        {
            _assembliesByPath[assemblyDir] = newAssembly;

            List<string> oldFiles = _fileSummaries.Keys.Where(f => f.StartsWith(assemblyDir)).ToList();
            foreach (string file in oldFiles)
            {
                if (!newAssembly.Files.Any(f => f.Path == file))
                {
                    _fileSummaries.Remove(file);
                    _typesByFile.Remove(file);
                }
            }

            RebuildProjectInfo();
            _logger.Debug($"Assembly {(changeType == FileChangeType.Created ? "added to" : "updated in")} cache: {newAssembly.Name}");
        }
    }

    private async Task HandleCsFileChangeAsync(string filePath, FileChangeType changeType)
    {
        if (changeType == FileChangeType.Deleted)
        {
            _typesByFile.Remove(filePath);
            _fileSummaries.Remove(filePath);
            UpdateAssemblyForFile(filePath);
            _logger.Debug($"Types removed for deleted file: {filePath}");
            return;
        }

        string? assemblyDir = FindAssemblyForFile(filePath);
        if (assemblyDir == null)
        {
            _logger.Debug($"No assembly found for file: {filePath}");
            return;
        }

        AssemblyInfo? assembly = _assembliesByPath.GetValueOrDefault(assemblyDir);
        if (assembly == null || assembly.IsTestProject) return;

        string? content = await _fileSystem.ReadFileAsync(filePath, default);
        if (content != null)
        {
            List<TypeSummary> types = ExtractTypes(filePath, content);
            _typesByFile[filePath] = types;

            FileSummary fileSummary = new(
                filePath,
                content.Length,
                EstimateTokens(content));
            _fileSummaries[filePath] = fileSummary;

            UpdateAssemblyForFile(filePath);
            _logger.Debug($"File updated: {filePath} ({fileSummary.EstimatedTokens} tokens)");
        }
    }

    private Task HandleRazorFileChangeAsync(string filePath, FileChangeType changeType)
    {
        if (changeType == FileChangeType.Deleted)
        {
            _fileSummaries.Remove(filePath);
        }
        UpdateAssemblyForFile(filePath);
        _logger.Debug($"Razor file {changeType}: {filePath}");
        return Task.CompletedTask;
    }

    private string? FindAssemblyForFile(string filePath)
    {
        string dir = Path.GetDirectoryName(filePath) ?? "";

        while (!string.IsNullOrEmpty(dir))
        {
            if (_assembliesByPath.ContainsKey(dir))
                return dir;

            string? parent = Path.GetDirectoryName(dir);
            if (parent == dir) break;
            dir = parent ?? "";
        }

        return null;
    }

    private void UpdateAssemblyForFile(string filePath)
    {
        string? assemblyDir = FindAssemblyForFile(filePath);
        if (assemblyDir == null) return;

        AssemblyInfo? assembly = _assembliesByPath.GetValueOrDefault(assemblyDir);
        if (assembly == null) return;

        List<FileSummary> files = _fileSummaries
            .Where(kvp => kvp.Key.StartsWith(assemblyDir))
            .Select(kvp => kvp.Value)
            .ToList();

        List<TypeSummary> types = _typesByFile
            .Where(kvp => kvp.Key.StartsWith(assemblyDir))
            .SelectMany(kvp => kvp.Value)
            .ToList();

        AssemblyInfo updatedAssembly = assembly with
        {
            Files = files,
            Types = types
        };

        _assembliesByPath[assemblyDir] = updatedAssembly;
        RebuildProjectInfo();
    }

    private void RebuildProjectInfo()
    {
        _cachedProject = new ProjectInfo(
            _cachedProject?.Name ?? "",
            _cachedProject?.RootPath ?? "",
            _assembliesByPath.Values.ToList()
        );
    }

    private async Task<ProjectInfo> AnalyzeProjectInfo(CancellationToken ct = default)
    {
        _logger.Info("Analyzing project structure...");

        List<string> csprojFiles = _fileSystem
            .EnumerateFiles(null, "*.csproj")
            .ToList();

        _logger.Debug($"Found {csprojFiles.Count} project files (assemblies)");

        _assembliesByPath.Clear();
        _typesByFile.Clear();
        _fileSummaries.Clear();

        foreach (string csproj in csprojFiles)
        {
            ct.ThrowIfCancellationRequested();
            AssemblyInfo? assembly = await AnalyzeAssemblyAsync(csproj, ct);
            if (assembly != null)
            {
                string dir = Path.GetDirectoryName(csproj) ?? "";
                _assembliesByPath[dir] = assembly;
            }
        }

        ProjectInfo project = new("", "", _assembliesByPath.Values.ToList());
        _logger.Info($"Project analyzed: {project.Assemblies.Count} assemblies, {project.TotalTokens:N0} total tokens");

        return project;
    }

    private async Task<AssemblyInfo?> AnalyzeAssemblyAsync(string csprojPath, CancellationToken ct)
    {
        string? content = await _fileSystem.ReadFileAsync(csprojPath, ct);
        if (content == null) return null;

        XDocument csproj;
        try
        {
            csproj = XDocument.Parse(content);
        }
        catch
        {
            _logger.Warning($"Could not parse: {csprojPath}");
            return null;
        }

        string name = Path.GetFileNameWithoutExtension(csprojPath);
        string relativePath = Path.GetDirectoryName(csprojPath) ?? "";

        List<string> references = ExtractReferences(csproj);
        bool isTest = references.Any(r => TestIndicators.Any(t => r.Contains(t, StringComparison.OrdinalIgnoreCase)));

        List<string> filePaths = _fileSystem
            .EnumerateFiles(relativePath, "*.cs")
            .Concat(_fileSystem.EnumerateFiles(relativePath, "*.razor"))
            .ToList();

        List<FileSummary> files = new();
        List<TypeSummary> types = new();

        if (!isTest)
        {
            foreach (string file in filePaths)
            {
                ct.ThrowIfCancellationRequested();
                string? fileContent = await _fileSystem.ReadFileAsync(file, ct);
                if (fileContent != null)
                {
                    int tokens = EstimateTokens(fileContent);
                    FileSummary fileSummary = new(file, fileContent.Length, tokens);
                    files.Add(fileSummary);
                    _fileSummaries[file] = fileSummary;

                    if (file.EndsWith(".cs"))
                    {
                        List<TypeSummary> fileTypes = ExtractTypes(file, fileContent);
                        types.AddRange(fileTypes);
                        _typesByFile[file] = fileTypes;
                    }
                }
            }
        }
        else
        {
            foreach (string file in filePaths)
            {
                files.Add(new FileSummary(file, 0, 0));
            }
        }

        return new AssemblyInfo(name, relativePath, isTest, references, files, types);
    }

    private static List<string> ExtractReferences(XDocument csproj)
    {
        List<string> refs = new();

        foreach (XElement? pkg in csproj.Descendants().Where(e => e.Name.LocalName == "PackageReference"))
        {
            string? include = pkg.Attribute("Include")?.Value;
            if (!string.IsNullOrEmpty(include))
                refs.Add(include);
        }

        foreach (XElement? proj in csproj.Descendants().Where(e => e.Name.LocalName == "ProjectReference"))
        {
            string? include = proj.Attribute("Include")?.Value;
            if (!string.IsNullOrEmpty(include))
                refs.Add($"[Project] {Path.GetFileNameWithoutExtension(include)}");
        }

        return refs;
    }

    private List<TypeSummary> ExtractTypes(string filePath, string content)
    {
        List<TypeSummary> types = new();

        try
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(content);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            foreach (MemberDeclarationSyntax member in root.Members)
            {
                if (member is BaseNamespaceDeclarationSyntax ns)
                {
                    string nsName = ns.Name.ToString();
                    foreach (MemberDeclarationSyntax typeMember in ns.Members)
                    {
                        if (typeMember is TypeDeclarationSyntax typeDecl)
                        {
                            TypeKind kind = typeDecl switch
                            {
                                ClassDeclarationSyntax => TypeKind.Class,
                                InterfaceDeclarationSyntax => TypeKind.Interface,
                                RecordDeclarationSyntax => TypeKind.Record,
                                StructDeclarationSyntax => TypeKind.Struct,
                                _ => TypeKind.Class
                            };

                            types.Add(new TypeSummary(nsName, typeDecl.Identifier.Text, kind, filePath));
                        }
                        else if (typeMember is EnumDeclarationSyntax enumDecl)
                        {
                            types.Add(new TypeSummary(nsName, enumDecl.Identifier.Text, TypeKind.Enum, filePath));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"Could not parse {filePath}: {ex.Message}");
        }

        return types;
    }

    private static int EstimateTokens(string text) => TokenEstimator.Estimate(text);

    public void Dispose()
    {
        _fileSystem.UnregisterObserver(this);
        _lock.Dispose();
    }
}

public sealed record ProjectInfo(
    string Name,
    string RootPath,
    IReadOnlyList<AssemblyInfo> Assemblies)
{
    public int TotalTokens => Assemblies
        .Where(a => !a.IsTestProject)
        .Sum(a => a.TotalTokens);
}

public sealed record AssemblyInfo(
    string Name,
    string RelativePath,
    bool IsTestProject,
    IReadOnlyList<string> References,
    IReadOnlyList<FileSummary> Files,
    IReadOnlyList<TypeSummary> Types)
{
    public int TotalTokens => Files.Sum(f => f.EstimatedTokens);
}

public sealed record FileSummary(
    string Path,
    int SizeBytes,
    int EstimatedTokens);

public sealed record TypeSummary(
    string Namespace,
    string Name,
    TypeKind Kind,
    string FilePath);

public enum TypeKind
{
    Class,
    Interface,
    Record,
    Struct,
    Enum
}