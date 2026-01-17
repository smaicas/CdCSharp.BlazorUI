using CdCSharp.Theon.Infrastructure;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CdCSharp.Theon.Core;

public interface ILlmClient
{
    Task<LlmResponse> SendAsync(IReadOnlyList<LlmMessage> messages, CancellationToken ct = default);
    Task<ModelInfo> GetModelInfoAsync(CancellationToken ct = default);
    int EstimateTokens(string text);
}

public sealed class LlmClient : ILlmClient, IDisposable
{
    private readonly HttpClient _http;
    private readonly TheonOptions _options;
    private readonly ITheonLogger _logger;
    private readonly Regex? _reasoningRegex;
    private ModelInfo? _cachedModelInfo;

    public LlmClient(TheonOptions options, ITheonLogger logger)
    {
        _options = options;
        _logger = logger;

        _http = new HttpClient
        {
            BaseAddress = new Uri(options.Llm.BaseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(options.Llm.TimeoutSeconds)
        };

        if (!string.IsNullOrWhiteSpace(options.Llm.ReasoningTagPattern))
        {
            try
            {
                _reasoningRegex = new Regex(options.Llm.ReasoningTagPattern, RegexOptions.Compiled | RegexOptions.Singleline);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Invalid reasoning pattern: {ex.Message}");
            }
        }
    }

    public async Task<LlmResponse> SendAsync(IReadOnlyList<LlmMessage> messages, CancellationToken ct = default)
    {
        object[] apiMessages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray();

        _logger.LogLlmRequest(apiMessages);

        object request = new
        {
            messages = apiMessages,
            temperature = _options.Llm.Temperature,
            stream = false
        };

        HttpResponseMessage response = await _http.PostAsJsonAsync("chat/completions", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(ct);
            _logger.Error($"LLM error: {response.StatusCode} - {error}");
            return new LlmResponse(string.Empty, 0, 0);
        }

        ApiResponse? result = await response.Content.ReadFromJsonAsync<ApiResponse>(ct);
        string content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

        if (_reasoningRegex != null && !string.IsNullOrEmpty(content))
        {
            content = _reasoningRegex.Replace(content, "").Trim();
        }

        _logger.LogLlmResponse(content);

        return new LlmResponse(
            content,
            result?.Usage?.PromptTokens ?? 0,
            result?.Usage?.CompletionTokens ?? 0);
    }

    public async Task<ModelInfo> GetModelInfoAsync(CancellationToken ct = default)
    {
        if (_cachedModelInfo != null)
            return _cachedModelInfo;

        try
        {
            HttpResponseMessage response = await _http.GetAsync("models", ct);
            if (response.IsSuccessStatusCode)
            {
                ModelsResponse? result = await response.Content.ReadFromJsonAsync<ModelsResponse>(ct);
                ApiModelInfo? loaded = result?.Data?.FirstOrDefault(m => m.State == "loaded");

                if (loaded != null)
                {
                    _cachedModelInfo = new ModelInfo(loaded.Id, loaded.MaxContextLength);
                    _logger.Info($"Model: {loaded.Id} (context: {loaded.MaxContextLength} tokens)");
                    return _cachedModelInfo;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"Could not get model info: {ex.Message}");
        }

        _cachedModelInfo = new ModelInfo("unknown", 8192);
        return _cachedModelInfo;
    }

    public int EstimateTokens(string text) => text.Length / 4;

    public void Dispose() => _http.Dispose();

    #region API Response Models

    private sealed record ApiResponse(
        [property: JsonPropertyName("choices")] List<ApiChoice>? Choices,
        [property: JsonPropertyName("usage")] ApiUsage? Usage);

    private sealed record ApiChoice(
        [property: JsonPropertyName("message")] ApiMessage? Message);

    private sealed record ApiMessage(
        [property: JsonPropertyName("content")] string Content);

    private sealed record ApiUsage(
        [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
        [property: JsonPropertyName("completion_tokens")] int CompletionTokens);

    private sealed record ModelsResponse(
        [property: JsonPropertyName("data")] List<ApiModelInfo>? Data);

    private sealed record ApiModelInfo(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("state")] string State,
        [property: JsonPropertyName("max_context_length")] int MaxContextLength);

    #endregion
}