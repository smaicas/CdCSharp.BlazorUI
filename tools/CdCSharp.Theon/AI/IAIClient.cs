using CdCSharp.Theon.Infrastructure;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CdCSharp.Theon.AI;

// ==================== Interfaces ====================
public interface IAIClient
{
    Task<ChatCompletionResponse> SendAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);
}

// ==================== Client Implementation ====================
public class LMStudioClient : IAIClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITheonLogger _logger;
    private readonly TheonOptions _options;

    public LMStudioClient(IOptions<TheonOptions> options, ITheonLogger logger)
    {
        _options = options.Value;
        _logger = logger;
        _baseUrl = options.Value.Llm.BaseUrl.TrimEnd('/');

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(options.Value.Llm.TimeoutSeconds)
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    public async Task<ChatCompletionResponse> SendAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string json = JsonSerializer.Serialize(request, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            string uri = $"/{_options.Llm.CompletionsUrl.TrimStart('/').TrimEnd('/')}";

            _logger.Debug($"Sending request to {_baseUrl}{uri}");

            HttpResponseMessage response = await _httpClient.PostAsync(
                uri,
                content,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            ChatCompletionResponse? result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson, _jsonOptions);

            return result ?? throw new InvalidOperationException("Failed to deserialize response");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to send chat completion request", ex);
            throw;
        }
    }

    private async Task<ModelsResponse> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("/v1/models", cancellationToken);
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            ModelsResponse? result = JsonSerializer.Deserialize<ModelsResponse>(responseJson, _jsonOptions);

            return result ?? throw new InvalidOperationException("Failed to deserialize models response");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to get models list", ex);
            throw;
        }
    }

    private async Task<bool> IsModelLoadedAsync(string modelName, CancellationToken cancellationToken = default)
    {
        try
        {
            ModelsResponse models = await GetModelsAsync(cancellationToken);
            return models.Data?.Any(m => m.Id == modelName) ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task ValidateCapabilities()
    {
        _logger.Section("Testing LM Studio Model Capabilities");

        _logger.Info("Checking model status...");
        bool isLoaded = await IsModelLoadedAsync(_options.Llm.Model);

        if (!isLoaded)
        {
            _logger.Warning($"Model '{_options.Llm.Model}' is NOT loaded");
            _logger.Info("Please load the model in LM Studio or update the model in configuration.");

            ModelsResponse models = await GetModelsAsync();
            if (models.Data?.Any() == true)
            {
                _logger.Info("Available models:");
                foreach (ModelInfo model in models.Data)
                {
                    _logger.Info($"  - {model.Id}");
                }
            }
            return;
        }

        _logger.Success($"Model '{_options.Llm.Model}' is loaded");

        // Test 1: Structured Output Support
        _logger.Info("Testing Structured Output...");
        try
        {
            ChatCompletionRequest structuredRequest = new()
            {
                Model = _options.Llm.Model,
                Messages =
                [
                    new() { Role = "user", Content = "Generate a simple person profile" }
                ],
                ResponseFormat = new ResponseFormat
                {
                    Type = "json_schema",
                    JsonSchema = new JsonSchema
                    {
                        Name = "person_profile",
                        Strict = "true",
                        Schema = new
                        {
                            type = "object",
                            properties = new
                            {
                                name = new { type = "string" },
                                age = new { type = "number" }
                            },
                            required = new[] { "name", "age" }
                        }
                    }
                },
                MaxTokens = 100,
                Temperature = _options.Llm.Temperature
            };

            ChatCompletionResponse structuredResponse = await SendAsync(structuredRequest);
            string? content = structuredResponse.Choices[0].Message.Content;

            JsonElement json = JsonSerializer.Deserialize<JsonElement>(content ?? "{}");
            if (json.TryGetProperty("name", out _) && json.TryGetProperty("age", out _))
            {
                _logger.Success("Structured Output: SUPPORTED");
            }
            else
            {
                _logger.Warning("Structured Output: NOT SUPPORTED (invalid schema)");
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"Structured Output: NOT SUPPORTED ({ex.Message})");
        }

        // Test 2: Tool Use Support
        _logger.Info("Testing Tool Use...");
        try
        {
            Tool getTool = new()
            {
                Type = "function",
                Function = new FunctionDefinition
                {
                    Name = "get_current_time",
                    Description = "Get the current time for a given timezone",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, PropertyDefinition>
                        {
                            ["timezone"] = new()
                            {
                                Type = "string",
                                Description = "The timezone (e.g., 'America/New_York', 'Europe/London')"
                            }
                        },
                        Required = ["timezone"],
                        AdditionalProperties = false
                    }
                }
            };

            ChatCompletionRequest toolRequest = new()
            {
                Model = _options.Llm.Model,
                Messages =
                [
                    new()
                    {
                        Role = "user",
                        Content = "What time is it in Tokyo?"
                    }
                ],
                Tools = [getTool],
                MaxTokens = 100,
                Temperature = _options.Llm.Temperature
            };

            ChatCompletionResponse toolResponse = await SendAsync(toolRequest);

            if (toolResponse.Choices[0].FinishReason == "tool_calls"
                && toolResponse.Choices[0].Message.ToolCalls != null
                && toolResponse.Choices[0].Message.ToolCalls?.Count > 0
                && toolResponse.Choices[0].Message.ToolCalls?.FirstOrDefault()?.Function.Name == "get_current_time")
            {
                _logger.Success("Tool Use: SUPPORTED");
            }
            else
            {
                _logger.Warning("Tool Use: NOT SUPPORTED (no tool calls generated)");
                _logger.Debug($"Finish Reason: {toolResponse.Choices[0].FinishReason}");
                _logger.Debug($"Response: {toolResponse.Choices[0].Message.Content}");
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"Tool Use: NOT SUPPORTED ({ex.Message})");
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// ==================== Request Models ====================
public class ChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "default";

    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = [];

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    [JsonPropertyName("response_format")]
    public ResponseFormat? ResponseFormat { get; set; }

    [JsonPropertyName("tools")]
    public List<Tool>? Tools { get; set; }
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tool_calls")]
    public List<ToolCall>? ToolCalls { get; set; }

    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }
}

// ==================== Response Format (Structured Output) ====================
public class ResponseFormat
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "json_schema";

    [JsonPropertyName("json_schema")]
    public JsonSchema? JsonSchema { get; set; }
}

public class JsonSchema
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("strict")]
    public string? Strict { get; set; }

    [JsonPropertyName("schema")]
    public object Schema { get; set; } = new();
}

// ==================== Tools ====================
public class Tool
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public FunctionDefinition Function { get; set; } = new();
}

public class FunctionDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public FunctionParameters Parameters { get; set; } = new();
}

public class FunctionParameters
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, PropertyDefinition> Properties { get; set; } = [];

    [JsonPropertyName("required")]
    public List<string>? Required { get; set; }

    [JsonPropertyName("additionalProperties")]
    public bool AdditionalProperties { get; set; } = false;
}

public class PropertyDefinition
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("enum")]
    public List<string>? Enum { get; set; }
}

// ==================== Response Models ====================
public class ModelsResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = "list";

    [JsonPropertyName("data")]
    public List<ModelInfo>? Data { get; set; }
}

public class ModelInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = "model";

    [JsonPropertyName("owned_by")]
    public string? OwnedBy { get; set; }

    [JsonPropertyName("created")]
    public long? Created { get; set; }
}

public class ChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = [];

    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
}

public class Choice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; } = new();

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("logprobs")]
    public object? Logprobs { get; set; }
}

public class ToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public FunctionCall Function { get; set; } = new();
}

public class FunctionCall
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}