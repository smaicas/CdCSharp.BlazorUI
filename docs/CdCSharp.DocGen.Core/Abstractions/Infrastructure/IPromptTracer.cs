namespace CdCSharp.DocGen.Core.Abstractions.Infrastructure;

public interface IPromptTracer
{
    bool IsEnabled { get; }

    /// <summary>
    /// Crea una traza inicial con el prompt enviado (sin respuesta aún)
    /// </summary>
    Task<string> TracePromptStartAsync(string agentId, string prompt);

    /// <summary>
    /// Completa la traza con la respuesta recibida
    /// </summary>
    Task TracePromptCompleteAsync(string traceId, string response);

    /// <summary>
    /// Marca la traza como fallida con información del error
    /// </summary>
    Task TracePromptFailureAsync(string traceId, Exception ex);
}