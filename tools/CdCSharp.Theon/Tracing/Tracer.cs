using System.Text.Json.Serialization;

namespace CdCSharp.Theon.Tracing;

public static class Tracer
{
    private static readonly object Lock = new();
    private static TracerSession? _session;
    private static readonly AsyncLocal<string?> CurrentSpanId = new();
    private static string _basePath = ".theon";

    public static bool IsActive => _session != null;
    public static string? ActiveSpanId => CurrentSpanId.Value;

    public static void Configure(string basePath)
    {
        lock (Lock)
        {
            _basePath = basePath;
        }
    }

    public static void StartSession(string name)
    {
        lock (Lock)
        {
            _session?.Flush();
            _session = new TracerSession(name, _basePath);
        }
    }

    public static void EndSession(bool success, string? message = null)
    {
        lock (Lock)
        {
            if (_session == null) return;

            Record(new SessionEndEvent(success, message));
            _session.Flush();
            _session = null;
            CurrentSpanId.Value = null;
        }
    }

    public static void FlushSession()
    {
        lock (Lock)
        {
            _session?.Flush();
        }
    }

    public static IDisposable Span(string spanId, string name, string? description = null)
    {
        string? parentId = CurrentSpanId.Value;
        CurrentSpanId.Value = spanId;

        Record(new SpanStartEvent(spanId, parentId, name, description));

        return new SpanScope(spanId, parentId);
    }

    public static void Record(TraceEvent evt)
    {
        lock (Lock)
        {
            _session?.Record(evt, CurrentSpanId.Value);
        }
    }

    private sealed class SpanScope : IDisposable
    {
        private readonly string _spanId;
        private readonly string? _parentId;
        private bool _disposed;

        public SpanScope(string spanId, string? parentId)
        {
            _spanId = spanId;
            _parentId = parentId;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Record(new SpanEndEvent(_spanId));
            CurrentSpanId.Value = _parentId;
        }
    }
}

internal sealed class TracerSession
{
    private readonly string _sessionId;
    private readonly string _name;
    private readonly DateTime _startedAt;
    private readonly string _traceDir;
    private readonly string _eventsDir;
    private readonly List<TraceEventEnvelope> _events = [];
    private int _sequence;

    public TracerSession(string name, string basePath)
    {
        _sessionId = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
        _name = name;
        _startedAt = DateTime.UtcNow;
        _traceDir = Path.Combine(basePath, "traces", _sessionId);
        _eventsDir = Path.Combine(_traceDir, "events");

        Directory.CreateDirectory(_eventsDir);

        Record(new SessionStartEvent(name), null);
    }

    public void Record(TraceEvent evt, string? spanId)
    {
        int seq = Interlocked.Increment(ref _sequence);

        TraceEventEnvelope envelope = new()
        {
            SessionId = _sessionId,
            Sequence = seq,
            Timestamp = DateTime.UtcNow,
            SpanId = spanId,
            EventType = evt.GetType().Name.Replace("Event", ""),
            Data = evt
        };

        _events.Add(envelope);
        WriteEventFile(envelope);
    }

    public void Flush()
    {
        TraceDocument doc = new()
        {
            SessionId = _sessionId,
            Name = _name,
            StartedAt = _startedAt,
            EndedAt = DateTime.UtcNow,
            Events = _events.ToList()
        };

        string jsonPath = Path.Combine(_traceDir, "trace.json");
        File.WriteAllText(jsonPath, TraceSerializer.ToJson(doc));

        string htmlPath = Path.Combine(_traceDir, "trace.html");
        File.WriteAllText(htmlPath, TraceSerializer.ToHtml(doc));
    }

    private void WriteEventFile(TraceEventEnvelope envelope)
    {
        string baseName = $"{envelope.Sequence:D4}_{envelope.EventType}";

        string jsonPath = Path.Combine(_eventsDir, $"{baseName}.json");
        File.WriteAllText(jsonPath, TraceSerializer.ToJson(envelope));

        string htmlPath = Path.Combine(_eventsDir, $"{baseName}.html");
        File.WriteAllText(htmlPath, TraceSerializer.ToHtml(envelope));
    }
}

public sealed class TraceEventEnvelope
{
    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }

    [JsonPropertyName("sequence")]
    public required int Sequence { get; init; }

    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; init; }

    [JsonPropertyName("spanId")]
    public string? SpanId { get; init; }

    [JsonPropertyName("eventType")]
    public required string EventType { get; init; }

    [JsonPropertyName("data")]
    public required object Data { get; init; }
}

public sealed class TraceDocument
{
    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("startedAt")]
    public required DateTime StartedAt { get; init; }

    [JsonPropertyName("endedAt")]
    public required DateTime EndedAt { get; init; }

    [JsonPropertyName("events")]
    public required List<TraceEventEnvelope> Events { get; init; }
}

#region Events

public abstract record TraceEvent;

public sealed record SessionStartEvent(string Name) : TraceEvent;

public sealed record SessionEndEvent(bool Success, string? Message) : TraceEvent;

public sealed record SpanStartEvent(
    string SpanId,
    string? ParentSpanId,
    string Name,
    string? Description) : TraceEvent;

public sealed record SpanEndEvent(string SpanId) : TraceEvent;

public sealed record LlmRequestEvent(
    string Model,
    int MessageCount,
    int ToolCount,
    string? FirstUserMessage) : TraceEvent;

public sealed record LlmResponseEvent(
    string? FinishReason,
    string? Content,
    int? ToolCallCount,
    double DurationMs) : TraceEvent;

public sealed record ToolCallEvent(
    string ToolName,
    string Arguments) : TraceEvent;

public sealed record ToolResultEvent(
    string ToolName,
    string Result,
    bool IsError,
    double DurationMs) : TraceEvent;

public sealed record FileReadEvent(
    string Path,
    int SizeBytes,
    int EstimatedTokens,
    bool IsEphemeral) : TraceEvent;

public sealed record ErrorEvent(
    string ErrorType,
    string Message,
    string? StackTrace) : TraceEvent;

#endregion