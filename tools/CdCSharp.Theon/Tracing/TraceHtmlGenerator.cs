using System.Text;
using System.Web;

namespace CdCSharp.Theon.Tracing;

public static class TraceHtmlGenerator
{
    public static string Generate(ExecutionTrace trace)
    {
        StringBuilder sb = new();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"  <title>Trace {trace.Id}</title>");
        sb.AppendLine(GetStyles());
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        sb.AppendLine(RenderHeader(trace));
        sb.AppendLine("<div class=\"container\">");
        sb.AppendLine(RenderUserInput(trace));
        sb.AppendLine(RenderOrchestrator(trace.Orchestrator));
        sb.AppendLine(RenderResult(trace.Result));
        sb.AppendLine("</div>");

        sb.AppendLine(GetScripts());
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static string GetScripts() => """
<script>
  document.querySelectorAll('.section-header').forEach(header => {
    header.addEventListener('click', () => {
      header.parentElement.classList.toggle('expanded');
    });
  });

  document.querySelectorAll('.llm-header, .tool-header').forEach(header => {
    header.addEventListener('click', (e) => {
      e.stopPropagation();
      header.parentElement.classList.toggle('expanded');
    });
  });

  document.querySelectorAll('.section').forEach(s => s.classList.add('expanded'));
</script>
""";

    private static string RenderHeader(ExecutionTrace trace) => $"""
<div class="header">
  <h1>🔍 Execution Trace</h1>
  <div class="stats">
    <div class="stat">
      <div class="stat-value">{trace.DurationMs}ms</div>
      <div class="stat-label">Duration</div>
    </div>
    <div class="stat">
      <div class="stat-value">{trace.TotalTokens:N0}</div>
      <div class="stat-label">Total Tokens</div>
    </div>
    <div class="stat">
      <div class="stat-value">{trace.TotalLlmCalls}</div>
      <div class="stat-label">LLM Calls</div>
    </div>
    <div class="stat">
      <div class="stat-value">{trace.Orchestrator.ToolExecutions.Count}</div>
      <div class="stat-label">Tool Executions</div>
    </div>
  </div>
  <div style="margin-top:12px;color:var(--text-secondary);font-size:.85rem">
    ID: {trace.Id} | {trace.Timestamp:yyyy-MM-dd HH:mm:ss} UTC
  </div>
</div>
""";

    private static string RenderUserInput(ExecutionTrace trace) => $"""
<div class="section expanded">
  <div class="section-header">
    <div class="section-icon icon-user">👤</div>
    <div class="section-title">User Input</div>
  </div>
  <div class="section-content">
    <div class="message user">
      <div class="message-content">{Encode(trace.UserInput)}</div>
    </div>
  </div>
</div>
""";

    private static string RenderOrchestrator(OrchestratorTrace orchestrator)
    {
        StringBuilder sb = new();

        sb.AppendLine("""
<div class="section expanded">
  <div class="section-header">
    <div class="section-icon icon-orchestrator">🎭</div>
    <div class="section-title">Orchestrator</div>
    <span class="chevron">▶</span>
  </div>
  <div class="section-content">
""");

        int toolIndex = 0;
        foreach (LlmCallTrace llmCall in orchestrator.LlmCalls)
        {
            sb.AppendLine(RenderLlmCall(llmCall));

            if (llmCall.Response.ToolCalls != null)
            {
                foreach (ToolCallTrace _ in llmCall.Response.ToolCalls)
                {
                    if (toolIndex < orchestrator.ToolExecutions.Count)
                    {
                        sb.AppendLine(RenderToolExecution(orchestrator.ToolExecutions[toolIndex]));
                        toolIndex++;
                    }
                }
            }
        }

        sb.AppendLine("</div></div>");
        return sb.ToString();
    }

