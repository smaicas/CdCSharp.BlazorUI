using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace CdCSharp.DocGen.Cli.Logging;

public sealed class MinimalConsoleFormatter : ConsoleFormatter
{
    public MinimalConsoleFormatter() : base("minimal") { }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        string? message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (string.IsNullOrEmpty(message)) return;

        // Obtener color y prefijo según el nivel
        (ConsoleColor color, string? prefix) = GetColorAndPrefix(logEntry.LogLevel);

        // Escribir con color
        Console.ForegroundColor = color;

        if (!string.IsNullOrEmpty(prefix))
        {
            textWriter.Write(prefix);
            textWriter.Write(" ");
        }

        textWriter.WriteLine(message);
        Console.ResetColor();

        // Mostrar excepción si existe
        if (logEntry.Exception != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            textWriter.WriteLine(logEntry.Exception.ToString());
            Console.ResetColor();
        }
    }

    private static (ConsoleColor color, string prefix) GetColorAndPrefix(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => (ConsoleColor.DarkGray, "›"),
            LogLevel.Debug => (ConsoleColor.Gray, "▸"),
            LogLevel.Information => (ConsoleColor.Cyan, "✓"),
            LogLevel.Warning => (ConsoleColor.Yellow, "⚠"),
            LogLevel.Error => (ConsoleColor.Red, "✗"),
            LogLevel.Critical => (ConsoleColor.Magenta, "‼"),
            _ => (ConsoleColor.White, "•")
        };
    }
}