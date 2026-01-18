using CdCSharp.Theon.Infrastructure;
using System.Text.Json;

namespace CdCSharp.Theon.Tools;

public interface IToolRegistry
{
    IReadOnlyList<ITool> Tools { get; }
    ITool? GetTool(string name);
    string GeneratePromptDocumentation();
    IReadOnlyList<object> GetNativeToolDefinitions();

    Task<ToolExecutionResult> ExecuteAsync(
        string toolName,
        JsonElement parameters,
        ToolExecutionContext context,
        CancellationToken ct = default);

    Task<ToolExecutionResult> ExecuteAsync(
        string toolName,
        string argumentsJson,
        ToolExecutionContext context,
        CancellationToken ct = default);
}

public sealed class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, ITool> _tools;
    private readonly ITheonLogger _logger;

    public IReadOnlyList<ITool> Tools => _tools.Values.ToList();

    public ToolRegistry(IEnumerable<ITool> tools, ITheonLogger logger)
    {
        _tools = tools.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
        _logger = logger;
        _logger.Info($"ToolRegistry initialized with {_tools.Count} tools");
    }

    public ITool? GetTool(string name) =>
        _tools.TryGetValue(name, out ITool? tool) ? tool : null;

    public async Task<ToolExecutionResult> ExecuteAsync(
        string toolName,
        JsonElement parameters,
        ToolExecutionContext context,
        CancellationToken ct = default)
    {
        ITool? tool = GetTool(toolName);
        if (tool == null)
        {
            _logger.Warning($"Tool not found: {toolName}");
            return ToolExecutionResult.Fail($"Unknown tool: {toolName}");
        }

        _logger.Debug($"Executing tool: {toolName}");

        try
        {
            ToolExecutionResult result = await tool.ExecuteAsync(parameters, context, ct);

            if (result.Success)
                _logger.Debug($"Tool {toolName} executed successfully");
            else
                _logger.Warning($"Tool {toolName} failed: {result.ErrorMessage}");

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error($"Tool {toolName} threw exception", ex);
            return ToolExecutionResult.Fail(ex.Message);
        }
    }

    public async Task<ToolExecutionResult> ExecuteAsync(
        string toolName,
        string argumentsJson,
        ToolExecutionContext context,
        CancellationToken ct = default)
    {
        try
        {
            JsonElement parameters = JsonDocument.Parse(argumentsJson).RootElement;
            return await ExecuteAsync(toolName, parameters, context, ct);
        }
        catch (JsonException ex)
        {
            _logger.Error($"Failed to parse tool arguments: {ex.Message}");
            return ToolExecutionResult.Fail($"Invalid JSON arguments: {ex.Message}");
        }
    }

    public IReadOnlyList<object> GetNativeToolDefinitions()
    {
        return _tools.Values.Select(tool =>
        {
            ToolDefinition def = tool.GetDefinition();
            return new
            {
                type = "function",
                function = new
                {
                    name = def.Name,
                    description = def.Description,
                    parameters = def.ParametersSchema
                }
            };
        }).Cast<object>().ToList();
    }

    public string GeneratePromptDocumentation()
    {
        List<string> sections = [];

        IEnumerable<IGrouping<ToolCategory, ITool>> byCategory = _tools.Values.GroupBy(t => t.Category);

        foreach (IGrouping<ToolCategory, ITool> group in byCategory.OrderBy(g => g.Key))
        {
            sections.Add($"## {group.Key.ToString().ToUpperInvariant()} TOOLS");
            sections.Add("");

            foreach (ITool tool in group.OrderBy(t => t.Name))
            {
                ToolPromptSpec spec = tool.GetPromptSpec();

                sections.Add($"### {tool.Name}");
                sections.Add("");
                sections.Add(tool.Description);
                sections.Add("");
                sections.Add("**Syntax:**");
                sections.Add("```");
                sections.Add(spec.Syntax);
                if (spec.ClosingTag != null)
                {
                    sections.Add("... content ...");
                    sections.Add(spec.ClosingTag);
                }
                sections.Add("```");
                sections.Add("");

                if (spec.ParameterDescriptions.Length > 0)
                {
                    sections.Add("**Parameters:**");
                    foreach (string param in spec.ParameterDescriptions)
                        sections.Add($"- {param}");
                    sections.Add("");
                }

                sections.Add("**Example:**");
                sections.Add("```");
                sections.Add(spec.Example);
                sections.Add("```");
                sections.Add("");
            }
        }

        return string.Join("\n", sections);
    }
}