    private static string RenderLlmCall(LlmCallTrace call)
    {
        StringBuilder sb = new();

        string tokensInfo = call.Response.Tokens != null
            ? $"<span class=\"badge badge-tokens\">{call.Response.Tokens.Total} tokens</span>"
            : "";

        sb.AppendLine($"""
<div class="llm-call">
  <div class="llm-header">
    <div class="section-icon icon-llm">🤖</div>
    <div style="flex:1">
      <strong>LLM Call #{call.Index + 1}</strong>
      <span style="color:var(--text-secondary);margin-left:8px">{call.Request.Model}</span>
    </div>
    <span class="badge badge-duration">{call.DurationMs}ms</span>
    {tokensInfo}
    <span class="chevron">▶</span>
  </div>
  <div class="llm-content">
    <h4 style="margin-bottom:12px;color:var(--accent-blue)">Request ({call.Request.MessageCount} messages)</h4>
""");

        foreach (MessageTrace msg in call.Request.Messages)
            sb.AppendLine(RenderMessage(msg));

        sb.AppendLine($"""
    <h4 style="margin:16px 0 12px;color:var(--accent-green)">Response</h4>
    <div class="message assistant">
      <div class="message-role">Finish: {call.Response.FinishReason}</div>
      <div class="message-content">{Encode(call.Response.Content)}</div>
    </div>
""");

        if (call.Response.ToolCalls?.Count > 0)
        {
            sb.AppendLine("<div class=\"tool-calls\"><strong>Tool Calls:</strong>");
            foreach (ToolCallTrace tc in call.Response.ToolCalls)
            {
                sb.AppendLine($"""
<div class="tool-call-item">
  <span class="tool-name">{tc.Name}</span>
  <div class="tool-args">{Encode(tc.Arguments)}</div>
</div>
""");
            }
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</div></div>");
        return sb.ToString();
    }

    private static string RenderToolExecution(ToolExecutionTrace tool)
    {
        StringBuilder sb = new();

        string errorBadge = tool.IsError ? "<span class=\"badge badge-error\">ERROR</span>" : "";
        string iconClass = tool.IsError ? "icon-error" : "icon-tool";

        sb.AppendLine($"""
<div class="tool-execution">
  <div class="tool-header">
    <div class="section-icon {iconClass}">🔧</div>
    <div style="flex:1"><strong>{tool.ToolName}</strong></div>
    <span class="badge badge-duration">{tool.DurationMs}ms</span>
    {errorBadge}
    <span class="chevron">▶</span>
  </div>
  <div class="tool-content">
    <strong>Arguments:</strong>
    <pre>{Encode(tool.Arguments)}</pre>
    <strong>Result:</strong>
    <pre>{Encode(tool.Content)}</pre>
""");

        if (tool.ContextTrace != null)
            sb.AppendLine(RenderContextTrace(tool.ContextTrace));

        sb.AppendLine("</div></div>");
        return sb.ToString();
    }

    private static string RenderContextTrace(ContextTrace ctx)
    {
        StringBuilder sb = new();

        sb.AppendLine($"""
<div class="context-trace">
  <div class="context-header">📦 Context: {ctx.ContextName}</div>
  <div>Question: {Encode(ctx.Question)}</div>
""");

        foreach (LlmCallTrace llmCall in ctx.LlmCalls)
            sb.AppendLine(RenderLlmCall(llmCall));

        foreach (ContextTrace delegated in ctx.DelegatedContexts)
            sb.AppendLine(RenderContextTrace(delegated));

        sb.AppendLine("</div>");
        return sb.ToString();
    }

    private static string RenderMessage(MessageTrace msg)
    {
        string roleClass = msg.Role.ToLowerInvariant();
        string toolInfo = msg.ToolCallId != null ? $" (tool_call_id: {msg.ToolCallId})" : "";

        return $"""
<div class="message {roleClass}">
  <div class="message-role">{msg.Role}{toolInfo}</div>
  <div class="message-content">{Encode(msg.Content)}</div>
</div>
""";
    }

    private static string RenderResult(ExecutionResult result)
    {
        string icon = result.Success ? "✅" : "❌";
        string cls = result.Success ? "icon-result" : "icon-error";

        return $"""
<div class="section expanded">
  <div class="section-header">
    <div class="section-icon {cls}">{icon}</div>
    <div class="section-title">Result</div>
  </div>
  <div class="section-content">
    <div class="message assistant">
      <div class="message-content">{Encode(result.MessagePreview)}</div>
    </div>
  </div>
</div>
""";
    }

    private static string Encode(string? text) =>
        HttpUtility.HtmlEncode(text ?? string.Empty);

    private static string GetStyles() => """
<style>
:root {
  --bg-primary: #0f172a;
  --bg-secondary: #111827;
  --bg-tertiary: #1f2933;

  --text-primary: #e5e7eb;
  --text-secondary: #9ca3af;

  --border-color: #2b3648;

  --accent-blue: #38bdf8;
  --accent-green: #4ade80;
  --accent-yellow: #facc15;
  --accent-red: #f87171;
  --accent-purple: #c084fc;
}

* {
  box-sizing: border-box;
}

body {
  margin: 0;
  padding: 24px;
  background: var(--bg-primary);
  color: var(--text-primary);
  font-family: system-ui, -apple-system, Segoe UI, Roboto, sans-serif;
  line-height: 1.5;
}

h1, h2, h3, h4 {
  margin: 0;
}

pre {
  white-space: pre-wrap;
  word-break: break-word;
}

.container {
  max-width: 1100px;
  margin: 0 auto;
}

/* ================= HEADER ================= */

.header {
  background: linear-gradient(135deg, #020617, #020617);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 24px;
}

.header h1 {
  font-size: 1.5rem;
  margin-bottom: 12px;
}

.stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 12px;
}

.stat {
  background: var(--bg-secondary);
  border-radius: 8px;
  padding: 12px;
  text-align: center;
}

.stat-value {
  font-size: 1.2rem;
  font-weight: 600;
}

.stat-label {
  font-size: 0.75rem;
  color: var(--text-secondary);
}

/* ================= SECTIONS ================= */

.section {
  background: var(--bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: 10px;
  margin-bottom: 16px;
  overflow: hidden;
}

.section-header {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 14px;
  cursor: pointer;
  user-select: none;
  background: var(--bg-tertiary);
}

.section-title {
  font-weight: 600;
}

.section-content {
  padding: 14px;
  display: none;
}

.section.expanded > .section-content {
  display: block;
}

.section-icon {
  width: 28px;
  height: 28px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1rem;
}

/* ================= ICON COLORS ================= */

.icon-user { background: #1d4ed8; }
.icon-orchestrator { background: #7c3aed; }
.icon-llm { background: #0ea5e9; }
.icon-tool { background: #16a34a; }
.icon-result { background: #22c55e; }
.icon-error { background: #dc2626; }

/* ================= BADGES ================= */

.badge {
  padding: 2px 8px;
  border-radius: 999px;
  font-size: 0.7rem;
  font-weight: 600;
  margin-left: 6px;
  white-space: nowrap;
}

.badge-duration {
  background: #1e3a8a;
  color: #bfdbfe;
}

.badge-tokens {
  background: #064e3b;
  color: #6ee7b7;
}

.badge-error {
  background: #7f1d1d;
  color: #fecaca;
}

/* ================= MESSAGES ================= */

.message {
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 10px 12px;
  margin-bottom: 10px;
}

.message-role {
  font-size: 0.75rem;
  color: var(--text-secondary);
  margin-bottom: 4px;
}

.message.user {
  background: #020617;
  border-left: 4px solid var(--accent-blue);
}

.message.assistant {
  background: #020617;
  border-left: 4px solid var(--accent-green);
}

.message.system {
  background: #020617;
  border-left: 4px solid var(--accent-yellow);
}

/* ================= LLM CALL ================= */

.llm-call {
  border: 1px solid var(--border-color);
  border-radius: 8px;
  margin-bottom: 12px;
  overflow: hidden;
}

.llm-header {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  background: #020617;
  cursor: pointer;
}

.llm-content {
  display: none;
  padding: 12px;
}

.llm-call.expanded > .llm-content {
  display: block;
}

/* ================= TOOL EXECUTION ================= */

.tool-execution {
  border: 1px dashed var(--border-color);
  border-radius: 8px;
  margin: 12px 0;
}

.tool-header {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  background: #020617;
  cursor: pointer;
}

.tool-content {
  display: none;
  padding: 12px;
}

.tool-execution.expanded > .tool-content {
  display: block;
}

/* ================= CONTEXT ================= */

.context-trace {
  margin-top: 14px;
  padding: 12px;
  border-radius: 8px;
  background: #020617;
  border: 1px solid var(--border-color);
}

.context-header {
  font-weight: 600;
  margin-bottom: 6px;
}

/* ================= FILE LIST ================= */

.file-list {
  margin: 6px 0 0;
  padding-left: 18px;
}

.file-list li {
  margin-bottom: 4px;
  font-size: 0.85rem;
}

/* ================= CHEVRON ================= */

.chevron {
  margin-left: auto;
  transition: transform 0.15s ease;
  opacity: 0.6;
}

.expanded > .section-header .chevron,
.expanded > .llm-header .chevron,
.expanded > .tool-header .chevron {
  transform: rotate(90deg);
}

.badge-delegation {
  background: #581c87;
  color: #e9d5ff;
}

</style>
""";

}