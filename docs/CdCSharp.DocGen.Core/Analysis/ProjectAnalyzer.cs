using CdCSharp.DocGen.Core.Cache;
using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;

namespace CdCSharp.DocGen.Core.Analysis;

public class ProjectAnalyzer
{
    private readonly AssemblyScanner _assemblyScanner;
    private readonly TypeDestructurer _typeDestructurer;
    private readonly ComponentAnalyzer _componentAnalyzer;
    private readonly TypeScriptAnalyzer _tsAnalyzer;
    private readonly CssAnalyzer _cssAnalyzer;
    private readonly CacheManager? _cache;
    private readonly ILogger _logger;

    public ProjectAnalyzer(CacheManager? cache = null, ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
        _cache = cache;
        _assemblyScanner = new AssemblyScanner(_logger);
        _typeDestructurer = new TypeDestructurer(_logger);
        _componentAnalyzer = new ComponentAnalyzer(_logger);
        _tsAnalyzer = new TypeScriptAnalyzer(_logger);
        _cssAnalyzer = new CssAnalyzer(_logger);
    }

    public async Task<(ProjectStructure Structure, Dictionary<string, DestructuredAssembly> Destructured)> AnalyzeAsync(string projectPath)
    {
        _logger.Info($"Analyzing project: {projectPath}");

        List<AssemblyInfo> assemblies = await _assemblyScanner.ScanAsync(projectPath);
        Dictionary<string, DestructuredAssembly> destructured = [];

        foreach (AssemblyInfo assembly in assemblies)
        {
            _logger.Progress($"Destructuring: {assembly.Name}");

            DestructuredAssembly da = await DestructureAssemblyAsync(projectPath, assembly);
            destructured[assembly.Name] = da;

            assembly.Summary.Classes = da.Namespaces.Sum(n => n.Types.Count(t => t.Kind == TypeKind.Class));
            assembly.Summary.Interfaces = da.Namespaces.Sum(n => n.Types.Count(t => t.Kind == TypeKind.Interface));
            assembly.Summary.Records = da.Namespaces.Sum(n => n.Types.Count(t => t.Kind == TypeKind.Record));
            assembly.Summary.Structs = da.Namespaces.Sum(n => n.Types.Count(t => t.Kind == TypeKind.Struct));
            assembly.Summary.Enums = da.Namespaces.Sum(n => n.Types.Count(t => t.Kind == TypeKind.Enum));
            assembly.Summary.Delegates = da.Namespaces.Sum(n => n.Types.Count(t => t.Kind == TypeKind.Delegate));
            assembly.Summary.Components = da.Components.Count;
            assembly.Summary.Generators = CountGenerators(da);
            assembly.Summary.TsModules = da.TypeScript.Count;
            assembly.Summary.CssFiles = da.Css.Count;
            assembly.Summary.CssVariables = da.Css.Sum(c => c.Variables.Count);
        }

        ProjectStructure structure = BuildStructure(projectPath, assemblies);

        _logger.Success($"Analysis complete: {assemblies.Count} assemblies");

        return (structure, destructured);
    }

    private async Task<DestructuredAssembly> DestructureAssemblyAsync(string rootPath, AssemblyInfo assembly)
    {
        string cacheKey = $"destructured:{assembly.Name}";

        if (_cache != null)
        {
            (bool hit, DestructuredAssembly? cached) = await _cache.TryGetAnalysisAsync<DestructuredAssembly>(
                Path.Combine(rootPath, assembly.Path), "destructured");

            if (hit && cached != null)
                return cached;
        }

        List<DestructuredNamespace> namespaces = await _typeDestructurer.DestructureAsync(rootPath, assembly.Files.CSharp);
        List<DestructuredComponent> components = await _componentAnalyzer.AnalyzeAsync(rootPath, assembly.Files.Razor);
        List<DestructuredTypeScript> typescript = await _tsAnalyzer.AnalyzeAsync(rootPath, assembly.Files.TypeScript);
        List<DestructuredCss> css = await _cssAnalyzer.AnalyzeAsync(rootPath, assembly.Files.Css);

        DestructuredAssembly result = new()
        {
            Assembly = assembly.Name,
            Namespaces = namespaces,
            Components = components,
            TypeScript = typescript,
            Css = css
        };

        if (_cache != null)
        {
            await _cache.SetAnalysisAsync(Path.Combine(rootPath, assembly.Path), "destructured", result);
        }

        return result;
    }

