namespace CdCSharp.Theon.Infrastructure;

public enum LogLevel { Trace, Debug, Info, Warning, Error }
public enum InteractionDirection { Input, Output }
public class TheonLogger
{
    private readonly string _logsPath;
    private readonly string _tracesPath;
    private readonly LogLevel _minLevel;
    private readonly object _lock = new();
    private readonly StreamWriter _logFile;
    private int _promptCounter;

    public TheonLogger(string outputPath, LogLevel minLevel = LogLevel.Info)
    {
        _logsPath = Path.Combine(outputPath, "logs");
        _tracesPath = Path.Combine(outputPath, "traces");
        _minLevel = minLevel;

        Directory.CreateDirectory(_logsPath);
        Directory.CreateDirectory(_tracesPath);

        string logFileName = $"theon_{DateTime.Now:yyyyMMdd_HHmmss}.log";
        _logFile = new StreamWriter(Path.Combine(_logsPath, logFileName), append: true)
        {
            AutoFlush = true
        };

        Log(LogLevel.Info, $"Session started - Log file: {logFileName}");
    }

    public void Trace(string message) => Log(LogLevel.Trace, message);
    public void Debug(string message) => Log(LogLevel.Debug, message);
    public void Info(string message) => Log(LogLevel.Info, message);
    public void Warning(string message) => Log(LogLevel.Warning, message);
    public void Error(string message, Exception? ex = null) => Log(LogLevel.Error, message, ex);

    public void Log(LogLevel level, string message, Exception? ex = null)
    {
        if (level < _minLevel) return;

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string prefix = level switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Info => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            _ => "???"
        };

        string logLine = $"[{timestamp}] [{prefix}] {message}";
        string? exLine = ex != null ? $"  Exception: {ex.GetType().Name}: {ex.Message}" : null;

        lock (_lock)
        {
            _logFile.WriteLine(logLine);
            if (exLine != null) _logFile.WriteLine(exLine);

            ConsoleColor color = level switch
            {
                LogLevel.Trace => ConsoleColor.DarkGray,
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Info => ConsoleColor.Cyan,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ForegroundColor = color;
            Console.Write($"{prefix} ");
            Console.ResetColor();
            Console.WriteLine(message);

            if (ex != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"  {ex.GetType().Name}: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    public void LogOrchestratorInteraction(InteractionDirection direction, string content)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string separator = new('=', 60);

        lock (_lock)
        {
            _logFile.WriteLine();
            _logFile.WriteLine(separator);
            _logFile.WriteLine($"[{timestamp}] ORCHESTRATOR {direction}");
            _logFile.WriteLine(separator);
            _logFile.WriteLine(content);
            _logFile.WriteLine(separator);
            _logFile.WriteLine();
        }
    }

    public void LogAgentInteraction(string agentId, InteractionDirection direction, string content)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        lock (_lock)
        {
            _logFile.WriteLine();
            _logFile.WriteLine($"[{timestamp}] AGENT:{agentId} {direction}");
            _logFile.WriteLine($"Content length: {content.Length} chars");
            _logFile.WriteLine(content);
            //_logFile.WriteLine(content.Length > 2000 ? content[..2000] + "\n... (truncated)" : content);
            _logFile.WriteLine();
        }
    }

    public string TracePrompt(string agentId, string prompt, string? response = null)
    {
        int counter = Interlocked.Increment(ref _promptCounter);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{counter:D4}_{timestamp}_{SanitizeFileName(agentId)}.md";
        string filePath = Path.Combine(_tracesPath, fileName);

        string content = $"""
            # Prompt Trace #{counter}
            
            **Agent:** {agentId}
            **Timestamp:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}
            **Status:** {(response == null ? "PENDING" : "COMPLETED")}
            
            ## Prompt

            {prompt}
            
            ## Response
            
            {(response == null ? "_Waiting for response..._" : response)}
            """;

        File.WriteAllText(filePath, content);
        return filePath;
    }

    public void UpdateTrace(string filePath, string response)
    {
        if (!File.Exists(filePath)) return;

        string content = File.ReadAllText(filePath);
        content = content.Replace("**Status:** PENDING", "**Status:** COMPLETED");
        content = content.Replace("_Waiting for response..._", response);
        File.WriteAllText(filePath, content);
    }

    private static string SanitizeFileName(string name)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
    }

    public void Dispose()
    {
        _logFile.Flush();
        _logFile.Dispose();
    }
}