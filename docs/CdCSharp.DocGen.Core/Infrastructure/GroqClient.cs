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
    private readonly SemaphoreSlim _rateLimiter;
    private const string BaseUrl = "https://api.groq.com/openai/v1/";
    private const int MinDelayMs = 2000;

    public GroqClient(string apiKey, ILogger? logger = null, string model = "llama-3.3-70b-versatile")
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(60)
        };
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _logger = logger ?? NullLogger.Instance;
        _model = model;
        _rateLimiter = new SemaphoreSlim(1, 1);
    }

    public async Task<string> SendAsync(string prompt, int maxTokens = 2000, double temperature = 0.3)
    {
        await _rateLimiter.WaitAsync();
        try
        {
            await Task.Delay(MinDelayMs);

            GroqRequest request = new()
            {
                Messages = [new GroqMessage("user", prompt)],
                Model = _model,
                MaxTokens = maxTokens,
                Temperature = temperature
            };

            HttpResponseMessage response = await _http.PostAsJsonAsync("chat/completions", request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.Warning("Rate limit hit, waiting 60s...");
                    await Task.Delay(60000);
                    return string.Empty;
                }

                _logger.Warning($"Groq API error ({response.StatusCode}): {error}");
                return string.Empty;
            }

            GroqResponse? result = await response.Content.ReadFromJsonAsync<GroqResponse>();
            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        }
        catch (TaskCanceledException)
        {
            _logger.Warning("Groq API timeout");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.Warning($"Groq API failed: {ex.Message}");
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
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.Warning($"Failed to parse response as {typeof(T).Name}: {ex.Message}");
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