// Tools/Output/AppendFileTool.cs
using CdCSharp.Theon.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CdCSharp.Theon.Tools.Output;

public sealed class AppendFileTool : ITool
{
    public string Name => "APPEND_FILE";
    public string Description => "Append content to an existing generated file.";
    public ToolCategory Category => ToolCategory.Output;

    public ToolPromptSpec GetPromptSpec() => new(
        Syntax: "[APPEND_FILE: name=\"filename.ext\"]",
        ClosingTag: "[/APPEND_FILE]",
        Example: """
            [APPEND_FILE: name="README.md"]
            ## Additional Section
            More content...
            [/APPEND_FILE]
            """,
        ParameterDescriptions:
        [
            "name: Existing output filename",
            "content: Content to append (between tags)"
        ]);

    public ToolDefinition GetDefinition()
    {
        string schema = """
        {
            "type": "object",
            "properties": {
                "name": { "type": "string", "description": "Existing output filename" },
                "content": { "type": "string", "description": "Content to append" }
            },
            "required": ["name", "content"]
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

        string? existingContent = outputContext.GetGeneratedFileContent(name);
        string newContent = existingContent != null
            ? existingContent + "\n" + content
            : content;

        await fileSystem.WriteOutputFileAsync(outputContext.CurrentResponseFolder, name, newContent, ct);
        outputContext.UpdateGeneratedFile(name, newContent);

        return ToolExecutionResult.Ok($"Appended to file: {name} ({content.Length} chars added)");
    }
}