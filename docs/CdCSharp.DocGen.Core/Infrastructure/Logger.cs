namespace CdCSharp.DocGen.Core.Infrastructure;

public interface ILogger
{
    void Info(string message);
    void Success(string message);
    void Warning(string message);
    void Error(string message);
    void Verbose(string message);
    void Progress(string message);
    void Trace(string message);
    void TracePrompt(string promptName, string content);
    void TraceResponse(string promptName, string content);
}

public class ConsoleLogger : ILogger
{
    private readonly bool _verbose;
    private readonly bool _trace;
    private readonly object _lock = new();
    private int _promptCounter = 0;

    public ConsoleLogger(bool verbose = false, bool trace = false)
    {
        _verbose = verbose;
        _trace = trace;
    }

    public void Info(string message) => Write(message, ConsoleColor.White);

    public void Success(string message) => Write($"✓ {message}", ConsoleColor.Green);

    public void Warning(string message) => Write($"⚠ {message}", ConsoleColor.Yellow);

    public void Error(string message) => Write($"✗ {message}", ConsoleColor.Red);

    public void Verbose(string message)
    {
        if (_verbose)
            Write($"  {message}", ConsoleColor.DarkGray);
    }

    public void Progress(string message) => Write($"→ {message}", ConsoleColor.Cyan);

    public void Trace(string message)
    {
        if (_trace)
            Write($"🔍 {message}", ConsoleColor.Magenta);
    }

    public void TracePrompt(string promptName, string content)
    {
        if (!_trace) return;

        int counter = Interlocked.Increment(ref _promptCounter);
        string separator = new('=', 80);

        Write($"\n{separator}", ConsoleColor.DarkCyan);
        Write($"📤 PROMPT #{counter}: {promptName}", ConsoleColor.Cyan);
        Write(separator, ConsoleColor.DarkCyan);
        Write($"Length: {content.Length} chars (~{content.Length / 4} tokens)", ConsoleColor.DarkGray);
        Write(separator, ConsoleColor.DarkCyan);
        Write(TruncateForDisplay(content, 2000), ConsoleColor.Gray);
        Write($"{separator}\n", ConsoleColor.DarkCyan);
    }

    public void TraceResponse(string promptName, string content)
    {
        if (!_trace) return;

        string separator = new('=', 80);

        Write($"\n{separator}", ConsoleColor.DarkGreen);
        Write($"📥 RESPONSE: {promptName}", ConsoleColor.Green);
        Write(separator, ConsoleColor.DarkGreen);
        Write($"Length: {content.Length} chars (~{content.Length / 4} tokens)", ConsoleColor.DarkGray);
        Write(separator, ConsoleColor.DarkGreen);
        Write(TruncateForDisplay(content, 2000), ConsoleColor.Gray);
        Write($"{separator}\n", ConsoleColor.DarkGreen);
    }

    private void Write(string message, ConsoleColor color)
    {
        lock (_lock)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = original;
        }
    }

    private static string TruncateForDisplay(string content, int maxLength)
    {
        if (content.Length <= maxLength)
            return content;

        int halfLength = maxLength / 2;
        return content[..halfLength] +
               $"\n\n... [TRUNCATED {content.Length - maxLength} chars] ...\n\n" +
               content[^halfLength..];
    }
}

public class NullLogger : ILogger
{
    public static NullLogger Instance { get; } = new();

    public void Info(string message) { }
    public void Success(string message) { }
    public void Warning(string message) { }
    public void Error(string message) { }
    public void Verbose(string message) { }
    public void Progress(string message) { }
    public void Trace(string message) { }
    public void TracePrompt(string promptName, string content) { }
    public void TraceResponse(string promptName, string content) { }
}