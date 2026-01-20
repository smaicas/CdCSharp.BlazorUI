using System.Text;
using System.Text.Json;

namespace CdCSharp.Theon.Tracing;

internal static class TraceSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string ToJson(object obj) => JsonSerializer.Serialize(obj, JsonOptions);

    public static string ToHtml(TraceEventEnvelope envelope)
    {
        StringBuilder sb = new();
        sb.AppendLine("<!DOCTYPE html><html><head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine($"<title>#{envelope.Sequence} {envelope.EventType}</title>");
        sb.AppendLine(Styles);
        sb.AppendLine("</head><body>");

        sb.AppendLine($"<div class='header'>");
        sb.AppendLine($"<h1>#{envelope.Sequence} {envelope.EventType}</h1>");
        sb.AppendLine($"<div class='meta'>Session: {envelope.SessionId}</div>");
        sb.AppendLine($"<div class='meta'>Span: {envelope.SpanId ?? "(root)"}</div>");
        sb.AppendLine($"<div class='meta'>Time: {envelope.Timestamp:HH:mm:ss.fff}</div>");
        sb.AppendLine("</div>");

        sb.AppendLine(RenderEventData(envelope.EventType, envelope.Data));

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    public static string ToHtml(TraceDocument doc)
    {
        StringBuilder sb = new();
        sb.AppendLine("<!DOCTYPE html><html><head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine($"<title>Trace: {Encode(doc.Name)}</title>");
        sb.AppendLine(Styles);
        sb.AppendLine("</head><body>");

        sb.AppendLine("<div class='header'>");
        sb.AppendLine($"<h1>{Encode(doc.Name)}</h1>");
        sb.AppendLine($"<div class='meta'>Session: {doc.SessionId}</div>");
        sb.AppendLine($"<div class='meta'>Started: {doc.StartedAt:yyyy-MM-dd HH:mm:ss}</div>");
        sb.AppendLine($"<div class='meta'>Duration: {(doc.EndedAt - doc.StartedAt).TotalSeconds:F2}s</div>");
        sb.AppendLine("</div>");

        int llmRequests = doc.Events.Count(e => e.EventType == "LlmRequest");
        int toolCalls = doc.Events.Count(e => e.EventType == "ToolCall");
        int errors = doc.Events.Count(e => e.EventType == "Error");

        sb.AppendLine("<div class='summary'>");
        sb.AppendLine($"<strong>Summary:</strong> {doc.Events.Count} events | {llmRequests} LLM calls | {toolCalls} tool calls | {errors} errors");
        sb.AppendLine("</div>");

        Dictionary<string, int> spanDepths = [];
        foreach (TraceEventEnvelope evt in doc.Events)
        {
            int depth = 0;
            if (evt.SpanId != null && spanDepths.TryGetValue(evt.SpanId, out int d))
                depth = d;

            if (evt.Data is SpanStartEvent spanStart)
            {
                int parentDepth = spanStart.ParentSpanId != null && spanDepths.TryGetValue(spanStart.ParentSpanId, out int pd) ? pd : 0;
                spanDepths[spanStart.SpanId] = parentDepth + 1;
                depth = parentDepth + 1;
            }

            string marginStyle = $"margin-left: {depth * 20}px;";
            string cssClass = GetCssClass(evt.EventType);

            sb.AppendLine($"<div class='{cssClass}' style='{marginStyle}'>");
            sb.AppendLine("<div class='event-header'>");
            sb.AppendLine($"<span class='event-type'>{evt.EventType}</span>");
            sb.AppendLine($"<span class='event-meta'>#{evt.Sequence} | {evt.Timestamp:HH:mm:ss.fff} | {evt.SpanId ?? "root"}</span>");
            sb.AppendLine("</div>");
            sb.AppendLine(RenderEventData(evt.EventType, evt.Data));
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private static string GetCssClass(string eventType) => eventType switch
    {
        "Error" => "event error",
        "SessionEnd" => "event session",
        "SessionStart" => "event session",
        "SpanStart" or "SpanEnd" => "event span",
        "LlmRequest" or "LlmResponse" => "event llm",
        "ToolCall" or "ToolResult" => "event tool",
        "FileRead" => "event file",
        _ => "event"
    };

    private static string RenderEventData(string eventType, object data)
    {
        string json = JsonSerializer.Serialize(data, JsonOptions);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            StringBuilder sb = new();
            sb.AppendLine("<div class='event-data'>");

            foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
            {
                string value = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => Encode(prop.Value.GetString() ?? ""),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => "<em>null</em>",
                    JsonValueKind.Number => prop.Value.ToString(),
                    _ => $"<pre>{Encode(prop.Value.ToString())}</pre>"
                };

                bool isLongText = prop.Value.ValueKind == JsonValueKind.String
                    && (prop.Value.GetString()?.Length ?? 0) > 200;

                if (isLongText)
                {
                    sb.AppendLine($"<details><summary><strong>{prop.Name}</strong></summary><pre>{value}</pre></details>");
                }
                else
                {
                    sb.AppendLine($"<div><strong>{prop.Name}:</strong> {value}</div>");
                }
            }

            sb.AppendLine("</div>");
            return sb.ToString();
        }
        catch
        {
            return $"<pre>{Encode(json)}</pre>";
        }
    }

    private static string Encode(string? text) => System.Net.WebUtility.HtmlEncode(text ?? "");

    private const string Styles = """
        <style>
            body { font-family: system-ui, -apple-system, sans-serif; margin: 20px; background: #1a1a2e; color: #eee; }
            .header { background: #16213e; padding: 20px; border-radius: 8px; margin-bottom: 20px; }
            .header h1 { margin: 0 0 10px 0; color: #0f9; }
            .meta { opacity: 0.7; font-size: 0.9em; }
            .summary { background: #0f3460; padding: 12px; border-radius: 6px; margin-bottom: 20px; }
            .event { background: #16213e; border-radius: 6px; padding: 12px; margin-bottom: 8px; border-left: 4px solid #444; }
            .event.error { border-left-color: #e74c3c; }
            .event.session { border-left-color: #9b59b6; }
            .event.span { border-left-color: #3498db; }
            .event.llm { border-left-color: #f39c12; }
            .event.tool { border-left-color: #2ecc71; }
            .event.file { border-left-color: #1abc9c; }
            .event-header { display: flex; justify-content: space-between; margin-bottom: 8px; }
            .event-type { font-weight: bold; color: #0f9; }
            .event-meta { font-size: 0.8em; opacity: 0.6; }
            .event-data { font-size: 0.9em; }
            .event-data > div { margin: 4px 0; }
            pre { background: #0d1b2a; padding: 10px; border-radius: 4px; overflow-x: auto; white-space: pre-wrap; margin: 5px 0; }
            details summary { cursor: pointer; color: #3498db; }
        </style>
        """;
}