using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CdCSharp.Theon.AI;

public class LMStudioClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly TheonLogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public LMStudioClient(string baseUrl, int timeoutSeconds, TheonLogger logger)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
        _logger = logger;
    }

    public async Task<string> SendAsync(List<ConversationMessage> messages, int maxTokens = 2000)
    {
        await _semaphore.WaitAsync();
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
            _semaphore.Release();
        }
    }

    public async Task<T?> SendAsync<T>(List<ConversationMessage> messages, int maxTokens = 2000) where T : class
    {
        string response = await SendAsync(messages, maxTokens);
        if (string.IsNullOrWhiteSpace(response)) return null;

        try
        {
            string json = ExtractJson(response);
            return JsonSerializer.Deserialize<T>(json);
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
        _semaphore.Dispose();
    }

    private record LMStudioResponse(
        [property: JsonPropertyName("choices")] List<LMStudioChoice>? Choices);

    private record LMStudioChoice(
        [property: JsonPropertyName("message")] LMStudioMessage? Message);

    private record LMStudioMessage(
        [property: JsonPropertyName("content")] string Content);
}