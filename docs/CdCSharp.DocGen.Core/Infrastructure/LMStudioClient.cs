using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CdCSharp.DocGen.Core.Infrastructure;

/// <summary>
/// Cliente para LM Studio con API compatible con OpenAI
/// </summary>
public class LMStudioClient : IAiClient
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    private readonly string _model;
    private readonly bool _trace;
    private readonly SemaphoreSlim _rateLimiter;
    private int _requestCounter = 0;

    public LMStudioClient(
        string baseUrl = "http://localhost:1234/v1/",
        ILogger? logger = null,
        string model = "local-model",
        bool trace = false)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromMinutes(5) // LM Studio local puede tardar más
        };

        _logger = logger ?? NullLogger.Instance;
        _model = model;
        _trace = trace;
        _rateLimiter = new SemaphoreSlim(1, 1);

        _logger.Info($"LM Studio client initialized: {baseUrl}");
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
                _logger.Trace($"Preparing LM Studio request #{requestId}");
                _logger.Trace($"Model: {_model}, MaxTokens: {maxTokens}, Temperature: {temperature}");
                _logger.TracePrompt(requestName, prompt);
            }

            var request = new
            {
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                model = _model,
                max_tokens = maxTokens,
                temperature = temperature,
                stream = false
            };

            if (_trace)
            {
                _logger.Trace("Sending request to LM Studio...");
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
                _logger.Warning($"LM Studio API error ({response.StatusCode}): {error}");
                return string.Empty;
            }

            LMStudioResponse? result = await response.Content.ReadFromJsonAsync<LMStudioResponse>();
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
            _logger.Warning("LM Studio request timeout");
            if (_trace)
            {
                _logger.Trace("Request timed out");
            }
            return string.Empty;
        }
        catch (HttpRequestException ex)
        {
            _logger.Error($"Cannot connect to LM Studio: {ex.Message}");
            _logger.Info("Make sure LM Studio is running and has a model loaded");
            if (_trace)
            {
                _logger.Trace($"Connection error: {ex}");
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.Warning($"LM Studio API failed: {ex.Message}");
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
        // Buscar JSON entre ```json y ```
        int jsonStart = response.IndexOf("```json");
        if (jsonStart >= 0)
        {
            jsonStart += 7;
            int jsonEnd = response.IndexOf("```", jsonStart);
            if (jsonEnd > jsonStart)
            {
                return response[jsonStart..jsonEnd].Trim();
            }
        }

        // Buscar directamente { } o [ ]
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

    private record LMStudioResponse(
        [property: JsonPropertyName("choices")] List<LMStudioChoice>? Choices
    );

    private record LMStudioChoice(
        [property: JsonPropertyName("message")] LMStudioMessage? Message
    );

    private record LMStudioMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content
    );
}