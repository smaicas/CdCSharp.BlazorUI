using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Abstractions.Infrastructure;
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
    }

    public Task<string> SendAsync(string prompt, int maxTokens = 2000, double temperature = 0.3)
    {
        List<ChatMessage> messages = [new("user", prompt)];
        return SendMessagesAsync(messages, maxTokens, temperature);
    }

    public async Task<string> SendMessagesAsync(
        IReadOnlyList<ChatMessage> messages,
        int maxTokens = 2000,
        double temperature = 0.3)
    {
        await _rateLimiter.WaitAsync();
        try
        {
            int requestId = Interlocked.Increment(ref _requestCounter);
            string requestName = $"Groq-{requestId}";

            _logger.LogDebug("Preparing Groq request #{RequestId}, Model: {Model}, Messages: {Count}, MaxTokens: {MaxTokens}",
                requestId, _model, messages.Count, maxTokens);

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
                    _logger.LogWarning("Groq rate limit hit, waiting 60s...");
                    await Task.Delay(60000);
                    return string.Empty;
                }

                _logger.LogWarning("Groq API error ({StatusCode}): {Error}", response.StatusCode, error);
                return string.Empty;
            }

            GroqResponse? result = await response.Content.ReadFromJsonAsync<GroqResponse>();
            string content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

            string promptSummary = string.Join("\n", messages.Select(m => $"[{m.Role}]: {Truncate(m.Content, 200)}"));
            await _tracer.TracePromptAsync(requestName, promptSummary, content);

            return content;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Groq API timeout");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Groq API failed");
            return string.Empty;
        }
        finally
        {
            _rateLimiter.Release();
        }
    }

    public async Task<T?> SendAsync<T>(string prompt, int maxTokens = 2000, double temperature = 0.3) where T : class
    {
        string response = await SendAsync(prompt, maxTokens, temperature);

        if (string.IsNullOrWhiteSpace(response))
            return null;

        try
        {
            string json = ExtractJson(response);
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