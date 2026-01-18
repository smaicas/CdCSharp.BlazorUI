// Tools/Modification/ModifyProjectFileTool.cs
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CdCSharp.Theon.Tools.Modification;

public sealed class ModifyProjectFileTool : ITool
{
    public string Name => "MODIFY_PROJECT_FILE";
    public string Description => "Modify actual project source code. Requires confirmation.";
    public ToolCategory Category => ToolCategory.Modification;

    public ToolPromptSpec GetPromptSpec() => new(
        Syntax: "[MODIFY_PROJECT_FILE: path=\"relative/path.cs\"]",
        ClosingTag: "[/MODIFY_PROJECT_FILE]",
        Example: """
            [MODIFY_PROJECT_FILE: path="Core/Example.cs"]
            using System;
            
            namespace Example;
            
            public class Modified { }
            [/MODIFY_PROJECT_FILE]
            """,
        ParameterDescriptions:
        [
            "path: Relative path of file to modify",
            "content: Complete new file content (between tags)"
        ]);

    public ToolDefinition GetDefinition()
    {
        string schema = """
        {
            "type": "object",
            "properties": {
                "path": { "type": "string", "description": "Relative file path" },
                "content": { "type": "string", "description": "New file content" }
            },
            "required": ["path", "content"]
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

        if (!parameters.TryGetProperty("content", out JsonElement contentElement))
            return ToolExecutionResult.Fail("Missing required parameter: content");

        string path = pathElement.GetString() ?? string.Empty;
        string content = contentElement.GetString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(path))
            return ToolExecutionResult.Fail("File path cannot be empty");

        TheonOptions options = context.Services.GetRequiredService<TheonOptions>();

        if (!options.Modification.Enabled)
            return ToolExecutionResult.Fail("Project modification is disabled");

        if (options.Modification.RequireConfirmation)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nModify project file: {path}");
            Console.Write("Confirm? (y/N): ");
            Console.ResetColor();

            string? response = Console.ReadLine();
            if (!string.Equals(response, "y", StringComparison.OrdinalIgnoreCase))
                return ToolExecutionResult.Fail("Modification rejected by user");
        }

        IFileSystem fileSystem = context.Services.GetRequiredService<IFileSystem>();
        bool success = await fileSystem.WriteProjectFileAsync(path, content, ct);

        if (!success)
            return ToolExecutionResult.Fail($"Failed to write file: {path}");

        IProjectAnalysis analysis = context.Services.GetRequiredService<IProjectAnalysis>();
        await analysis.RefreshFileAsync(path, ct);

        return ToolExecutionResult.Ok($"Modified project file: {path}");
    }
}