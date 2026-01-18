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
    //void LogLlmRequest(IReadOnlyList<LlmMessage> messages);
    //void LogLlmResponse(string content, IReadOnlyList<LlmToolCall>? toolCalls = null);
}

public sealed class TheonLogger : ITheonLogger, IDisposable
{
    private readonly string _logsPath;
    private readonly StreamWriter _llmLogWriter;
    private readonly object _lock = new();
    private int _interactionCount;

    public TheonLogger(IOptions<TheonOptions> options)
    {
        _logsPath = Path.IsPathRooted(options.Value.LogsPath)
            ? options.Value.LogsPath
            : Path.Combine(options.Value.ProjectPath, options.Value.LogsPath);

        Directory.CreateDirectory(_logsPath);

        string llmLogFile = Path.Combine(_logsPath, $"llm_{DateTime.Now:yyyyMMdd_HHmmss}.log");
        _llmLogWriter = new StreamWriter(llmLogFile, append: false) { AutoFlush = true };
    }

    public void Info(string message) => Log("INF", message, ConsoleColor.Cyan);
    public void Debug(string message) => Log("DBG", message, ConsoleColor.Gray);
    public void Warning(string message) => Log("WRN", message, ConsoleColor.Yellow);
    public void Success(string message) => Log("SUC", $"✓ {message}", ConsoleColor.Green);

    public void Section(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(title);
        Console.WriteLine(new string('─', title.Length));
        Console.ResetColor();
        Console.WriteLine();
    }

    public void Error(string message, Exception? ex = null)
    {
        Log("ERR", $"✗ {message}", ConsoleColor.Red);
        if (ex != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"  {ex.GetType().Name}: {ex.Message}");
            Console.ResetColor();
        }
    }

    private void Log(string level, string message, ConsoleColor color)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{timestamp}] ");
        Console.ForegroundColor = color;
        Console.Write($"{level} ");
        Console.ResetColor();
        Console.WriteLine(message);
        _llmLogWriter.WriteLine($"[{timestamp}] {level}: {message}");
        _llmLogWriter.Flush();
    }

    public void Dispose() => _llmLogWriter.Dispose();
}