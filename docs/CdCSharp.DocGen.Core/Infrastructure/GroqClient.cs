using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CdCSharp.DocGen.Core.Infrastructure;

public interface IAiClient : IDisposable
{
    Task<string> SendAsync(string prompt, int maxTokens = 2000, double temperature = 0.3);
    Task<T?> SendAsync<T>(string prompt, int maxTokens = 2000, double temperature = 0.3) where T : class;
}

public class GroqClient : IAiClient
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    private readonly string _model;
    private readonly bool _trace;
    private readonly SemaphoreSlim _rateLimiter;
    private int _requestCounter = 0;
    private const string BaseUrl = "https://api.groq.com/openai/v1/";
    private const int MinDelayMs = 2000;

    public GroqClient(string apiKey, ILogger? logger = null, string model = "llama-3.3-70b-versatile", bool trace = false)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(60)
        };
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _logger = logger ?? NullLogger.Instance;
        _model = model;
        _trace = trace;
        _rateLimiter = new SemaphoreSlim(1, 1);
    }

    public async Task<string> SendAsync(string prompt, int maxTokens = 2000, double temperature = 0.3)
    {
        await _rateLimiter.WaitAsync();
        try
        {
            int requestId = Interlocked.Increment(ref _requestCounter);
            string requestName = $"Request-{requestId}";

            if (_trace)
            {
                _logger.Trace($"Preparing API request #{requestId}");
                _logger.Trace($"Model: {_model}, MaxTokens: {maxTokens}, Temperature: {temperature}");
                _logger.TracePrompt(requestName, prompt);
            }

            await Task.Delay(MinDelayMs);

            GroqRequest request = new()
            {
                Messages = [new GroqMessage("user", prompt)],
                Model = _model,
                MaxTokens = maxTokens,
                Temperature = temperature
            };

            if (_trace)
            {
                _logger.Trace($"Sending request to Groq API...");
            }

            DateTime startTime = DateTime.UtcNow;
            HttpResponseMessage response = await _http.PostAsJsonAsync("chat/completions", request);
            TimeSpan elapsed = DateTime.UtcNow - startTime;

            if (_trace)
            {
                _logger.Trace($"Received response in {elapsed.TotalSeconds:F2}s");
                _logger.Trace($"Status: {(int)response.StatusCode} {response.StatusCode}");
            }

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.Warning("Rate limit hit, waiting 60s...");
                    if (_trace)
                    {
                        _logger.Trace($"Rate limit error details: {error}");
                    }
                    await Task.Delay(60000);
                    return string.Empty;
                }

                _logger.Warning($"Groq API error ({response.StatusCode}): {error}");
                return string.Empty;
            }

            GroqResponse? result = await response.Content.ReadFromJsonAsync<GroqResponse>();
            string content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

            if (_trace)
            {
                int inputTokens = prompt.Length / 4;
                int outputTokens = content.Length / 4;
                _logger.Trace($"Estimated tokens - Input: ~{inputTokens}, Output: ~{outputTokens}, Total: ~{inputTokens + outputTokens}");
                _logger.TraceResponse(requestName, content);
            }

            return content;
        }
        catch (TaskCanceledException)
        {
            _logger.Warning("Groq API timeout");
            if (_trace)
            {
                _logger.Trace("Request timed out after 60 seconds");
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.Warning($"Groq API failed: {ex.Message}");
            if (_trace)
            {
                _logger.Trace($"Exception details: {ex}");
            }
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

            if (_trace)
            {
                _logger.Trace($"Parsing response as {typeof(T).Name}");
                _logger.Trace($"Extracted JSON length: {json.Length} chars");
            }

            T? result = System.Text.Json.JsonSerializer.Deserialize<T>(json);

            if (_trace)
            {
                _logger.Trace($"Successfully parsed as {typeof(T).Name}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.Warning($"Failed to parse response as {typeof(T).Name}: {ex.Message}");
            if (_trace)
            {
                _logger.Trace($"Parse error details: {ex}");
                _logger.Trace($"Raw response that failed to parse: {response}");
            }
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