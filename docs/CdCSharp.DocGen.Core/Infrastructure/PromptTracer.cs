using CdCSharp.DocGen.Core.Abstractions.Infrastructure;
using CdCSharp.DocGen.Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CdCSharp.DocGen.Core.Infrastructure;

public class PromptTracer : IPromptTracer
{
    private readonly ILogger<PromptTracer> _logger;
    private readonly DocGenOptions _options;
    private readonly string _traceOutputPath;
    private int _requestCounter;
    public bool IsEnabled => _options.PromptTracer.Enabled;

    public PromptTracer(ILogger<PromptTracer> logger, IOptions<DocGenOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _traceOutputPath = Path.IsPathRooted(_options.PromptTracer.TraceOutputPath)
            ? _options.PromptTracer.TraceOutputPath
            : Path.Combine(_options.ProjectPath, _options.PromptTracer.TraceOutputPath);
    }

    public async Task TracePromptAsync(string requestId, string prompt, string response)
    {
        if (!IsEnabled)
            return;

        try
        {
            Directory.CreateDirectory(_traceOutputPath);

            int counter = Interlocked.Increment(ref _requestCounter);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{counter:D4}_{timestamp}_{SanitizeFileName(requestId)}.txt";
            string filePath = Path.Combine(_traceOutputPath, fileName);

            string content = BuildTraceContent(requestId, counter, prompt, response);

            await File.WriteAllTextAsync(filePath, content);

            _logger.LogDebug("Trace saved: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save trace for {RequestId}", requestId);
        }
    }

    private static string BuildTraceContent(string requestId, int counter, string prompt, string response)
    {
        string separator = new('=', 80);

        return $"""
            {separator}
            REQUEST #{counter}: {requestId}
            TIMESTAMP: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}
            {separator}

            === PROMPT ===
            Length: {prompt.Length} chars (~{prompt.Length / 4} tokens)

            {prompt}

            {separator}

            === RESPONSE ===
            Length: {response.Length} chars (~{response.Length / 4} tokens)

            {response}

            {separator}
            """;
    }

    private static string SanitizeFileName(string name)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        string sanitized = string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Length > 50 ? sanitized[..50] : sanitized;
    }
}