using CdCSharp.DocGen.Core.Abstractions.Analysis;
using CdCSharp.DocGen.Core.Abstractions.Cache;
using CdCSharp.DocGen.Core.Models.Analysis;
using CdCSharp.DocGen.Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CdCSharp.DocGen.Core.Analysis;

public class ProjectAnalyzer : IProjectAnalyzer
{
    private readonly IAssemblyScanner _assemblyScanner;
    private readonly ITypeDestructurer _typeDestructurer;
    private readonly IComponentAnalyzer _componentAnalyzer;
    private readonly ITypeScriptAnalyzer _tsAnalyzer;
    private readonly ICssAnalyzer _cssAnalyzer;
    private readonly ICacheManager _cache;
    private readonly ILogger<ProjectAnalyzer> _logger;
    private readonly DocGenOptions _options;

    public ProjectAnalyzer(
        IAssemblyScanner assemblyScanner,
        ITypeDestructurer typeDestructurer,
        IComponentAnalyzer componentAnalyzer,
        ITypeScriptAnalyzer tsAnalyzer,
        ICssAnalyzer cssAnalyzer,
        ICacheManager cache,
        IOptions<DocGenOptions> options,
        ILogger<ProjectAnalyzer> logger)
    {
        _assemblyScanner = assemblyScanner;
        _typeDestructurer = typeDestructurer;
        _componentAnalyzer = componentAnalyzer;
        _tsAnalyzer = tsAnalyzer;
        _cssAnalyzer = cssAnalyzer;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AnalysisResult> AnalyzeAsync(string projectPath)
    {
        _logger.LogInformation("Analyzing project: {ProjectPath}", projectPath);

        List<AssemblyInfo> assemblies = await _assemblyScanner.ScanAsync(projectPath);
        Dictionary<string, DestructuredAssembly> destructured = [];

        foreach (AssemblyInfo assembly in assemblies)
        {
            _logger.LogInformation("Destructuring: {AssemblyName}", assembly.Name);

            DestructuredAssembly da = await DestructureAssemblyAsync(projectPath, assembly);
            destructured[assembly.Name] = da;

            assembly.Metrics.Classes = da.Namespaces.Sum(n => n.Types.Count(t => t.Kind == TypeKind.Class));
            assembly.Metrics.Interfaces = da.Namespaces.Sum(n => n.Types.Count(t => t.Kind == TypeKind.Interface));
            assembly.Metrics.Records = da.Namespaces.Sum(n => n.Types.Count(t => t.Kind == TypeKind.Record));
            assembly.Metrics.Structs = da.Namespaces.Sum(n => n.Types.Count(t => t.Kind == TypeKind.Struct));
            assembly.Metrics.Enums = da.Namespaces.Sum(n => n.Types.Count(t => t.Kind == TypeKind.Enum));
            assembly.Metrics.Delegates = da.Namespaces.Sum(n => n.Types.Count(t => t.Kind == TypeKind.Delegate));
            assembly.Metrics.Components = da.Components.Count;
            assembly.Metrics.Generators = CountGenerators(da);
            assembly.Metrics.TsModules = da.TypeScript.Count;
            assembly.Metrics.CssFiles = da.Css.Count;
        }

        ProjectStructure structure = BuildStructure(projectPath, assemblies);

        _logger.LogInformation("Analysis complete: {Count} assemblies", assemblies.Count);

        return new AnalysisResult
        {
            Structure = structure,
            Destructured = destructured
        };
    }

    private async Task<DestructuredAssembly> DestructureAssemblyAsync(string rootPath, AssemblyInfo assembly)
    {
        string assemblyPath = Path.Combine(rootPath, assembly.Path);

        (bool hit, DestructuredAssembly? cached) = await _cache.TryGetAnalysisAsync<DestructuredAssembly>(
            assemblyPath, "destructured");

        if (hit && cached != null)
            return cached;

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

        await _cache.SetAnalysisAsync(assemblyPath, "destructured", result);

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

        ProjectSummary summary = new()
        {
            TotalAssemblies = assemblies.Count,
            TotalTestProjects = assemblies.Count(a => a.IsTestProject),
            TotalClasses = assemblies.Sum(a => a.Metrics.Classes),
            TotalInterfaces = assemblies.Sum(a => a.Metrics.Interfaces),
            TotalRecords = assemblies.Sum(a => a.Metrics.Records),
            TotalComponents = assemblies.Sum(a => a.Metrics.Components),
            TotalGenerators = assemblies.Sum(a => a.Metrics.Generators),
            DetectedPatterns = DetectPatterns(assemblies),
            ProjectType = DetermineProjectType(assemblies)
        };

        return new ProjectStructure
        {
            Solution = solutionName,
            RootPath = projectPath,
            Assemblies = assemblies,
            Summary = summary
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

        if (assemblies.Any(a => a.Metrics.Generators > 0))
            patterns.Add("SourceGenerators");

        if (assemblies.Any(a => a.Metrics.Components > 0))
            patterns.Add("Blazor");

        if (assemblies.Any(a => a.References.Any(r => r.Contains("Microsoft.AspNetCore"))))
            patterns.Add("ASP.NET Core");

        if (assemblies.Any(a => a.References.Any(r => r.Contains("EntityFramework"))))
            patterns.Add("Entity Framework");

        if (assemblies.Any(a => a.References.Any(r => r.Contains("MediatR"))))
            patterns.Add("MediatR/CQRS");

        if (assemblies.Any(a => a.References.Any(r => r.Contains("FluentValidation"))))
            patterns.Add("FluentValidation");

        if (assemblies.Any(a => a.Metrics.TsModules > 0))
            patterns.Add("TypeScript");

        return patterns;
    }

    private static string DetermineProjectType(List<AssemblyInfo> assemblies)
    {
        List<AssemblyInfo> nonTest = assemblies.Where(a => !a.IsTestProject).ToList();

        if (nonTest.Any(a => a.Metrics.Components > 0))
        {
            if (nonTest.Any(a => a.References.Any(r => r.Contains("Microsoft.AspNetCore.Components.WebAssembly"))))
                return "Blazor WebAssembly";
            if (nonTest.Any(a => a.References.Any(r => r.Contains("Microsoft.AspNetCore"))))
                return "Blazor Server";
            return "Blazor Component Library";
        }

        if (nonTest.Any(a => a.References.Any(r => r.Contains("Microsoft.AspNetCore.Mvc") || r.Contains("ControllerBase"))))
            return "ASP.NET Core Web API";

        if (nonTest.Any(a => a.Metrics.Generators > 0))
            return "Source Generator Library";

        if (nonTest.Count == 1 && nonTest[0].Files.CSharp.Any(f => f.Contains("Program.cs")))
            return "Console Application";

        return "Class Library";
    }
}