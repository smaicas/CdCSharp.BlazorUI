namespace CdCSharp.Theon.Infrastructure;

public interface ITheonLogger
{
    void Info(string message);
    void Debug(string message);
    void Warning(string message);
    void Error(string message, Exception? ex = null);
    void LogLlmRequest(IEnumerable<object> messages);
    void LogLlmResponse(string content);
}

public sealed class TheonLogger : ITheonLogger, IDisposable
{
    private readonly string _logsPath;
    private readonly StreamWriter _llmLogWriter;
    private readonly object _lock = new();
    private int _interactionCount;

    public TheonLogger(TheonOptions options)
    {
        _logsPath = Path.IsPathRooted(options.LogsPath)
            ? options.LogsPath
            : Path.Combine(options.ProjectPath, options.LogsPath);

        Directory.CreateDirectory(_logsPath);

        string llmLogFile = Path.Combine(_logsPath, $"llm_{DateTime.Now:yyyyMMdd_HHmmss}.log");
        _llmLogWriter = new StreamWriter(llmLogFile, append: false) { AutoFlush = true };
    }

    public void Info(string message) => Log("INF", message, ConsoleColor.Cyan);
    public void Debug(string message) => Log("DBG", message, ConsoleColor.Gray);
    public void Warning(string message) => Log("WRN", message, ConsoleColor.Yellow);

    public void Error(string message, Exception? ex = null)
    {
        Log("ERR", message, ConsoleColor.Red);
        if (ex != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"  {ex.GetType().Name}: {ex.Message}");
            Console.ResetColor();
        }
    }

    public void LogLlmRequest(IEnumerable<object> messages)
    {
        int count = Interlocked.Increment(ref _interactionCount);
        lock (_lock)
        {
            _llmLogWriter.WriteLine($"\n{'=',-60}");
            _llmLogWriter.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] REQUEST #{count}");
            _llmLogWriter.WriteLine($"{'=',-60}");
            foreach (object msg in messages)
            {
                _llmLogWriter.WriteLine(System.Text.Json.JsonSerializer.Serialize(msg));
            }
        }
    }

    public void LogLlmResponse(string content)
    {
        lock (_lock)
        {
            _llmLogWriter.WriteLine($"\n{'-',-60}");
            _llmLogWriter.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] RESPONSE");
            _llmLogWriter.WriteLine($"{'-',-60}");
            _llmLogWriter.WriteLine(content);
            _llmLogWriter.WriteLine();
        }
    }

    private void Log(string level, string message, ConsoleColor color)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
        Console.ForegroundColor = color;
        Console.Write($"{level} ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public void Dispose() => _llmLogWriter.Dispose();
}