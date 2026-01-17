using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using CdCSharp.Theon.Orchestration;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CdCSharp.Theon.AI;

public class LMStudioClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly TheonLogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly MetricsCollector? _metrics;

    public LMStudioClient(string baseUrl, int timeoutSeconds, TheonLogger logger, MetricsCollector? metrics = null)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<string> SendAsync(List<ConversationMessage> messages, int maxTokens = 2000, string? agentId = null)
    {
        await _semaphore.WaitAsync();
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            object[] apiMessages = messages.Select(m => new
            {
                role = m.Role switch
                {
                    MessageRole.System => "system",
                    MessageRole.Assistant or MessageRole.AgentResponse => "assistant",
                    _ => "user"
                },
                content = m.Content
            }).ToArray();

            object request = new
            {
                messages = apiMessages,
                max_tokens = maxTokens,
                temperature = 0.3,
                stream = false
            };

            _logger.Debug($"Sending request with {messages.Count} messages");

            HttpResponseMessage response = await _http.PostAsJsonAsync("chat/completions", request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                _logger.Error($"LMStudio error: {response.StatusCode}", new Exception(error));
                return string.Empty;
            }

            LMStudioResponse? result = await response.Content.ReadFromJsonAsync<LMStudioResponse>();
            string content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

            // Registrar métricas
            if (_metrics != null && !string.IsNullOrEmpty(agentId))
            {
                int inputTokens = EstimateTokens(string.Join("", messages.Select(m => m.Content)));
                int outputTokens = EstimateTokens(content);
                _metrics.RecordTokenUsage(agentId, inputTokens, outputTokens);
            }

            _logger.Debug($"Received response: {content.Length} chars");
            return content;
        }
        catch (Exception ex)
        {
            _logger.Error("LMStudio request failed", ex);
            return string.Empty;
        }
        finally
        {
            sw.Stop();
            _semaphore.Release();
        }
    }

    private static int EstimateTokens(string text) => text.Length / 4;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> SendAsync<T>(List<ConversationMessage> messages, int maxTokens = 2000, int maxRetries = 2) where T : class
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            string response = await SendAsync(messages, maxTokens);

            if (string.IsNullOrWhiteSpace(response))
            {
                _logger.Warning($"Empty response on attempt {attempt + 1}");
                continue;
            }

            if (IsCorruptedResponse(response))
            {
                _logger.Warning($"Corrupted response detected on attempt {attempt + 1}: {response[..Math.Min(50, response.Length)]}");
                continue;
            }

            try
            {
                string json = ExtractJson(response);
                T? result = JsonSerializer.Deserialize<T>(json, JsonOptions);

                if (result != null && IsValidResponse(result))
                {
                    return result;
                }

                _logger.Warning($"Invalid response structure on attempt {attempt + 1}");
            }
            catch (JsonException ex)
            {
                _logger.Warning($"JSON parse failed on attempt {attempt + 1}: {ex.Message}");
            }

            if (attempt < maxRetries)
            {
                messages.Add(new ConversationMessage
                {
                    Role = MessageRole.User,
                    Content = "Your response was not valid JSON. Please respond with ONLY a JSON object, no other text."
                });
            }
        }

        _logger.Error($"Failed to get valid response after {maxRetries + 1} attempts");
        return null;
    }

    private static bool IsCorruptedResponse(string response)
    {
        string[] corruptPatterns = ["<|", "|>", "<|channel|>", "<|im_", "<|endoftext|>"];
        return corruptPatterns.Any(p => response.Contains(p));
    }

    private static bool IsValidResponse<T>(T response)
    {
        if (response is RoutingDecision routing)
        {
            return !string.IsNullOrWhiteSpace(routing.TargetExpertise);
        }
        return true;
    }

    private static string ExtractJson(string response)
    {
        int start = response.IndexOf('{');
        int end = response.LastIndexOf('}');
        if (start >= 0 && end > start)
            return response[start..(end + 1)];

        start = response.IndexOf('[');
        end = response.LastIndexOf(']');
        if (start >= 0 && end > start)
            return response[start..(end + 1)];

        return response;
    }

    public void Dispose()
    {
        _http.Dispose();
        _semaphore.Dispose();
    }

    private record LMStudioResponse(
        [property: JsonPropertyName("choices")] List<LMStudioChoice>? Choices,
        [property: JsonPropertyName("usage")] LMStudioUsage? Usage);

    private record LMStudioUsage(
        [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
        [property: JsonPropertyName("completion_tokens")] int CompletionTokens);

    private record LMStudioChoice(
        [property: JsonPropertyName("message")] LMStudioMessage? Message);

    private record LMStudioMessage(
        [property: JsonPropertyName("content")] string Content);
}