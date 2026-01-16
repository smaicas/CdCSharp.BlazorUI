namespace CdCSharp.DocGen.Core.Abstractions.Infrastructure;

public interface IPromptTracer
{
    bool IsEnabled { get; }
    Task TracePromptAsync(string requestId, string prompt, string response);
}