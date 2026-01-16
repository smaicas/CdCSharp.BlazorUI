using CdCSharp.DocGen.Core.Abstractions.Analysis;
using CdCSharp.DocGen.Core.Models.Analysis;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CdCSharp.DocGen.Core.Analysis;

public partial class CssAnalyzer : ICssAnalyzer
{
    private readonly ILogger<CssAnalyzer> _logger;

    public CssAnalyzer(ILogger<CssAnalyzer> logger)
    {
        _logger = logger;
    }

    public async Task<List<DestructuredCss>> AnalyzeAsync(string rootPath, List<string> cssFiles)
    {
        List<DestructuredCss> results = [];

        foreach (string relativePath in cssFiles)
        {
            string fullPath = Path.Combine(rootPath, relativePath);
            if (!File.Exists(fullPath))
                continue;

            try
            {
                string content = await File.ReadAllTextAsync(fullPath);
                DestructuredCss css = AnalyzeFile(relativePath, content);
                results.Add(css);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze CSS {RelativePath}", relativePath);
            }
        }

        return results;
    }

    private static DestructuredCss AnalyzeFile(string filePath, string content)
    {
        string ext = Path.GetExtension(filePath).ToLowerInvariant();
        CssFileType fileType = ext switch
        {
            ".scss" => CssFileType.Scss,
            ".less" => CssFileType.Less,
            _ => CssFileType.Css
        };

        return new DestructuredCss
        {
            File = filePath,
            Type = fileType,
            Variables = ExtractVariables(content),
            Selectors = ExtractSelectors(content),
            Imports = ExtractImports(content)
        };
    }

    private static List<CssVariable> ExtractVariables(string content)
    {
        List<CssVariable> variables = [];

        MatchCollection scopeMatches = ScopeSelectorRegex().Matches(content);
        List<(int Start, int End, string Scope)> scopes = [];

        foreach (Match match in scopeMatches)
        {
            string selector = match.Groups[1].Value.Trim();
            int start = match.Index;

            int braceCount = 0;
            int end = start;
            bool foundFirst = false;

            for (int i = match.Index + match.Length; i < content.Length; i++)
            {
                if (content[i] == '{')
                {
                    braceCount++;
                    foundFirst = true;
                }
                else if (content[i] == '}')
                {
                    braceCount--;
                    if (foundFirst && braceCount == 0)
                    {
                        end = i;
                        break;
                    }
                }
            }

            scopes.Add((start, end, selector));
        }

        foreach (Match match in CssVariableRegex().Matches(content))
        {
            string varName = match.Groups[1].Value;
            string varValue = match.Groups[2].Value.Trim().TrimEnd(';');
            int position = match.Index;

            string scope = ":root";
            foreach ((int Start, int End, string Scope) s in scopes)
            {
                if (position >= s.Start && position <= s.End)
                {
                    scope = s.Scope;
                    break;
                }
            }

            variables.Add(new CssVariable
            {
                Name = varName,
                Value = varValue,
                Scope = scope
            });
        }

        foreach (Match match in ScssVariableRegex().Matches(content))
        {
            variables.Add(new CssVariable
            {
                Name = match.Groups[1].Value,
                Value = match.Groups[2].Value.Trim().TrimEnd(';'),
                Scope = "global"
            });
        }

        return variables;
    }

    private static List<string> ExtractSelectors(string content)
    {
        List<string> selectors = [];
        HashSet<string> seen = [];

        foreach (Match match in SelectorRegex().Matches(content))
        {
            string selector = match.Groups[1].Value.Trim();

            if (string.IsNullOrWhiteSpace(selector))
                continue;

            if (selector.StartsWith('@') || selector.StartsWith("//") || selector.StartsWith("/*"))
                continue;

            if (selector.Contains("--"))
                continue;

            string[] parts = selector.Split(',');
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed) && seen.Add(trimmed))
                {
                    selectors.Add(trimmed);
                }
            }
        }

        return selectors.Take(100).ToList();
    }

    private static List<string> ExtractImports(string content)
    {
        List<string> imports = [];

        foreach (Match match in CssImportRegex().Matches(content))
        {
            imports.Add(match.Groups[1].Value);
        }

        foreach (Match match in ScssImportRegex().Matches(content))
        {
            imports.Add(match.Groups[1].Value);
        }

        foreach (Match match in ScssUseRegex().Matches(content))
        {
            imports.Add($"@use {match.Groups[1].Value}");
        }

        return imports;
    }

    [GeneratedRegex(@"(--[\w-]+)\s*:\s*([^;]+);?", RegexOptions.Compiled)]
    private static partial Regex CssVariableRegex();

    [GeneratedRegex(@"\$([\w-]+)\s*:\s*([^;]+);", RegexOptions.Compiled)]
    private static partial Regex ScssVariableRegex();

    [GeneratedRegex(@"([^{}]+)\s*\{", RegexOptions.Compiled)]
    private static partial Regex SelectorRegex();

    [GeneratedRegex(@"([^{]+)\s*\{", RegexOptions.Compiled)]
    private static partial Regex ScopeSelectorRegex();

    [GeneratedRegex(@"@import\s+['""]([^'""]+)['""]", RegexOptions.Compiled)]
    private static partial Regex CssImportRegex();

    [GeneratedRegex(@"@import\s+['""]?([^'"";\s]+)['""]?", RegexOptions.Compiled)]
    private static partial Regex ScssImportRegex();

    [GeneratedRegex(@"@use\s+['""]([^'""]+)['""]", RegexOptions.Compiled)]
    private static partial Regex ScssUseRegex();
}