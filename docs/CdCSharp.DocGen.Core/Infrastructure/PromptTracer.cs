using CdCSharp.DocGen.Core.Abstractions.Infrastructure;
using CdCSharp.DocGen.Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace CdCSharp.DocGen.Core.Infrastructure;

public class PromptTracer : IPromptTracer
{
    private readonly ILogger<PromptTracer> _logger;
    private readonly DocGenOptions _options;
    private readonly string _traceOutputPath;
    private int _requestCounter;

    private readonly ConcurrentDictionary<string, TraceInfo> _activeTraces = new();

    public bool IsEnabled => _options.PromptTracer.Enabled;

    public PromptTracer(ILogger<PromptTracer> logger, IOptions<DocGenOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _traceOutputPath = Path.IsPathRooted(_options.PromptTracer.TraceOutputPath)
            ? _options.PromptTracer.TraceOutputPath
            : Path.Combine(_options.ProjectPath, _options.PromptTracer.TraceOutputPath);
    }

    public async Task<string> TracePromptStartAsync(string agentId, string prompt)
    {
        if (!IsEnabled)
            return string.Empty;

        try
        {
            Directory.CreateDirectory(_traceOutputPath);

            int counter = Interlocked.Increment(ref _requestCounter);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string traceId = $"{counter:D4}_{timestamp}";
            string fileName = $"{traceId}_{SanitizeFileName(agentId)}.txt";
            string filePath = Path.Combine(_traceOutputPath, fileName);

            TraceInfo info = new()
            {
                TraceId = traceId,
                AgentId = agentId,
                FilePath = filePath,
                StartTime = DateTime.Now,
                Prompt = prompt
            };

            _activeTraces[traceId] = info;

            string initialContent = BuildInitialTraceContent(counter, agentId, prompt);
            await File.WriteAllTextAsync(filePath, initialContent);

            _logger.LogDebug("Trace started: {TraceId} → {FilePath}", traceId, filePath);

            return traceId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to start trace for {AgentId}", agentId);
            return string.Empty;
        }
    }

    public async Task TracePromptCompleteAsync(string traceId, string response)
    {
        if (!IsEnabled || string.IsNullOrEmpty(traceId))
            return;

        if (!_activeTraces.TryGetValue(traceId, out TraceInfo? info))
        {
            _logger.LogWarning("Trace {TraceId} not found", traceId);
            return;
        }

        try
        {
            TimeSpan elapsed = DateTime.Now - info.StartTime;

            string completeContent = BuildCompleteTraceContent(info, response, elapsed);
            await File.WriteAllTextAsync(info.FilePath, completeContent);

            _activeTraces.TryRemove(traceId, out _);

            _logger.LogDebug("Trace completed: {TraceId} ({Elapsed:F2}s)",
                traceId, elapsed.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to complete trace {TraceId}", traceId);
        }
    }

    public async Task TracePromptFailureAsync(string traceId, Exception ex)
    {
        if (!IsEnabled || string.IsNullOrEmpty(traceId))
            return;

        if (!_activeTraces.TryGetValue(traceId, out TraceInfo? info))
        {
            _logger.LogWarning("Trace {TraceId} not found", traceId);
            return;
        }

        try
        {
            TimeSpan elapsed = DateTime.Now - info.StartTime;

            string failureContent = BuildFailureTraceContent(info, ex, elapsed);
            await File.WriteAllTextAsync(info.FilePath, failureContent);

            _activeTraces.TryRemove(traceId, out _);

            _logger.LogDebug("Trace failed: {TraceId} ({Elapsed:F2}s)",
                traceId, elapsed.TotalSeconds);
        }
        catch (Exception traceEx)
        {
            _logger.LogWarning(traceEx, "Failed to mark trace {TraceId} as failed", traceId);
        }
    }

    private static string BuildInitialTraceContent(int counter, string agentId, string prompt)
    {
        string separator = new('=', 80);

        return $"""
            {separator}
            TRACE #{counter:D4}
            AGENT: {agentId}
            STATUS: PENDING RESPONSE
            STARTED: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}
            {separator}

            === PROMPT ===
            Length: {prompt.Length} chars (~{prompt.Length / 4} tokens)

            {prompt}

            {separator}
            WAITING FOR RESPONSE...
            {separator}
            """;
    }

    private static string BuildCompleteTraceContent(TraceInfo info, string response, TimeSpan elapsed)
    {
        string separator = new('=', 80);

        return $"""
            {separator}
            TRACE: {info.TraceId}
            AGENT: {info.AgentId}
            STATUS: COMPLETED
            STARTED: {info.StartTime:yyyy-MM-dd HH:mm:ss.fff}
            COMPLETED: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}
            ELAPSED: {elapsed.TotalSeconds:F2}s
            {separator}

            === PROMPT ===
            Length: {info.Prompt.Length} chars (~{info.Prompt.Length / 4} tokens)

            {info.Prompt}

            {separator}

            === RESPONSE ===
            Length: {response.Length} chars (~{response.Length / 4} tokens)

            {response}

            {separator}
            """;
    }

    private static string BuildFailureTraceContent(TraceInfo info, Exception ex, TimeSpan elapsed)
    {
        string separator = new('=', 80);

        return $"""
            {separator}
            TRACE: {info.TraceId}
            AGENT: {info.AgentId}
            STATUS: FAILED
            STARTED: {info.StartTime:yyyy-MM-dd HH:mm:ss.fff}
            FAILED: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}
            ELAPSED: {elapsed.TotalSeconds:F2}s
            {separator}

            === PROMPT ===
            Length: {info.Prompt.Length} chars (~{info.Prompt.Length / 4} tokens)

            {info.Prompt}

            {separator}

            === ERROR ===
            Type: {ex.GetType().Name}
            Message: {ex.Message}

            StackTrace:
            {ex.StackTrace}

            {separator}
            """;
    }

    private static string SanitizeFileName(string name)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        string sanitized = string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Length > 50 ? sanitized[..50] : sanitized;
    }

    private sealed class TraceInfo
    {
        public required string TraceId { get; init; }
        public required string AgentId { get; init; }
        public required string FilePath { get; init; }
        public required DateTime StartTime { get; init; }
        public required string Prompt { get; init; }
    }
}