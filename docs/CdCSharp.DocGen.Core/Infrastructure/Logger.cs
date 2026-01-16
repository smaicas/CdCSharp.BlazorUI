namespace CdCSharp.DocGen.Core.Infrastructure;

public interface ILogger
{
    void Info(string message);
    void Success(string message);
    void Warning(string message);
    void Error(string message);
    void Verbose(string message);
    void Progress(string message);
}

public class ConsoleLogger : ILogger
{
    private readonly bool _verbose;
    private readonly object _lock = new();

    public ConsoleLogger(bool verbose = false)
    {
        _verbose = verbose;
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
}