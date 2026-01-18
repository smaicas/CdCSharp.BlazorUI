// Tools/Exploration/ExploreFilesTool.cs
using CdCSharp.Theon.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CdCSharp.Theon.Tools.Exploration;

public sealed class ExploreFilesTool : ITool
{
    public string Name => "EXPLORE_FILES";
    public string Description => "Explore multiple specific files in one step.";
    public ToolCategory Category => ToolCategory.Exploration;

    public ToolPromptSpec GetPromptSpec() => new(
        Syntax: "[EXPLORE_FILES: paths=\"a.cs,b.cs,c.cs\"]",
        ClosingTag: null,
        Example: "[EXPLORE_FILES: paths=\"Core/LlmClient.cs,Core/Prompts.cs\"]",
        ParameterDescriptions: ["paths: Comma-separated relative paths"]);

    public ToolDefinition GetDefinition()
    {
        string schema = """
        {
            "type": "object",
            "properties": {
                "paths": { 
                    "type": "array", 
                    "items": { "type": "string" },
                    "description": "List of file paths to explore" 
                }
            },
            "required": ["paths"]
        }
        """;

        return new ToolDefinition(Name, Description, JsonDocument.Parse(schema).RootElement);
    }

    public async Task<ToolExecutionResult> ExecuteAsync(
        JsonElement parameters,
        ToolExecutionContext context,
        CancellationToken ct = default)
    {
        List<string> paths = [];

        if (parameters.TryGetProperty("paths", out JsonElement pathsElement))
        {
            if (pathsElement.ValueKind == JsonValueKind.Array)
            {
                paths = pathsElement.EnumerateArray()
                    .Select(e => e.GetString())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Cast<string>()
                    .ToList();
            }
            else if (pathsElement.ValueKind == JsonValueKind.String)
            {
                string? pathsStr = pathsElement.GetString();
                if (!string.IsNullOrWhiteSpace(pathsStr))
                {
                    paths = pathsStr
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList();
                }
            }
        }

        if (paths.Count == 0)
            return ToolExecutionResult.Fail("No valid paths provided");

        IScopeFactory scopeFactory = context.Services.GetRequiredService<IScopeFactory>();
        MultiFileScope? scope = await scopeFactory.CreateMultiFileScopeAsync(paths, ct);

        if (scope == null)
            return ToolExecutionResult.Fail($"No files found for paths: {string.Join(", ", paths)}");

        return ToolExecutionResult.Ok(scope.BuildContext());
    }
}