// Tools/Exploration/ExploreFolderTool.cs
using CdCSharp.Theon.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CdCSharp.Theon.Tools.Exploration;

public sealed class ExploreFolderTool : ITool
{
    public string Name => "EXPLORE_FOLDER";
    public string Description => "Explore all files in a folder/module.";
    public ToolCategory Category => ToolCategory.Exploration;

    public ToolPromptSpec GetPromptSpec() => new(
        Syntax: "[EXPLORE_FOLDER: path=\"folder/\"]",
        ClosingTag: null,
        Example: "[EXPLORE_FOLDER: path=\"Core/\"]",
        ParameterDescriptions: ["path: Folder path (should end with '/')"]);

    public ToolDefinition GetDefinition()
    {
        string schema = """
        {
            "type": "object",
            "properties": {
                "path": { "type": "string", "description": "Folder path to explore" }
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
            return ToolExecutionResult.Fail("Folder path cannot be empty");

        IScopeFactory scopeFactory = context.Services.GetRequiredService<IScopeFactory>();
        FolderScope? scope = await scopeFactory.CreateFolderScopeAsync(path, ct);

        if (scope == null)
            return ToolExecutionResult.Fail($"Folder not found: {path}");

        return ToolExecutionResult.Ok(scope.BuildContext());
    }
}