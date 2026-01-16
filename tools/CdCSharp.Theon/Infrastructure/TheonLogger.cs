namespace CdCSharp.Theon.Infrastructure;

public enum LogLevel { Trace, Debug, Info, Warning, Error }

public class TheonLogger
{
    private readonly string _outputPath;
    private readonly LogLevel _minLevel;
    private readonly object _lock = new();
    private int _promptCounter;

    public TheonLogger(string outputPath, LogLevel minLevel = LogLevel.Info)
    {
        _outputPath = outputPath;
        _minLevel = minLevel;
        Directory.CreateDirectory(Path.Combine(_outputPath, "logs"));
        Directory.CreateDirectory(Path.Combine(_outputPath, "traces"));
    }

    public void Trace(string message) => Log(LogLevel.Trace, message);
    public void Debug(string message) => Log(LogLevel.Debug, message);
    public void Info(string message) => Log(LogLevel.Info, message);
    public void Warning(string message) => Log(LogLevel.Warning, message);
    public void Error(string message, Exception? ex = null) => Log(LogLevel.Error, message, ex);

    public void Log(LogLevel level, string message, Exception? ex = null)
    {
        if (level < _minLevel) return;

        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string prefix = level switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Info => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            _ => "???"
        };

        ConsoleColor color = level switch
        {
            LogLevel.Trace => ConsoleColor.DarkGray,
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Info => ConsoleColor.Cyan,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => ConsoleColor.White
        };

        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{timestamp}] ");
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

    public string TracePrompt(string agentId, string prompt, string? response = null)
    {
        int counter = Interlocked.Increment(ref _promptCounter);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{counter:D4}_{timestamp}_{SanitizeFileName(agentId)}.md";
        string filePath = Path.Combine(_outputPath, "traces", fileName);

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
}