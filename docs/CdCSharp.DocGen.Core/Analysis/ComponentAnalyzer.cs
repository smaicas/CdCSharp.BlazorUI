using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;
using System.Text.RegularExpressions;

namespace CdCSharp.DocGen.Core.Analysis;

public partial class ComponentAnalyzer
{
    private readonly ILogger _logger;

    public ComponentAnalyzer(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    public async Task<List<DestructuredComponent>> AnalyzeAsync(string rootPath, List<string> razorFiles)
    {
        List<DestructuredComponent> components = [];

        foreach (string relativePath in razorFiles)
        {
            string fullPath = Path.Combine(rootPath, relativePath);
            if (!File.Exists(fullPath))
                continue;

            try
            {
                string content = await File.ReadAllTextAsync(fullPath);
                DestructuredComponent component = AnalyzeComponent(relativePath, content, rootPath);
                components.Add(component);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to analyze component {relativePath}: {ex.Message}");
            }
        }

        return components;
    }

    private DestructuredComponent AnalyzeComponent(string filePath, string content, string rootPath)
    {
        string name = Path.GetFileNameWithoutExtension(filePath);
        string codeBehindPath = Path.Combine(rootPath, filePath + ".cs");
        bool hasCodeBehind = File.Exists(codeBehindPath);

        string fullContent = content;
        if (hasCodeBehind)
        {
            try
            {
                fullContent += "\n" + File.ReadAllText(codeBehindPath);
            }
            catch { }
        }

        return new DestructuredComponent
        {
            Name = name,
            File = filePath,
            HasCodeBehind = hasCodeBehind,
            Inherits = ExtractInherits(content),
            Implements = ExtractImplements(fullContent),
            Parameters = ExtractParameters(fullContent),
            CascadingParameters = ExtractCascadingParameters(fullContent),
            Injectables = ExtractInjectables(fullContent),
            EventCallbacks = ExtractEventCallbacks(fullContent),
            RenderFragments = ExtractRenderFragments(fullContent)
        };
    }

    private static string? ExtractInherits(string content)
    {
        Match match = InheritsRegex().Match(content);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static List<string> ExtractImplements(string content)
    {
        List<string> implements = [];
        MatchCollection matches = ImplementsRegex().Matches(content);
        foreach (Match match in matches)
        {
            implements.Add(match.Groups[1].Value.Trim());
        }
        return implements;
    }

    private static List<ComponentParameter> ExtractParameters(string content)
    {
        List<ComponentParameter> parameters = [];
        MatchCollection matches = ParameterRegex().Matches(content);

        foreach (Match match in matches)
        {
            bool isRequired = match.Groups[1].Success && match.Groups[1].Value.Contains("required");
            bool editorRequired = match.Groups[1].Success && match.Groups[1].Value.Contains("EditorRequired");
            string type = match.Groups[2].Value;
            string name = match.Groups[3].Value;
            string? defaultValue = match.Groups[4].Success ? match.Groups[4].Value : null;

            parameters.Add(new ComponentParameter
            {
                Name = name,
                Type = type,
                Required = isRequired || type.EndsWith("?") == false,
                EditorRequired = editorRequired,
                DefaultValue = defaultValue
            });
        }

        return parameters;
    }

    private static List<ComponentParameter> ExtractCascadingParameters(string content)
    {
        List<ComponentParameter> parameters = [];
        MatchCollection matches = CascadingParameterRegex().Matches(content);

        foreach (Match match in matches)
        {
            parameters.Add(new ComponentParameter
            {
                Name = match.Groups[2].Value,
                Type = match.Groups[1].Value,
                Required = false,
                EditorRequired = false
            });
        }

        return parameters;
    }

    private static List<InjectableService> ExtractInjectables(string content)
    {
        List<InjectableService> injectables = [];

        MatchCollection razorMatches = InjectRazorRegex().Matches(content);
        foreach (Match match in razorMatches)
        {
            injectables.Add(new InjectableService
            {
                Type = match.Groups[1].Value,
                Name = match.Groups[2].Value
            });
        }

        MatchCollection attrMatches = InjectAttributeRegex().Matches(content);
        foreach (Match match in attrMatches)
        {
            injectables.Add(new InjectableService
            {
                Type = match.Groups[1].Value,
                Name = match.Groups[2].Value
            });
        }

        return injectables;
    }

    private static List<string> ExtractEventCallbacks(string content)
    {
        List<string> callbacks = [];
        MatchCollection matches = EventCallbackRegex().Matches(content);

        foreach (Match match in matches)
        {
            string type = match.Groups[1].Value;
            string name = match.Groups[2].Value;
            callbacks.Add($"{name}: {type}");
        }

        return callbacks;
    }

    private static List<string> ExtractRenderFragments(string content)
    {
        List<string> fragments = [];
        MatchCollection matches = RenderFragmentRegex().Matches(content);

        foreach (Match match in matches)
        {
            string type = match.Groups[1].Value;
            string name = match.Groups[2].Value;
            fragments.Add(type.Contains("<") ? $"{name}: {type}" : name);
        }

        return fragments;
    }

    [GeneratedRegex(@"@inherits\s+([\w.<>]+)", RegexOptions.Compiled)]
    private static partial Regex InheritsRegex();

    [GeneratedRegex(@"@implements\s+([\w.<>]+)", RegexOptions.Compiled)]
    private static partial Regex ImplementsRegex();

    [GeneratedRegex(@"\[Parameter\]\s*(?:\[([\w\s,()""]+)\]\s*)*public\s+(?:required\s+)?([\w<>?,\s]+)\s+(\w+)(?:\s*=\s*([^;]+))?", RegexOptions.Compiled)]
    private static partial Regex ParameterRegex();

    [GeneratedRegex(@"\[CascadingParameter\]\s*public\s+([\w<>?,\s]+)\s+(\w+)", RegexOptions.Compiled)]
    private static partial Regex CascadingParameterRegex();

    [GeneratedRegex(@"@inject\s+([\w.<>]+)\s+(\w+)", RegexOptions.Compiled)]
    private static partial Regex InjectRazorRegex();

    [GeneratedRegex(@"\[Inject\]\s*public\s+([\w.<>]+)\s+(\w+)", RegexOptions.Compiled)]
    private static partial Regex InjectAttributeRegex();

    [GeneratedRegex(@"public\s+(EventCallback(?:<[\w<>?,\s]+>)?)\s+(\w+)", RegexOptions.Compiled)]
    private static partial Regex EventCallbackRegex();

    [GeneratedRegex(@"public\s+(RenderFragment(?:<[\w<>?,\s]+>)?)\s+(\w+)", RegexOptions.Compiled)]
    private static partial Regex RenderFragmentRegex();
}