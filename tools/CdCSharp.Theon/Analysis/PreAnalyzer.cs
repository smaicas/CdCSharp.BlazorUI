using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using System.Text.Json;
using System.Xml.Linq;

namespace CdCSharp.Theon.Analysis;

public class PreAnalyzer
{
    private readonly TypeDestructurer _destructurer;
    private readonly LlmFormatter _formatter;
    private readonly TheonLogger _logger;
    private readonly TheonOptions _options;
    private readonly IgnoreFilter _ignoreFilter;

    private static readonly string[] TestIndicators = ["xunit", "nunit", "mstest", "Test.Sdk"];

    public PreAnalyzer(
        TypeDestructurer destructurer,
        LlmFormatter formatter,
        TheonLogger logger,
        TheonOptions options,
        IgnoreFilter ignoreFilter)
    {
        _destructurer = destructurer;
        _formatter = formatter;
        _logger = logger;
        _options = options;
        _ignoreFilter = ignoreFilter;
    }

    public async Task<PreAnalysisResult> AnalyzeAsync(string projectPath)
    {
        _logger.Info("Starting pre-analysis...");

        string preanalysisPath = Path.Combine(_options.OutputPath, "preanalysis");
        Directory.CreateDirectory(preanalysisPath);

        // Scan project structure
        ProjectStructure structure = await ScanProjectAsync(projectPath);
        _logger.Info($"Scanned {structure.Assemblies.Count} assemblies");

        // Save project structure
        string structureJson = JsonSerializer.Serialize(structure, new JsonSerializerOptions { WriteIndented = true });
        string structureJsonPath = Path.Combine(preanalysisPath, "structure.json");
        await File.WriteAllTextAsync(structureJsonPath, structureJson);
        _logger.Debug($"Saved: {structureJsonPath}");

        Dictionary<string, AssemblyAnalysis> assemblyAnalyses = [];

        // Process each assembly
        foreach (AssemblyStructure assembly in structure.Assemblies.Where(a => !a.IsTestProject))
        {
            _logger.Info($"Destructuring: {assembly.Name}");

            List<NamespaceInfo> namespaces = await _destructurer.DestructureAsync(
                projectPath, assembly.Files.CSharp);

            AssemblyStructure detailedAssembly = assembly with { Namespaces = namespaces };

            string assemblyJson = JsonSerializer.Serialize(detailedAssembly, new JsonSerializerOptions { WriteIndented = true });
            string assemblyJsonPath = Path.Combine(preanalysisPath, $"{assembly.Name}.json");
            await File.WriteAllTextAsync(assemblyJsonPath, assemblyJson);

            string llmFormat = _formatter.FormatAssemblyDetail(detailedAssembly);
            string llmPath = Path.Combine(preanalysisPath, $"{assembly.Name}.llm.txt");
            await File.WriteAllTextAsync(llmPath, llmFormat);

            assemblyAnalyses[assembly.Name] = new AssemblyAnalysis
            {
                Assembly = detailedAssembly,
                JsonPath = assemblyJsonPath,
                LlmPath = llmPath,
                LlmContent = llmFormat
            };

            _logger.Debug($"  Types: {namespaces.Sum(n => n.Types.Count)}");
        }

        // Generate project-level LLM format
        string projectLlmFormat = _formatter.FormatProjectStructure(structure);
        string projectLlmPath = Path.Combine(preanalysisPath, "project.llm.txt");
        await File.WriteAllTextAsync(projectLlmPath, projectLlmFormat);

        _logger.Info($"Pre-analysis complete. Output: {preanalysisPath}");

        return new PreAnalysisResult
        {
            Structure = structure,
            AssemblyAnalyses = assemblyAnalyses,
            ProjectLlmFormat = projectLlmFormat,
            OutputPath = preanalysisPath
        };
    }

    private async Task<ProjectStructure> ScanProjectAsync(string projectPath)
    {
        await _ignoreFilter.InitializeAsync(projectPath);
        _logger.Info($"Scanning project: {projectPath}");

        string[] csprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories)
            .Where(f => !_ignoreFilter.IsIgnored(f))
            .ToArray();

        _logger.Info($"Found {csprojFiles.Length} projects");

