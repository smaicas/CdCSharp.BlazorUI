using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using CdCSharp.Theon.Orchestration;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CdCSharp.Theon.AI;

public class LMStudioClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly TheonLogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly MetricsCollector? _metrics;
    private readonly string? _reasoningPattern;
    private readonly Regex? _reasoningRegex;

    private const double Temperature = 0.7;

    public LMStudioClient(string baseUrl, int timeoutSeconds, TheonLogger logger, MetricsCollector? metrics = null, string? reasoningPattern = null)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
        _logger = logger;
        _metrics = metrics;
        _reasoningPattern = reasoningPattern;

        if (!string.IsNullOrWhiteSpace(_reasoningPattern))
        {
            try
            {
                _reasoningRegex = new Regex(_reasoningPattern, RegexOptions.Compiled | RegexOptions.Singleline);
                _logger.Info($"Reasoning filter enabled with pattern: {_reasoningPattern}");
            }
            catch (Exception ex)
            {
                _logger.Warning($"Invalid reasoning pattern '{_reasoningPattern}': {ex.Message}");
                _reasoningRegex = null;
            }
        }
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
                //max_tokens = maxTokens,
                temperature = Temperature,
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

            // Aplicar filtro de razonamiento si está configurado
            if (_reasoningRegex != null && !string.IsNullOrEmpty(content))
            {
                string originalContent = content;
                content = _reasoningRegex.Replace(content, "").Trim();

                if (content != originalContent)
                {
                    _logger.Debug($"Reasoning filter applied: {originalContent.Length} → {content.Length} chars");
                }
            }

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

                if (attempt < maxRetries)
                {
                    _logger.Debug($"Response that failed to parse: {response[..Math.Min(200, response.Length)]}");
                }
            }

            if (attempt < maxRetries)
            {
                messages.Add(new ConversationMessage
                {
                    Role = MessageRole.User,
                    Content = "Your response was not valid JSON. Please respond with ONLY a JSON object, no other text. Start with { and end with }."
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
        // Patrón mejorado para extraer JSON válido
        string jsonPattern = @"\{(?:[^{}]|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}";
        MatchCollection matches = Regex.Matches(response, jsonPattern, RegexOptions.Singleline);

        if (matches.Count > 0)
        {
            string lastMatch = matches[^1].Value;
            return SanitizeJson(lastMatch); // ← Sanitizar aquí
        }

        // Fallback
        int start = response.IndexOf('{');
        int end = response.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            string json = response[start..(end + 1)];
            return SanitizeJson(json); // ← Y aquí
        }

        // Arrays
        start = response.IndexOf('[');
        end = response.LastIndexOf(']');
        if (start >= 0 && end > start)
        {
            string json = response[start..(end + 1)];
            return SanitizeJson(json); // ← Y aquí
        }

        return response;
    }

    /// <summary>
    /// Sanitiza JSON corrigiendo backslashes sin escapar en valores string.
    /// LLMs a menudo generan rutas de Windows sin escapar correctamente: "C:\path\file.cs"
    /// Esta función las convierte a formato JSON válido: "C:\\path\\file.cs"
    /// </summary>
    private static string SanitizeJson(string json)
    {
        return Regex.Replace(json, @"""([^""]*?)""", match =>
        {
            string content = match.Groups[1].Value;

            // Solo procesar si contiene backslashes
            if (!content.Contains('\\'))
                return match.Value;

            // Proteger backslashes ya escapados
            string temp = content.Replace(@"\\", "\x00DOUBLE\x00");

            // Escapar backslashes simples
            temp = temp.Replace(@"\", @"\\");

            // Restaurar los que ya estaban correctamente escapados
            temp = temp.Replace("\x00DOUBLE\x00", @"\\");

            return $"\"{temp}\"";
        });
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