    private static int CountGenerators(DestructuredAssembly assembly)
    {
        return assembly.Namespaces
            .SelectMany(n => n.Types)
            .Count(t => t.Attributes.Any(a => a.Contains("Generator")));
    }

    private ProjectStructure BuildStructure(string projectPath, List<AssemblyInfo> assemblies)
    {
        string solutionName = FindSolutionName(projectPath);

        GlobalSummary globalSummary = new()
        {
            TotalAssemblies = assemblies.Count,
            TotalTestProjects = assemblies.Count(a => a.IsTestProject),
            TotalClasses = assemblies.Sum(a => a.Summary.Classes),
            TotalInterfaces = assemblies.Sum(a => a.Summary.Interfaces),
            TotalRecords = assemblies.Sum(a => a.Summary.Records),
            TotalComponents = assemblies.Sum(a => a.Summary.Components),
            TotalGenerators = assemblies.Sum(a => a.Summary.Generators),
            TotalTsModules = assemblies.Sum(a => a.Summary.TsModules),
            TotalCssFiles = assemblies.Sum(a => a.Summary.CssFiles),
            DetectedPatterns = DetectPatterns(assemblies),
            ProjectType = DetermineProjectType(assemblies)
        };

        return new ProjectStructure
        {
            Solution = solutionName,
            RootPath = projectPath,
            Assemblies = assemblies,
            GlobalSummary = globalSummary
        };
    }

    private static string FindSolutionName(string projectPath)
    {
        string[] slnFiles = Directory.GetFiles(projectPath, "*.sln", SearchOption.TopDirectoryOnly);
        if (slnFiles.Length > 0)
            return Path.GetFileNameWithoutExtension(slnFiles[0]);

        return new DirectoryInfo(projectPath).Name;
    }

    private static List<string> DetectPatterns(List<AssemblyInfo> assemblies)
    {
        List<string> patterns = [];

        if (assemblies.Any(a => a.Summary.Generators > 0))
            patterns.Add("SourceGenerators");

        if (assemblies.Any(a => a.Summary.Components > 0))
            patterns.Add("Blazor");

        if (assemblies.Any(a => a.References.Any(r => r.Contains("Microsoft.AspNetCore"))))
            patterns.Add("ASP.NET Core");

        if (assemblies.Any(a => a.References.Any(r => r.Contains("EntityFramework"))))
            patterns.Add("Entity Framework");

        if (assemblies.Any(a => a.References.Any(r => r.Contains("MediatR"))))
            patterns.Add("MediatR/CQRS");

        if (assemblies.Any(a => a.References.Any(r => r.Contains("FluentValidation"))))
            patterns.Add("FluentValidation");

        if (assemblies.Any(a => a.Summary.TsModules > 0))
            patterns.Add("TypeScript");

        return patterns;
    }

    private static string DetermineProjectType(List<AssemblyInfo> assemblies)
    {
        List<AssemblyInfo> nonTest = assemblies.Where(a => !a.IsTestProject).ToList();

        if (nonTest.Any(a => a.Summary.Components > 0))
        {
            if (nonTest.Any(a => a.References.Any(r => r.Contains("Microsoft.AspNetCore.Components.WebAssembly"))))
                return "Blazor WebAssembly";
            if (nonTest.Any(a => a.References.Any(r => r.Contains("Microsoft.AspNetCore"))))
                return "Blazor Server";
            return "Blazor Component Library";
        }

        if (nonTest.Any(a => a.References.Any(r => r.Contains("Microsoft.AspNetCore.Mvc") || r.Contains("ControllerBase"))))
            return "ASP.NET Core Web API";

        if (nonTest.Any(a => a.Summary.Generators > 0))
            return "Source Generator Library";

        if (nonTest.Count == 1 && nonTest[0].Files.CSharp.Any(f => f.Contains("Program.cs")))
            return "Console Application";

        return "Class Library";
    }
}