// Tools/Exploration/ExploreFileTool.cs
using CdCSharp.Theon.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CdCSharp.Theon.Tools.Exploration;

public sealed class ExploreFileTool : ITool
{
    public string Name => "EXPLORE_FILE";
    public string Description => "Retrieve full source code of a specific file.";
    public ToolCategory Category => ToolCategory.Exploration;

    public ToolPromptSpec GetPromptSpec() => new(
        Syntax: "[EXPLORE_FILE: path=\"relative/path.cs\"]",
        ClosingTag: null,
        Example: "[EXPLORE_FILE: path=\"Core/Orchestrator.cs\"]",
        ParameterDescriptions: ["path: Relative path from project root"]);

    public ToolDefinition GetDefinition()
    {
        string schema = """
        {
            "type": "object",
            "properties": {
                "path": { "type": "string", "description": "Relative file path" }
            },
            "required": ["path"]
        }
        """;

        return new ToolDefinition(Name, Description, JsonDocument.Parse(schema).RootElement);
    }

    public async Task<ToolExecutionResult> ExecuteAsync(
        JsonElement parameters,
        ToolExecutionContext context,
        CancellationToken ct = default)
    {
        if (!parameters.TryGetProperty("path", out JsonElement pathElement))
            return ToolExecutionResult.Fail("Missing required parameter: path");

        string path = pathElement.GetString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path))
            return ToolExecutionResult.Fail("File path cannot be empty");

        IScopeFactory scopeFactory = context.Services.GetRequiredService<IScopeFactory>();
        FileScope? scope = await scopeFactory.CreateFileScopeAsync(path, ct);

        if (scope == null)
            return ToolExecutionResult.Fail($"File not found: {path}");

        return ToolExecutionResult.Ok(scope.BuildContext());
    }
}