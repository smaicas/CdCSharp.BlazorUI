// Models/AgentModels.cs

namespace CdCSharp.Theon.Models;

public class Agent
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public required string Name { get; init; }
    public required string Expertise { get; init; }
    public string Context { get; set; } = string.Empty;
    public AgentState State { get; set; } = AgentState.Active;
    public List<ConversationMessage> ConversationHistory { get; } = [];
    public byte[]? SleepData { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
}

public enum AgentState { Active, Sleeping }

// ✅ ACTUALIZADO: Query por ID
public record AgentRequest
{
    public required AgentRequestType Type { get; init; }
    public required string FromAgentId { get; init; }

    // ✅ CAMBIO: Ahora usa TargetAgentId obligatoriamente
    public string? TargetAgentId { get; init; }

    // ✅ OBSOLETO: Ya no se usa para queries, solo para validación/creación
    [Obsolete("Use TargetAgentId for queries. TargetExpertise only used for CREATE_AGENT.")]
    public string? TargetExpertise { get; init; }

    public string Payload { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public enum AgentRequestType
{
    CreateAgent,
    QueryAgent,
    GetFileContent,
    GetFileList
}

public record AgentCreationSpec
{
    public required string Name { get; init; }
    public required string Expertise { get; init; }
    public List<string> InitialContextFiles { get; init; } = [];
    public string? ParentAgentId { get; init; }
}

public class AgentExecutionResult
{
    public string AgentId { get; set; } = "";
    public string RawResponse { get; set; } = "";
    public string CleanContent { get; set; } = "";
    public float Confidence { get; set; } = 0.0f;
    public List<string> FileRequests { get; } = [];
    public List<string> FilePathsRequests { get; } = [];
    public List<AgentRequest> AgentQueries { get; } = [];
    public List<AgentCreationSpec> AgentCreationRequests { get; } = [];
    public List<GeneratedFile> GeneratedFiles { get; } = [];
    public List<string> SuggestedValidators { get; set; } = [];
    public List<string> FormatErrors { get; set; } = [];

    public bool HasPendingRequests =>
        FileRequests.Count > 0 ||
        FilePathsRequests.Count > 0 ||
        AgentQueries.Count > 0 ||
        AgentCreationRequests.Count > 0;

    public bool HasFormatErrors => FormatErrors.Count > 0;
}

public class FormatValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = [];
}

public record AgentResponse
{
    public required string AgentId { get; init; }
    public required string Content { get; init; }
    public float Confidence { get; init; } = 1.0f;
    public List<GeneratedFile> GeneratedFiles { get; init; } = [];
    public List<string> SuggestedValidatorExpertise { get; init; } = [];
    public bool NeedsValidation => Confidence < 0.7f || SuggestedValidatorExpertise.Count > 0;
}

public record GeneratedFile
{
    public required string FileName { get; init; }
    public required string Content { get; init; }
    public string? Language { get; init; }
}

public record ValidationResult
{
    public required string ValidatorAgentId { get; init; }
    public bool Approved { get; init; }
    public List<string> Objections { get; init; } = [];
    public List<string> Suggestions { get; init; } = [];
    public float Confidence { get; init; }
}