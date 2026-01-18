using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Tools;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CdCSharp.Theon.Core;

public interface ILlmClient
{
    Task<LlmResponse> SendAsync(IReadOnlyList<LlmMessage> messages, CancellationToken ct = default);
    Task<ModelInfo> GetModelInfoAsync(CancellationToken ct = default);
    Task<ModelCapabilities> DetectCapabilitiesAsync(CancellationToken ct = default);
    int EstimateTokens(string text);
    ModelCapabilities Capabilities { get; }
    IReadOnlyList<object> GetToolDefinitions();
}

public sealed class ModelCapabilities
{
    public bool SupportsTools { get; set; }
}

public sealed class LlmClient : ILlmClient, IDisposable
{
    private readonly HttpClient _http;
    private readonly TheonOptions _options;
    private readonly ITheonLogger _logger;
    private readonly IToolRegistry _toolRegistry;
    private readonly Regex? _reasoningRegex;

    private ModelInfo? _cachedModelInfo;
    private ModelCapabilities? _capabilities;

    public ModelCapabilities Capabilities => _capabilities ?? new ModelCapabilities();

    public LlmClient(TheonOptions options, ITheonLogger logger, IToolRegistry toolRegistry)
    {
        _options = options;
        _logger = logger;
        _toolRegistry = toolRegistry;

        _http = new HttpClient
        {
            BaseAddress = new Uri(options.Llm.BaseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(options.Llm.TimeoutSeconds)
        };

        if (!string.IsNullOrWhiteSpace(options.Llm.ReasoningTagPattern))
        {
            try
            {
                _reasoningRegex = new Regex(options.Llm.ReasoningTagPattern, RegexOptions.Compiled | RegexOptions.Singleline);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Invalid reasoning pattern: {ex.Message}");
            }
        }

        if (options.Llm.Capabilities != null)
        {
            _capabilities = new ModelCapabilities
            {
                SupportsTools = options.Llm.Capabilities.SupportsTools ?? false
            };
        }
    }

    public IReadOnlyList<object> GetToolDefinitions() => _toolRegistry.GetNativeToolDefinitions();

    public async Task<ModelCapabilities> DetectCapabilitiesAsync(CancellationToken ct = default)
    {
        if (_capabilities != null)
            return _capabilities;

        _capabilities = new ModelCapabilities();

        try
        {
            object testRequest = new
            {
                messages = new[] { new { role = "user", content = "What is 2+2?" } },
                tools = new[]
                {
                    new
                    {
                        type = "function",
                        function = new
                        {
                            name = "calculate",
                            description = "Perform calculation",
                            parameters = new
                            {
                                type = "object",
                                properties = new { expression = new { type = "string" } },
                                required = new[] { "expression" }
                            }
                        }
                    }
                },
                max_tokens = 50,
                temperature = 0
            };

            HttpResponseMessage response = await _http.PostAsJsonAsync("chat/completions", testRequest, ct);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync(ct);
                if (!content.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                    !content.Contains("not supported", StringComparison.OrdinalIgnoreCase))
                {
                    _capabilities.SupportsTools = true;
                    _logger.Info("Model supports native tools");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Debug($"Tools detection failed: {ex.Message}");
        }

        _logger.Info($"Model capabilities: Tools={_capabilities.SupportsTools}");
        return _capabilities;
    }

    public async Task<LlmResponse> SendAsync(IReadOnlyList<LlmMessage> messages, CancellationToken ct = default)
    {
        await DetectCapabilitiesAsync(ct);

        _logger.LogLlmRequest(messages);

        object request = BuildRequest(messages);

        HttpResponseMessage response = await _http.PostAsJsonAsync("chat/completions", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(ct);
            _logger.Error($"LLM error: {response.StatusCode} - {error}");
            return new LlmResponse(string.Empty, null, 0, 0);
        }

        ApiResponse? result = await response.Content.ReadFromJsonAsync<ApiResponse>(ct);

        ApiChoice? choice = result?.Choices?.FirstOrDefault();
        string content = choice?.Message?.Content ?? string.Empty;
        List<LlmToolCall>? toolCalls = null;

        if (choice?.Message?.ToolCalls != null && choice.Message.ToolCalls.Count > 0)
        {
            toolCalls = choice.Message.ToolCalls
                .Select(tc => new LlmToolCall(tc.Id, tc.Function.Name, tc.Function.Arguments))
                .ToList();
        }

        if (_reasoningRegex != null && !string.IsNullOrEmpty(content))
        {
            content = _reasoningRegex.Replace(content, "").Trim();
        }

        _logger.LogLlmResponse(content, toolCalls);

        return new LlmResponse(
            content,
            toolCalls,
            result?.Usage?.PromptTokens ?? 0,
            result?.Usage?.CompletionTokens ?? 0);
    }

    private object BuildRequest(IReadOnlyList<LlmMessage> messages)
    {
        List<object> apiMessages = [];

        foreach (LlmMessage msg in messages)
        {
            if (msg.ToolCallId != null)
            {
                apiMessages.Add(new
                {
                    role = "tool",
                    content = msg.Content,
                    tool_call_id = msg.ToolCallId
                });
            }
            else if (msg.ToolCalls != null)
            {
                apiMessages.Add(new
                {
                    role = msg.Role,
                    tool_calls = msg.ToolCalls.Select(tc => new
                    {
                        id = tc.Id,
                        type = "function",
                        function = new { name = tc.Name, arguments = tc.Arguments }
                    }).ToArray()
                });
            }
            else
            {
                apiMessages.Add(new { role = msg.Role, content = msg.Content });
            }
        }

        Dictionary<string, object> request = new()
        {
            ["messages"] = apiMessages,
            ["temperature"] = _options.Llm.Temperature,
            ["stream"] = false
        };

        if (_capabilities?.SupportsTools == true)
        {
            request["tools"] = _toolRegistry.GetNativeToolDefinitions();
        }

        return request;
    }

    public async Task<ModelInfo> GetModelInfoAsync(CancellationToken ct = default)
    {
        if (_cachedModelInfo != null)
            return _cachedModelInfo;

        try
        {
            HttpResponseMessage response = await _http.GetAsync("models", ct);
            if (response.IsSuccessStatusCode)
            {
                ModelsResponse? result = await response.Content.ReadFromJsonAsync<ModelsResponse>(ct);
                ApiModelInfo? loaded = result?.Data?.FirstOrDefault(m => m.State == "loaded");

                if (loaded != null)
                {
                    _cachedModelInfo = new ModelInfo(loaded.Id, loaded.MaxContextLength);
                    _logger.Info($"Model: {loaded.Id} (context: {loaded.MaxContextLength} tokens)");
                    return _cachedModelInfo;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"Could not get model info: {ex.Message}");
        }

        _cachedModelInfo = new ModelInfo("unknown", 8192);
        return _cachedModelInfo;
    }

    public int EstimateTokens(string text) => text.Length / 4;

    public void Dispose() => _http.Dispose();

    #region API Response Models

    private sealed record ApiResponse(
        [property: JsonPropertyName("choices")] List<ApiChoice>? Choices,
        [property: JsonPropertyName("usage")] ApiUsage? Usage);

    private sealed record ApiChoice(
        [property: JsonPropertyName("message")] ApiMessage? Message,
        [property: JsonPropertyName("finish_reason")] string? FinishReason);

    private sealed record ApiMessage(
        [property: JsonPropertyName("role")] string? Role,
        [property: JsonPropertyName("content")] string? Content,
        [property: JsonPropertyName("tool_calls")] List<ApiToolCall>? ToolCalls);

    private sealed record ApiToolCall(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("function")] ApiFunction Function);

    private sealed record ApiFunction(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("arguments")] string Arguments);

    private sealed record ApiUsage(
        [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
        [property: JsonPropertyName("completion_tokens")] int CompletionTokens);

    private sealed record ModelsResponse(
        [property: JsonPropertyName("data")] List<ApiModelInfo>? Data);

    private sealed record ApiModelInfo(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("state")] string State,
        [property: JsonPropertyName("max_context_length")] int MaxContextLength);

    #endregion
}