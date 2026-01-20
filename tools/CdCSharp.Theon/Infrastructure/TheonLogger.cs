using Microsoft.Extensions.Options;

namespace CdCSharp.Theon.Infrastructure;

public interface ITheonLogger
{
    void Info(string message);
    void Debug(string message);
    void Warning(string message);
    void Error(string message, Exception? ex = null);
    void Section(string title);
    void Success(string message);

    void LogToolCall(string contextName, string toolName, string? details = null);
    void LogToolResult(string contextName, string toolName, bool success, string? summary = null);
    void LogContextQuery(string orchestratorOrContext, string targetContext, string question);
    void LogPlanCreated(int stepCount, List<string> taskTypes);
    void LogPlanStep(int stepNumber, int totalSteps, string status, string contextType, string purpose);
    void LogFileOperation(string operation, string path, int? tokens = null);
    void LogLlmCall(string contextName, int messageCount, int? toolCount);
    void LogBudgetStatus(string contextName, int used, int max, float percent);
}

public sealed class TheonLogger : ITheonLogger, IDisposable
{
    private readonly string _logsPath;
    private readonly StreamWriter _llmLogWriter;
    private readonly object _lock = new();
    private int _indentLevel = 0;

    public TheonLogger(IOptions<TheonOptions> options)
    {
        _logsPath = Path.IsPathRooted(options.Value.LogsPath)
            ? options.Value.LogsPath
            : Path.Combine(options.Value.ProjectPath, options.Value.LogsPath);

        Directory.CreateDirectory(_logsPath);

        string llmLogFile = Path.Combine(_logsPath, $"llm_{DateTime.Now:yyyyMMdd_HHmmss}.log");
        _llmLogWriter = new StreamWriter(llmLogFile, append: false) { AutoFlush = true };
    }

    private string Indent => new(' ', _indentLevel * 2);

    public void Info(string message) => Log("INF", message, ConsoleColor.Cyan);
    public void Debug(string message) => Log("DBG", message, ConsoleColor.Gray);
    public void Warning(string message) => Log("WRN", message, ConsoleColor.Yellow);
    public void Success(string message) => Log("SUC", $"{message}", ConsoleColor.Green);

    public void Section(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(title);
        Console.WriteLine(new string('─', Math.Min(title.Length, Console.WindowWidth - 1)));
        Console.ResetColor();
        Console.WriteLine();
    }

    public void Error(string message, Exception? ex = null)
    {
        Log("ERR", $"{message}", ConsoleColor.Red);
        if (ex != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"  {ex.GetType().Name}: {ex.Message}");
            Console.ResetColor();
        }
    }

    public void LogToolCall(string contextName, string toolName, string? details = null)
    {
        _indentLevel++;
        string msg = details != null
            ? $"[{contextName}] 🔧 {toolName}: {details}"
            : $"[{contextName}] 🔧 {toolName}";
        Log("TOOL", msg, ConsoleColor.Magenta);
    }

    public void LogToolResult(string contextName, string toolName, bool success, string? summary = null)
    {
        string icon = success ? "✓" : "✗";
        string msg = summary != null
            ? $"[{contextName}] {icon} {toolName} → {summary}"
            : $"[{contextName}] {icon} {toolName}";

        Log("RSLT", msg, success ? ConsoleColor.Green : ConsoleColor.Red);
        _indentLevel = Math.Max(0, _indentLevel - 1);
    }

    public void LogContextQuery(string source, string targetContext, string question)
    {
        _indentLevel++;
        string preview = question.Length > 60 ? question[..57] + "..." : question;
        Log("QURY", $"[{source}] → [{targetContext}] \"{preview}\"", ConsoleColor.Cyan);
    }

    public void LogPlanCreated(int stepCount, List<string> taskTypes)
    {
        string tasks = string.Join(", ", taskTypes);
        Log("PLAN", $"📋 Created plan: {stepCount} steps | Tasks: {tasks}", ConsoleColor.Yellow);
        _indentLevel++;
    }

    public void LogPlanStep(int stepNumber, int totalSteps, string status, string contextType, string purpose)
    {
        string icon = status switch
        {
            "InProgress" => "→",
            "Completed" => "✓",
            "Failed" => "✗",
            _ => "○"
        };

        string msg = $"{icon} Step {stepNumber}/{totalSteps}: [{contextType}] {purpose}";
        ConsoleColor color = status switch
        {
            "Completed" => ConsoleColor.Green,
            "Failed" => ConsoleColor.Red,
            "InProgress" => ConsoleColor.Yellow,
            _ => ConsoleColor.Gray
        };

        Log("STEP", msg, color);

        if (status == "Completed")
        {
            // Check if this was the last step
            if (stepNumber == totalSteps)
            {
                _indentLevel = Math.Max(0, _indentLevel - 1);
                Log("PLAN", "✓ All plan steps completed", ConsoleColor.Green);
            }
        }
    }

    public void LogFileOperation(string operation, string path, int? tokens = null)
    {
        string tokenInfo = tokens.HasValue ? $" ({tokens.Value:N0} tokens)" : "";
        string msg = $"{operation}: {path}{tokenInfo}";
        Log("FILE", msg, ConsoleColor.DarkCyan);
    }

    public void LogLlmCall(string contextName, int messageCount, int? toolCount)
    {
        string toolInfo = toolCount.HasValue && toolCount > 0
            ? $" | {toolCount} tools available"
            : "";
        Log("LLM ", $"[{contextName}] Calling model ({messageCount} messages{toolInfo})", ConsoleColor.DarkGray);
    }

    public void LogBudgetStatus(string contextName, int used, int max, float percent)
    {
        ConsoleColor color = percent switch
        {
            < 70 => ConsoleColor.Green,
            < 90 => ConsoleColor.Yellow,
            _ => ConsoleColor.Red
        };

        string msg = $"[{contextName}] Budget: {used:N0}/{max:N0} tokens ({percent:F1}%)";
        Log("BDGT", msg, color);
    }

    private void Log(string level, string message, ConsoleColor color)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string indentedMessage = Indent + message;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{timestamp}] ");
        Console.ForegroundColor = color;
        Console.Write($"{level} ");
        Console.ResetColor();
        Console.WriteLine(indentedMessage);

        _llmLogWriter.WriteLine($"[{timestamp}] {level}: {indentedMessage}");
        _llmLogWriter.Flush();
    }

    public void Dispose() => _llmLogWriter.Dispose();
}