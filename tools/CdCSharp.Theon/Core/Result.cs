using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.Theon.Core;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T? value, Error? error)
    {
        _value = value;
        _error = error;
    }

    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_error))]
    public bool IsSuccess => _error == null;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access value of failed result: {_error!.Message}");

    public Error Error => IsSuccess
        ? throw new InvalidOperationException("Cannot access error of successful result")
        : _error!;

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Error error) => new(default, error);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(_error!);

    public async Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onSuccess,
        Func<Error, Task<TResult>> onFailure) =>
        IsSuccess ? await onSuccess(_value!) : await onFailure(_error!);

    public Result<TNew> Map<TNew>(Func<T, TNew> mapper) =>
        IsSuccess ? Result<TNew>.Success(mapper(_value!)) : Result<TNew>.Failure(_error!);

    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder) =>
        IsSuccess ? binder(_value!) : Result<TNew>.Failure(_error!);
}

/// <summary>
/// Represents an error with a code, message, and optional metadata.
/// </summary>
public sealed record Error(string Code, string Message, Dictionary<string, object> Metadata = null)
{
    public static Error FileNotFound(string path) =>
        new("FILE_NOT_FOUND", $"File not found: {path}", new() { ["path"] = path });

    public static Error FileAlreadyExists(string path) =>
        new("FILE_ALREADY_EXISTS", $"File already exists: {path}", new() { ["path"] = path });

    public static Error BudgetExhausted(string context, int required, int available) =>
        new("BUDGET_EXHAUSTED", $"Budget exhausted in context '{context}'. Required: {required}, Available: {available}", new()
        {
            ["context"] = context,
            ["required"] = required,
            ["available"] = available
        });

    public static Error MaxDepthReached(string type, int maxDepth) =>
        new("MAX_DEPTH_REACHED", $"Maximum {type} depth ({maxDepth}) reached", new()
        {
            ["type"] = type,
            ["max_depth"] = maxDepth
        });

    public static Error CircularDependency(string chain) =>
        new("CIRCULAR_DEPENDENCY", $"Circular dependency detected: {chain}", new()
        {
            ["chain"] = chain
        });

    public static Error ContextNotFound(string contextName, IEnumerable<string> available) =>
        new("CONTEXT_NOT_FOUND", $"Context '{contextName}' not found", new()
        {
            ["context_name"] = contextName,
            ["available"] = available.ToList()
        });

    public static Error InvalidPattern(string pattern, string reason) =>
        new("INVALID_PATTERN", $"Pattern '{pattern}' is invalid: {reason}", new()
        {
            ["pattern"] = pattern,
            ["reason"] = reason
        });

    public static Error ModificationDisabled() =>
        new("MODIFICATION_DISABLED", "Project modification is disabled in configuration");

    public static Error UnknownTool(string toolName) =>
        new("UNKNOWN_TOOL", $"Unknown tool: {toolName}", new()
        {
            ["tool_name"] = toolName
        });

    public static Error Custom(string code, string message, Dictionary<string, object> metadata = null) =>
        new(code, message, metadata);
}

/// <summary>
/// Exception thrown when attempting to access the value of a failed Result.
/// </summary>
public sealed class ResultException : Exception
{
    public Error Error { get; }

    public ResultException(Error error) : base(error.Message)
    {
        Error = error;
    }
}