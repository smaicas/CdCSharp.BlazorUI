using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Abstractions.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CdCSharp.DocGen.Core.AI;

public class LMStudioClient : IAiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<LMStudioClient> _logger;
    private readonly IPromptTracer _tracer;
    private readonly SemaphoreSlim _rateLimiter;
    private int _requestCounter;

    public LMStudioClient(
        string baseUrl,
        ILogger<LMStudioClient> logger,
        IPromptTracer tracer)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromMinutes(5)
        };

        _logger = logger;
        _tracer = tracer;
        _rateLimiter = new SemaphoreSlim(1, 1);

        _logger.LogInformation("LM Studio client initialized: {BaseUrl}", baseUrl);
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

        int requestId = Interlocked.Increment(ref _requestCounter);
        string agentId = $"LMStudio-{requestId}";
        string traceId = string.Empty;

        try
        {
            _logger.LogDebug("Preparing LM Studio request #{RequestId}, Messages: {Count}, MaxTokens: {MaxTokens}",
                requestId, messages.Count, maxTokens);

            string promptSummary = string.Join("\n", messages.Select(m => $"""
            [{m.Role}]: 
            
            {m.Content}
            """));

            // ✅ Traza ANTES de enviar
            traceId = await _tracer.TracePromptStartAsync(agentId, promptSummary);

            var request = new
            {
                messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
                max_tokens = maxTokens,
                temperature,
                stream = false
            };

            DateTime startTime = DateTime.UtcNow;
            HttpResponseMessage response = await _http.PostAsJsonAsync("chat/completions", request);
            TimeSpan elapsed = DateTime.UtcNow - startTime;

            _logger.LogDebug("LM Studio response received in {Elapsed:F2}s, Status: {StatusCode}",
                elapsed.TotalSeconds, response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("LM Studio API error ({StatusCode}): {Error}", response.StatusCode, error);

                // ✅ Traza el error
                await _tracer.TracePromptFailureAsync(traceId,
                    new Exception($"HTTP {response.StatusCode}: {error}"));

                return string.Empty;
            }

            LMStudioResponse? result = await response.Content.ReadFromJsonAsync<LMStudioResponse>();
            string content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

            // ✅ Completa la traza con la respuesta
            await _tracer.TracePromptCompleteAsync(traceId, content);

            return content;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning("LM Studio request timeout");
            await _tracer.TracePromptFailureAsync(traceId, ex);
            return string.Empty;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Cannot connect to LM Studio. Make sure it's running and has a model loaded");
            await _tracer.TracePromptFailureAsync(traceId, ex);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LM Studio API failed");
            await _tracer.TracePromptFailureAsync(traceId, ex);
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
        int jsonStart = response.IndexOf("```json");
        if (jsonStart >= 0)
        {
            jsonStart += 7;
            int jsonEnd = response.IndexOf("```", jsonStart);
            if (jsonEnd > jsonStart)
                return response[jsonStart..jsonEnd].Trim();
        }

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