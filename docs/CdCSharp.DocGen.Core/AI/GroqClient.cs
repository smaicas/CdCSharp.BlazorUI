// AI/GroqClient.cs
using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Abstractions.Infrastructure;
using CdCSharp.DocGen.Core.Models.AI;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CdCSharp.DocGen.Core.AI;

public class GroqClient : IAiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<GroqClient> _logger;
    private readonly IPromptTracer _tracer;
    private readonly RetryPolicy _retryPolicy;
    private readonly string _model;
    private readonly SemaphoreSlim _rateLimiter;
    private int _requestCounter;

    private const string BaseUrl = "https://api.groq.com/openai/v1/";
    private const int MinDelayMs = 2000;

    public GroqClient(
        string apiKey,
        string model,
        ILogger<GroqClient> logger,
        IPromptTracer tracer)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(60)
        };
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        _logger = logger;
        _tracer = tracer;
        _model = model;
        _rateLimiter = new SemaphoreSlim(1, 1);
        _retryPolicy = new RetryPolicy(logger);
    }

    public async Task<string> SendAsync(string prompt, int maxTokens = 2000, double temperature = 0.3)
    {
        AiResponse response = await SendWithResponseAsync(prompt, maxTokens, temperature);
        return response.Content;
    }

    public async Task<string> SendMessagesAsync(
        IReadOnlyList<ChatMessage> messages,
        int maxTokens = 2000,
        double temperature = 0.3)
    {
        AiResponse response = await SendMessagesWithResponseAsync(messages, maxTokens, temperature);
        return response.Content;
    }

    public Task<AiResponse> SendWithResponseAsync(string prompt, int maxTokens = 2000, double temperature = 0.3)
    {
        List<ChatMessage> messages = [new("user", prompt)];
        return SendMessagesWithResponseAsync(messages, maxTokens, temperature);
    }

    public async Task<AiResponse> SendMessagesWithResponseAsync(
        IReadOnlyList<ChatMessage> messages,
        int maxTokens = 2000,
        double temperature = 0.3)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            await _rateLimiter.WaitAsync();

            int requestId = Interlocked.Increment(ref _requestCounter);
            string agentId = $"Groq-{requestId}";
            string traceId = string.Empty;

            try
            {
                _logger.LogDebug("Preparing Groq request #{RequestId}, Model: {Model}, Messages: {Count}, MaxTokens: {MaxTokens}",
                    requestId, _model, messages.Count, maxTokens);

                string promptSummary = string.Join("\n", messages.Select(m =>
                    $"[{m.Role}]: {Truncate(m.Content, 200)}"));

                traceId = await _tracer.TracePromptStartAsync(agentId, promptSummary);

                await Task.Delay(MinDelayMs);

                GroqRequest request = new()
                {
                    Messages = messages.Select(m => new GroqMessage(m.Role, m.Content)).ToArray(),
                    Model = _model,
                    MaxTokens = maxTokens,
                    Temperature = temperature
                };

                DateTime startTime = DateTime.UtcNow;
                HttpResponseMessage response = await _http.PostAsJsonAsync("chat/completions", request);
                TimeSpan elapsed = DateTime.UtcNow - startTime;

                _logger.LogDebug("Groq response received in {Elapsed:F2}s, Status: {StatusCode}",
                    elapsed.TotalSeconds, response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        await _tracer.TracePromptFailureAsync(traceId, new Exception($"Rate limit: {error}"));
                        return AiResponse.Fail(AiErrorType.RateLimit, error);
                    }

                    await _tracer.TracePromptFailureAsync(traceId, new Exception($"HTTP {response.StatusCode}: {error}"));
                    return AiResponse.Fail(AiErrorType.InvalidResponse, $"HTTP {response.StatusCode}: {error}");
                }

                GroqResponse? result = await response.Content.ReadFromJsonAsync<GroqResponse>();
                string content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

                await _tracer.TracePromptCompleteAsync(traceId, content);

                return new AiResponse
                {
                    Success = true,
                    Content = content,
                    Metrics = new AiMetrics
                    {
                        EstimatedInputTokens = messages.Sum(m => m.Content.Length) / 4,
                        EstimatedOutputTokens = content.Length / 4,
                        LatencySeconds = elapsed.TotalSeconds
                    }
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning("Groq API timeout");
                await _tracer.TracePromptFailureAsync(traceId, ex);
                return AiResponse.Fail(AiErrorType.Timeout, "Request timed out");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Groq API connection error");
                await _tracer.TracePromptFailureAsync(traceId, ex);
                return AiResponse.Fail(AiErrorType.ConnectionError, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Groq API failed");
                await _tracer.TracePromptFailureAsync(traceId, ex);
                return AiResponse.Fail(AiErrorType.Unknown, ex.Message);
            }
            finally
            {
                _rateLimiter.Release();
            }
        });
    }

    public async Task<T?> SendAsync<T>(string prompt, int maxTokens = 2000, double temperature = 0.3) where T : class
    {
        AiResponse response = await SendWithResponseAsync(prompt, maxTokens, temperature);

        if (!response.Success || string.IsNullOrWhiteSpace(response.Content))
            return null;

        try
        {
            string json = ExtractJson(response.Content);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse response as {TypeName}", typeof(T).Name);
            return null;
        }
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

    private static string Truncate(string text, int maxLength)
        => text.Length <= maxLength ? text : text[..maxLength] + "...";

    public void Dispose()
    {
        _http.Dispose();
        _rateLimiter.Dispose();
    }

    private record GroqRequest
    {
        [JsonPropertyName("messages")]
        public GroqMessage[] Messages { get; init; } = [];

        [JsonPropertyName("model")]
        public string Model { get; init; } = string.Empty;

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; init; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; init; }
    }

    private record GroqMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content
    );

    private record GroqResponse(
        [property: JsonPropertyName("choices")] List<GroqChoice>? Choices
    );

    private record GroqChoice(
        [property: JsonPropertyName("message")] GroqMessage? Message
    );
}