        List<AssemblyStructure> assemblies = [];
        foreach (string csproj in csprojFiles)
        {
            AssemblyStructure assembly = await ScanCsprojAsync(csproj, projectPath);
            assemblies.Add(assembly);
            _logger.Debug($"  {assembly.Name}: {assembly.Namespaces.Sum(n => n.Types.Count)} types");
        }

        string solutionName = FindSolutionName(projectPath);

        return new ProjectStructure
        {
            Solution = solutionName,
            RootPath = projectPath,
            Assemblies = assemblies,
            Summary = BuildSummary(assemblies)
        };
    }

    private async Task<AssemblyStructure> ScanCsprojAsync(string csprojPath, string rootPath)
    {
        string projectDir = Path.GetDirectoryName(csprojPath)!;
        string projectName = Path.GetFileNameWithoutExtension(csprojPath);
        string relativePath = Path.GetRelativePath(rootPath, projectDir);

        XDocument csproj = XDocument.Load(csprojPath);
        List<string> references = ExtractReferences(csproj);
        bool isTest = IsTestProject(references);

        FileCollection files = ScanFiles(projectDir, rootPath);

        return new AssemblyStructure
        {
            Name = projectName,
            Path = relativePath,
            IsTestProject = isTest,
            References = references,
            Namespaces = [],
            Files = files
        };
    }

    private FileCollection ScanFiles(string projectDir, string rootPath)
    {
        FileCollection files = new()
        {
            CSharp = [],
            Razor = [],
            TypeScript = [],
            Css = [],
            Other = []
        };

        if (!Directory.Exists(projectDir)) return files;

        foreach (string file in Directory.GetFiles(projectDir, "*.*", SearchOption.AllDirectories))
        {
            if (_ignoreFilter.IsIgnored(file)) continue;

            string relative = Path.GetRelativePath(rootPath, file);
            string ext = Path.GetExtension(file).ToLowerInvariant();

            switch (ext)
            {
                case ".cs": files.CSharp.Add(relative); break;
                case ".razor": files.Razor.Add(relative); break;
                case ".ts" or ".tsx": files.TypeScript.Add(relative); break;
                case ".css" or ".scss": files.Css.Add(relative); break;
                case ".json" or ".html" or ".js": files.Other.Add(relative); break;
            }
        }

        return files;
    }

    private static List<string> ExtractReferences(XDocument csproj)
    {
        List<string> refs = [];

        foreach (XElement pkg in csproj.Descendants().Where(e => e.Name.LocalName == "PackageReference"))
        {
            string? include = pkg.Attribute("Include")?.Value;
            if (!string.IsNullOrEmpty(include)) refs.Add(include);
        }

        foreach (XElement proj in csproj.Descendants().Where(e => e.Name.LocalName == "ProjectReference"))
        {
            string? include = proj.Attribute("Include")?.Value;
            if (!string.IsNullOrEmpty(include))
                refs.Add($"[Project] {Path.GetFileNameWithoutExtension(include)}");
        }

        return refs;
    }

    private static bool IsTestProject(List<string> references)
    {
        return references.Any(r => TestIndicators.Any(t =>
            r.Contains(t, StringComparison.OrdinalIgnoreCase)));
    }

    private static string FindSolutionName(string projectPath)
    {
        string[] slnFiles = Directory.GetFiles(projectPath, "*.sln", SearchOption.TopDirectoryOnly);
        return slnFiles.Length > 0
            ? Path.GetFileNameWithoutExtension(slnFiles[0])
            : new DirectoryInfo(projectPath).Name;
    }

    private static ProjectSummary BuildSummary(List<AssemblyStructure> assemblies)
    {
        List<string> patterns = [];
        if (assemblies.Any(a => a.Files.Razor.Count > 0)) patterns.Add("Blazor");
        if (assemblies.Any(a => a.Files.TypeScript.Count > 0)) patterns.Add("TypeScript");
        if (assemblies.Any(a => a.References.Any(r => r.Contains("EntityFramework")))) patterns.Add("EF");

        return new ProjectSummary
        {
            TotalAssemblies = assemblies.Count,
            TotalTypes = assemblies.Sum(a => a.Namespaces.Sum(n => n.Types.Count)),
            TotalFiles = assemblies.Sum(a =>
                a.Files.CSharp.Count + a.Files.Razor.Count +
                a.Files.TypeScript.Count + a.Files.Css.Count),
            DetectedPatterns = patterns
        };
    }
}