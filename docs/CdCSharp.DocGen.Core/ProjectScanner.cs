using CdCSharp.DocGen.Core.Analysis;
using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;
using CdCSharp.DocGen.Core.Scanning;

namespace CdCSharp.DocGen.Core;

public class ProjectScanner
{
    private readonly List<IFileScanner> _scanners =
    [
        new CSharpScanner(),
        new RazorScanner(),
        new TypeScriptScanner(),
        new CssScanner()
    ];

    private readonly PublicApiAnalyzer _apiAnalyzer;
    private readonly ComponentAnalyzer _componentAnalyzer;
    private readonly PatternDetector _patternDetector;
    private readonly ILogger _logger;

    public ProjectScanner(ILogger? logger = null)
    {
        _logger = logger ?? new NullLogger();
        _apiAnalyzer = new PublicApiAnalyzer(_logger);
        _componentAnalyzer = new ComponentAnalyzer(_logger);
        _patternDetector = new PatternDetector(_logger);
    }

    public async Task<(ProjectInfo Project, List<ComponentInfo> Components)> ScanAsync(
        string projectPath,
        bool useGitignore = true,
        string? customIgnoreFile = null)
    {
        if (!Directory.Exists(projectPath))
            throw new DirectoryNotFoundException($"Project directory not found: {projectPath}");

        string projectName = new DirectoryInfo(projectPath).Name;

        _logger.Info($"Scanning: {projectPath}");

        // Cargar patrones de ignore
        (GitignorePatternMatcher matcher, string ignoreSource) =
            await IgnoreFileLoader.LoadAsync(projectPath, customIgnoreFile, useGitignore, _logger);

        _logger.Info($"Using ignore patterns: {ignoreSource}");

        // Escanear archivos
        _logger.Verbose("Discovering files...");
        List<Models.FileInfo> files = [];
        string[] allFiles = Directory.GetFiles(projectPath, "*.*", SearchOption.AllDirectories);
        int ignored = 0;
        int scanned = 0;
        int unsupported = 0;

        foreach (string filePath in allFiles)
        {
            if (matcher.IsIgnored(filePath))
            {
                ignored++;
                continue;
            }

            IFileScanner? scanner = _scanners.FirstOrDefault(s => s.CanScan(filePath));
            if (scanner != null)
            {
                try
                {
                    Models.FileInfo metadata = await scanner.ScanAsync(filePath, projectPath);
                    files.Add(metadata);
                    scanned++;

                    if (scanned % 50 == 0)
                        _logger.Verbose($"  Scanned {scanned} files...");
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to scan {Path.GetFileName(filePath)}: {ex.Message}");
                }
            }
            else
            {
                unsupported++;
            }
        }

        _logger.Info($"Scanned {files.Count} files ({ignored} ignored, {unsupported} unsupported)");

        // Analizar API pública
        _logger.Verbose("Analyzing public API...");
        List<Models.FileInfo> csFiles = files.Where(f => f.Type == FileType.CSharp).ToList();
        List<TypeInfo> publicTypes = await _apiAnalyzer.AnalyzeAsync(projectPath, csFiles);

        if (publicTypes.Count > 0)
            _logger.Info($"Found {publicTypes.Count} public types");

        // Analizar componentes Blazor
        _logger.Verbose("Analyzing Blazor components...");
        List<Models.FileInfo> razorFiles = files.Where(f => f.Type == FileType.Razor).ToList();
        List<ComponentInfo> components = await _componentAnalyzer.AnalyzeAsync(projectPath, razorFiles);

        if (components.Count > 0)
            _logger.Info($"Found {components.Count} Blazor components");

        // Construir ProjectInfo
        ProjectInfo project = new()
        {
            Name = projectName,
            RootPath = projectPath,
            Type = DetermineProjectType(files, publicTypes),
            Files = files,
            PublicTypes = publicTypes
        };

        // Detectar patrones arquitectónicos
        _logger.Verbose("Detecting architectural patterns...");
        project.Patterns.AddRange(_patternDetector.Detect(project));

        if (project.Patterns.Count > 0)
        {
            _logger.Info($"Detected patterns: {string.Join(", ", project.Patterns.Select(p => p.Name))}");
        }

        // Estadísticas finales
        int totalLines = files.Sum(f => f.LineCount);
        int totalTokens = files.Sum(f => f.TokenEstimate);

        _logger.Verbose($"Total lines: {totalLines:N0}");
        _logger.Verbose($"Estimated tokens: {totalTokens:N0}");

        return (project, components);
    }

    private ProjectType DetermineProjectType(List<Models.FileInfo> files, List<TypeInfo> types)
    {
        ProjectType type;

        if (files.Any(f => f.Type == FileType.Razor))
            type = ProjectType.BlazorComponent;
        else if (types.Any(t => t.BaseTypes.Any(b => b.Contains("ControllerBase"))))
            type = ProjectType.WebApi;
        else if (types.Any(t => t.PublicMembers.Any(m => m.Name == "Main")))
            type = ProjectType.Console;
        else
            type = ProjectType.ClassLibrary;

        _logger.Verbose($"Detected project type: {type}");
        return type;
    }
}