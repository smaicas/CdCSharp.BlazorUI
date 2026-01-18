// Tools/Output/GenerateFileTool.cs
using CdCSharp.Theon.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CdCSharp.Theon.Tools.Output;

public sealed class GenerateFileTool : ITool
{
    public string Name => "GENERATE_FILE";
    public string Description => "Create a new output file. Content must be between opening and closing tags.";
    public ToolCategory Category => ToolCategory.Output;

    public ToolPromptSpec GetPromptSpec() => new(
        Syntax: "[GENERATE_FILE: name=\"filename.ext\" language=\"lang\"]",
        ClosingTag: "[/GENERATE_FILE]",
        Example: """
            [GENERATE_FILE: name="README.md" language="markdown"]
            # Project Title
            Content here...
            [/GENERATE_FILE]
            """,
        ParameterDescriptions:
        [
            "name: Output filename",
            "language: File language (markdown, csharp, json, xml, txt)",
            "content: File content (between tags)"
        ]);

    public ToolDefinition GetDefinition()
    {
        string schema = """
        {
            "type": "object",
            "properties": {
                "name": { "type": "string", "description": "Output filename" },
                "language": { "type": "string", "description": "File language" },
                "content": { "type": "string", "description": "File content" }
            },
            "required": ["name", "language", "content"]
        }
        """;

        return new ToolDefinition(Name, Description, JsonDocument.Parse(schema).RootElement);
    }

    public async Task<ToolExecutionResult> ExecuteAsync(
        JsonElement parameters,
        ToolExecutionContext context,
        CancellationToken ct = default)
    {
        if (!parameters.TryGetProperty("name", out JsonElement nameElement))
            return ToolExecutionResult.Fail("Missing required parameter: name");

        if (!parameters.TryGetProperty("content", out JsonElement contentElement))
            return ToolExecutionResult.Fail("Missing required parameter: content");

        string name = nameElement.GetString() ?? string.Empty;
        string content = contentElement.GetString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
            return ToolExecutionResult.Fail("Filename cannot be empty");

        IFileSystem fileSystem = context.Services.GetRequiredService<IFileSystem>();
        IOutputContext outputContext = context.Services.GetRequiredService<IOutputContext>();

        await fileSystem.WriteOutputFileAsync(outputContext.CurrentResponseFolder, name, content, ct);
        outputContext.UpdateGeneratedFile(name, content);

        return ToolExecutionResult.Ok($"Generated file: {name} ({content.Length} chars)");
    }
}