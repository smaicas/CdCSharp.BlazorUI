using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;
using System.Xml.Linq;

namespace CdCSharp.DocGen.Core.Analysis;

public class AssemblyScanner
{
    private readonly ILogger _logger;
    private readonly List<string> _testPackageIndicators;
    private IgnoreFilter? _ignoreFilter;

    public AssemblyScanner(ILogger? logger = null, List<string>? testPackageIndicators = null)
    {
        _logger = logger ?? NullLogger.Instance;
        _testPackageIndicators = testPackageIndicators ?? ["xunit", "nunit", "mstest", "xUnit", "NUnit", "MSTest"];
    }

    public async Task<List<AssemblyInfo>> ScanAsync(string projectPath)
    {
        _ignoreFilter = await IgnoreFilter.LoadAsync(projectPath, _logger);
        _logger.Verbose($"Using ignore patterns: {_ignoreFilter.Source}");

        List<AssemblyInfo> assemblies = [];
        string[] csprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories)
            .Where(f => !_ignoreFilter.IsIgnored(f))
            .ToArray();

        _logger.Progress($"Found {csprojFiles.Length} projects");

        foreach (string csprojPath in csprojFiles)
        {
            try
            {
                AssemblyInfo assembly = await ScanProjectAsync(csprojPath, projectPath);
                assemblies.Add(assembly);
                _logger.Verbose($"  Scanned: {assembly.Name}");
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to scan {Path.GetFileName(csprojPath)}: {ex.Message}");
            }
        }

        return assemblies;
    }

    private async Task<AssemblyInfo> ScanProjectAsync(string csprojPath, string rootPath)
    {
        string projectDir = Path.GetDirectoryName(csprojPath)!;
        string projectName = Path.GetFileNameWithoutExtension(csprojPath);
        string relativePath = Path.GetRelativePath(rootPath, projectDir);

        XDocument csproj = XDocument.Load(csprojPath);
        List<string> references = ExtractReferences(csproj);
        bool isTestProject = IsTestProject(csproj, references);

        AssemblyFiles files = await ScanFilesAsync(projectDir, rootPath);

        return new AssemblyInfo
        {
            Name = projectName,
            Path = relativePath,
            IsTestProject = isTestProject,
            References = references,
            Files = files,
            Summary = new AssemblySummary
            {
                CssFiles = files.Css.Count,
                TsModules = files.TypeScript.Count
            }
        };
    }

    private List<string> ExtractReferences(XDocument csproj)
    {
        List<string> references = [];

        IEnumerable<XElement> packageRefs = csproj.Descendants()
            .Where(e => e.Name.LocalName == "PackageReference");

        foreach (XElement pkg in packageRefs)
        {
            string? include = pkg.Attribute("Include")?.Value;
            if (!string.IsNullOrEmpty(include))
                references.Add(include);
        }

        IEnumerable<XElement> projectRefs = csproj.Descendants()
            .Where(e => e.Name.LocalName == "ProjectReference");

        foreach (XElement proj in projectRefs)
        {
            string? include = proj.Attribute("Include")?.Value;
            if (!string.IsNullOrEmpty(include))
            {
                string refName = Path.GetFileNameWithoutExtension(include);
                references.Add($"[Project] {refName}");
            }
        }

        return references;
    }

    private bool IsTestProject(XDocument csproj, List<string> references)
    {
        bool hasTestSdk = csproj.Descendants()
            .Any(e => e.Name.LocalName == "PackageReference" &&
                     e.Attribute("Include")?.Value?.Contains("Test.Sdk") == true);

        if (hasTestSdk)
            return true;

        return references.Any(r =>
            _testPackageIndicators.Any(indicator =>
                r.Contains(indicator, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task<AssemblyFiles> ScanFilesAsync(string projectDir, string rootPath)
    {
        AssemblyFiles files = new()
        {
            CSharp = [],
            Razor = [],
            TypeScript = [],
            Css = [],
            Other = []
        };

        if (!Directory.Exists(projectDir))
            return files;

        string[] allFiles = Directory.GetFiles(projectDir, "*.*", SearchOption.AllDirectories);

        foreach (string file in allFiles)
        {
            if (_ignoreFilter?.IsIgnored(file) == true)
                continue;

            string relativePath = Path.GetRelativePath(rootPath, file);
            string ext = Path.GetExtension(file).ToLowerInvariant();

            switch (ext)
            {
                case ".cs":
                    files.CSharp.Add(relativePath);
                    break;
                case ".razor":
                    files.Razor.Add(relativePath);
                    break;
                case ".ts" or ".tsx":
                    files.TypeScript.Add(relativePath);
                    break;
                case ".css" or ".scss" or ".less":
                    files.Css.Add(relativePath);
                    break;
                case ".js" or ".jsx" or ".json" or ".html":
                    files.Other.Add(relativePath);
                    break;
            }
        }

        await Task.CompletedTask;
        return files;
    }
}