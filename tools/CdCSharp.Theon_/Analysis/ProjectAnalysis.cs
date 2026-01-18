using CdCSharp.Theon.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml.Linq;

namespace CdCSharp.Theon.Analysis;

public interface IProjectAnalysis
{
    ProjectInfo? Project { get; }
    Task AnalyzeAsync(CancellationToken ct = default);
    Task RefreshFileAsync(string relativePath, CancellationToken ct = default);
}

public sealed class ProjectAnalysis : IProjectAnalysis
{
    private readonly TheonOptions _options;
    private readonly IFileSystem _fileSystem;
    private readonly ITheonLogger _logger;

    private ProjectInfo? _project;
    private readonly Dictionary<string, List<TypeSummary>> _typesByFile = [];

    public ProjectInfo? Project => _project;

    private static readonly string[] TestIndicators = ["xunit", "nunit", "mstest", "Test.Sdk"];

    public ProjectAnalysis(TheonOptions options, IFileSystem fileSystem, ITheonLogger logger)
    {
        _options = options;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task AnalyzeAsync(CancellationToken ct = default)
    {
        _logger.Info("Analyzing project structure...");

        List<string> csprojFiles = _fileSystem
            .EnumerateFiles(null, "*.csproj")
            .ToList();

        _logger.Debug($"Found {csprojFiles.Count} project files");

        List<AssemblyInfo> assemblies = [];

        foreach (string csproj in csprojFiles)
        {
            ct.ThrowIfCancellationRequested();
            AssemblyInfo? assembly = await AnalyzeAssemblyAsync(csproj, ct);
            if (assembly != null)
                assemblies.Add(assembly);
        }

        string solutionName = FindSolutionName();

        _project = new ProjectInfo(solutionName, _options.ProjectPath, assemblies);

        _logger.Info($"Analysis complete: {assemblies.Count} assemblies, {assemblies.Sum(a => a.Types.Count)} types");
    }

    public async Task RefreshFileAsync(string relativePath, CancellationToken ct = default)
    {
        if (_project == null) return;

        _logger.Debug($"Refreshing analysis for: {relativePath}");

        string? content = await _fileSystem.ReadFileAsync(relativePath, ct);
        if (content == null) return;

        List<TypeSummary> types = await ExtractTypesAsync(relativePath, content);

        _typesByFile[relativePath] = types;

        RebuildProjectFromCache();
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

        List<string> files = _fileSystem
            .EnumerateFiles(relativePath, "*.cs")
            .Concat(_fileSystem.EnumerateFiles(relativePath, "*.razor"))
            .ToList();

        List<TypeSummary> types = [];
        if (!isTest)
        {
            foreach (string file in files.Where(f => f.EndsWith(".cs")))
            {
                ct.ThrowIfCancellationRequested();
                string? fileContent = await _fileSystem.ReadFileAsync(file, ct);
                if (fileContent != null)
                {
                    List<TypeSummary> fileTypes = await ExtractTypesAsync(file, fileContent);
                    types.AddRange(fileTypes);
                    _typesByFile[file] = fileTypes;
                }
            }
        }

        return new AssemblyInfo(name, relativePath, isTest, references, files, types);
    }

    private static List<string> ExtractReferences(XDocument csproj)
    {
        List<string> refs = [];

        foreach (XElement pkg in csproj.Descendants().Where(e => e.Name.LocalName == "PackageReference"))
        {
            string? include = pkg.Attribute("Include")?.Value;
            if (!string.IsNullOrEmpty(include))
                refs.Add(include);
        }

        foreach (XElement proj in csproj.Descendants().Where(e => e.Name.LocalName == "ProjectReference"))
        {
            string? include = proj.Attribute("Include")?.Value;
            if (!string.IsNullOrEmpty(include))
                refs.Add($"[Project] {Path.GetFileNameWithoutExtension(include)}");
        }

        return refs;
    }

    private Task<List<TypeSummary>> ExtractTypesAsync(string filePath, string content)
    {
        List<TypeSummary> types = [];

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

        return Task.FromResult(types);
    }

    private void RebuildProjectFromCache()
    {
        if (_project == null) return;

        List<AssemblyInfo> updatedAssemblies = _project.Assemblies.Select(assembly =>
        {
            List<TypeSummary> types = assembly.Files
                .Where(f => _typesByFile.ContainsKey(f))
                .SelectMany(f => _typesByFile[f])
                .ToList();

            return assembly with { Types = types };
        }).ToList();

        _project = _project with { Assemblies = updatedAssemblies };
    }

    private string FindSolutionName()
    {
        IEnumerable<string> slnFiles = _fileSystem.EnumerateFiles(null, "*.sln");
        string? first = slnFiles.FirstOrDefault();

        return first != null
            ? Path.GetFileNameWithoutExtension(first)
            : new DirectoryInfo(_options.ProjectPath).Name;
    }
}