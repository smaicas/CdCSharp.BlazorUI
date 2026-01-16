using CdCSharp.DocGen.Core.Abstractions.Analysis;
using CdCSharp.DocGen.Core.Models.Analysis;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CdCSharp.DocGen.Core.Analysis;

public partial class TypeScriptAnalyzer : ITypeScriptAnalyzer
{
    private readonly ILogger<TypeScriptAnalyzer> _logger;

    public TypeScriptAnalyzer(ILogger<TypeScriptAnalyzer> logger)
    {
        _logger = logger;
    }

    public async Task<List<DestructuredTypeScript>> AnalyzeAsync(string rootPath, List<string> tsFiles)
    {
        List<DestructuredTypeScript> results = [];

        foreach (string relativePath in tsFiles)
        {
            string fullPath = Path.Combine(rootPath, relativePath);
            if (!File.Exists(fullPath))
                continue;

            try
            {
                string content = await File.ReadAllTextAsync(fullPath);
                DestructuredTypeScript ts = AnalyzeFile(relativePath, content);
                results.Add(ts);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze TypeScript {RelativePath}", relativePath);
            }
        }

        return results;
    }

    private static DestructuredTypeScript AnalyzeFile(string filePath, string content)
    {
        return new DestructuredTypeScript
        {
            File = filePath,
            Exports = ExtractExports(content),
            Imports = ExtractImports(content)
        };
    }

    private static List<TsExport> ExtractExports(string content)
    {
        List<TsExport> exports = [];

        foreach (Match match in ExportFunctionRegex().Matches(content))
        {
            bool isDefault = match.Groups[1].Success;
            bool isAsync = match.Groups[2].Success;
            string name = match.Groups[3].Value;
            string parameters = match.Groups[4].Value;
            string returnType = match.Groups[5].Success ? match.Groups[5].Value : "void";

            exports.Add(new TsExport
            {
                Kind = TsExportKind.Function,
                Name = name,
                Signature = $"{(isAsync ? "async " : "")}function {name}({parameters}): {returnType}",
                IsDefault = isDefault
            });
        }

        foreach (Match match in ExportClassRegex().Matches(content))
        {
            exports.Add(new TsExport
            {
                Kind = TsExportKind.Class,
                Name = match.Groups[2].Value,
                IsDefault = match.Groups[1].Success
            });
        }

        foreach (Match match in ExportInterfaceRegex().Matches(content))
        {
            exports.Add(new TsExport
            {
                Kind = TsExportKind.Interface,
                Name = match.Groups[1].Value,
                IsDefault = false
            });
        }

        foreach (Match match in ExportTypeRegex().Matches(content))
        {
            exports.Add(new TsExport
            {
                Kind = TsExportKind.Type,
                Name = match.Groups[1].Value,
                IsDefault = false
            });
        }

        foreach (Match match in ExportConstRegex().Matches(content))
        {
            exports.Add(new TsExport
            {
                Kind = TsExportKind.Const,
                Name = match.Groups[2].Value,
                Signature = match.Groups[3].Success ? match.Groups[3].Value : null,
                IsDefault = match.Groups[1].Success
            });
        }

        foreach (Match match in ExportEnumRegex().Matches(content))
        {
            exports.Add(new TsExport
            {
                Kind = TsExportKind.Enum,
                Name = match.Groups[1].Value,
                IsDefault = false
            });
        }

        return exports;
    }

    private static List<TsImport> ExtractImports(string content)
    {
        List<TsImport> imports = [];

        foreach (Match match in ImportRegex().Matches(content))
        {
            string importPart = match.Groups[1].Value;
            string fromPath = match.Groups[2].Value;

            List<string> names = [];

            if (importPart.Contains('{'))
            {
                Match namedMatch = NamedImportsRegex().Match(importPart);
                if (namedMatch.Success)
                {
                    names.AddRange(namedMatch.Groups[1].Value
                        .Split(',')
                        .Select(n => n.Trim())
                        .Where(n => !string.IsNullOrEmpty(n)));
                }
            }

            Match defaultMatch = DefaultImportRegex().Match(importPart);
            if (defaultMatch.Success)
            {
                names.Insert(0, $"default as {defaultMatch.Groups[1].Value}");
            }

            if (importPart.Contains('*'))
            {
                Match namespaceMatch = NamespaceImportRegex().Match(importPart);
                if (namespaceMatch.Success)
                {
                    names.Add($"* as {namespaceMatch.Groups[1].Value}");
                }
            }

            if (names.Count > 0)
            {
                imports.Add(new TsImport
                {
                    From = fromPath,
                    Names = names
                });
            }
        }

        return imports;
    }

    [GeneratedRegex(@"export\s+(default\s+)?(async\s+)?function\s+(\w+)\s*\(([^)]*)\)(?:\s*:\s*([\w<>\[\]|&\s]+))?", RegexOptions.Compiled)]
    private static partial Regex ExportFunctionRegex();

    [GeneratedRegex(@"export\s+(default\s+)?class\s+(\w+)", RegexOptions.Compiled)]
    private static partial Regex ExportClassRegex();

    [GeneratedRegex(@"export\s+interface\s+(\w+)", RegexOptions.Compiled)]
    private static partial Regex ExportInterfaceRegex();

    [GeneratedRegex(@"export\s+type\s+(\w+)", RegexOptions.Compiled)]
    private static partial Regex ExportTypeRegex();

    [GeneratedRegex(@"export\s+(default\s+)?const\s+(\w+)(?:\s*:\s*([\w<>\[\]|&\s]+))?", RegexOptions.Compiled)]
    private static partial Regex ExportConstRegex();

    [GeneratedRegex(@"export\s+enum\s+(\w+)", RegexOptions.Compiled)]
    private static partial Regex ExportEnumRegex();

    [GeneratedRegex(@"import\s+(.+?)\s+from\s+['""]([^'""]+)['""]", RegexOptions.Compiled)]
    private static partial Regex ImportRegex();

    [GeneratedRegex(@"\{([^}]+)\}", RegexOptions.Compiled)]
    private static partial Regex NamedImportsRegex();

    [GeneratedRegex(@"^(\w+)(?:\s*,|\s*$)", RegexOptions.Compiled)]
    private static partial Regex DefaultImportRegex();

    [GeneratedRegex(@"\*\s+as\s+(\w+)", RegexOptions.Compiled)]
    private static partial Regex NamespaceImportRegex();
}