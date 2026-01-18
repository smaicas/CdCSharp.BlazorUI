// Tools/Exploration/ExploreAssemblyTool.cs
using CdCSharp.Theon.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CdCSharp.Theon.Tools.Exploration;

public sealed class ExploreAssemblyTool : ITool
{
    public string Name => "EXPLORE_ASSEMBLY";
    public string Description => "Discover assembly structure: namespaces, types, files, and references.";
    public ToolCategory Category => ToolCategory.Exploration;

    public ToolPromptSpec GetPromptSpec() => new(
        Syntax: "[EXPLORE_ASSEMBLY: name=\"AssemblyName\"]",
        ClosingTag: null,
        Example: "[EXPLORE_ASSEMBLY: name=\"CdCSharp.Theon\"]",
        ParameterDescriptions: ["name: Exact assembly name to explore"]);

    public ToolDefinition GetDefinition()
    {
        string schema = """
        {
            "type": "object",
            "properties": {
                "name": { "type": "string", "description": "Assembly name to explore" }
            },
            "required": ["name"]
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

        string assemblyName = nameElement.GetString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(assemblyName))
            return ToolExecutionResult.Fail("Assembly name cannot be empty");

        IScopeFactory scopeFactory = context.Services.GetRequiredService<IScopeFactory>();
        AssemblyScope? scope = await scopeFactory.CreateAssemblyScopeAsync(assemblyName, ct);

        if (scope == null)
            return ToolExecutionResult.Fail($"Assembly not found: {assemblyName}");

        return ToolExecutionResult.Ok(scope.BuildContext());
    }
}