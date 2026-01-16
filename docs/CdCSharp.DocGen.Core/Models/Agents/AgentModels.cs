using System.Text.Json.Serialization;

namespace CdCSharp.DocGen.Core.Models.Agents;

public record AgentDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("systemPrompt")]
    public string SystemPrompt { get; init; } = string.Empty;

    [JsonPropertyName("expertise")]
    public AgentExpertise Expertise { get; init; } = new();

    [JsonPropertyName("capabilities")]
    public List<string> Capabilities { get; init; } = [];

    [JsonPropertyName("isBuiltIn")]
    public bool IsBuiltIn { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public record AgentExpertise
{
    [JsonPropertyName("assemblies")]
    public List<string> Assemblies { get; init; } = [];

    [JsonPropertyName("filePatterns")]
    public List<string> FilePatterns { get; init; } = [];

    [JsonPropertyName("files")]
    public List<string> Files { get; init; } = [];

    [JsonPropertyName("namespaces")]
    public List<string> Namespaces { get; init; } = [];

    [JsonPropertyName("topics")]
    public List<string> Topics { get; init; } = [];
}

public record AgentTask
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    [JsonPropertyName("agentId")]
    public string AgentId { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("focus")]
    public string Focus { get; init; } = string.Empty;

    [JsonPropertyName("targetSections")]
    public List<string> TargetSections { get; init; } = [];

    [JsonPropertyName("priority")]
    public int Priority { get; init; }

    [JsonPropertyName("instructions")]
    public List<TaskInstruction> Instructions { get; init; } = [];
}

public record TaskInstruction
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..4];

    [JsonPropertyName("instruction")]
    public string Instruction { get; init; } = string.Empty;

    [JsonPropertyName("expectedOutput")]
    public string ExpectedOutput { get; init; } = string.Empty;

    [JsonPropertyName("maxTokens")]
    public int MaxTokens { get; init; } = 2000;

    [JsonPropertyName("requiredFiles")]
    public RequiredFiles RequiredFiles { get; init; } = new();
}

public record RequiredFiles
{
    [JsonPropertyName("destructured")]
    public List<string> Destructured { get; init; } = [];

    [JsonPropertyName("fullContent")]
    public List<string> FullContent { get; init; } = [];
}

public record AgentMessage
{
    [JsonPropertyName("role")]
    public AgentMessageRole Role { get; init; }

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    [JsonPropertyName("fromAgentId")]
    public string? FromAgentId { get; init; }

    [JsonPropertyName("toAgentId")]
    public string? ToAgentId { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgentMessageRole
{
    System,
    User,
    Assistant,
    AgentQuery,
    AgentResponse
}

public record AgentQuery
{
    [JsonPropertyName("fromAgentId")]
    public string FromAgentId { get; init; } = string.Empty;

    [JsonPropertyName("targetAgentId")]
    public string? TargetAgentId { get; init; }

    [JsonPropertyName("targetExpertise")]
    public string? TargetExpertise { get; init; }

    [JsonPropertyName("question")]
    public string Question { get; init; } = string.Empty;

    [JsonPropertyName("context")]
    public string? Context { get; init; }
}

public record AgentQueryResult
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("respondingAgentId")]
    public string RespondingAgentId { get; init; } = string.Empty;

    [JsonPropertyName("response")]
    public string Response { get; init; } = string.Empty;

    [JsonPropertyName("agentCreated")]
    public bool AgentCreated { get; init; }
}

public record AgentCreationRequest
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("expertise")]
    public AgentExpertise Expertise { get; init; } = new();

    [JsonPropertyName("reason")]
    public string Reason { get; init; } = string.Empty;
}

public record AgentResult
{
    [JsonPropertyName("agentId")]
    public string AgentId { get; init; } = string.Empty;

    [JsonPropertyName("taskId")]
    public string TaskId { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    [JsonPropertyName("targetSections")]
    public List<string> TargetSections { get; init; } = [];

    [JsonPropertyName("queriesMade")]
    public List<AgentQuery> QueriesMade { get; init; } = [];

    [JsonPropertyName("executedAt")]
    public DateTime ExecutedAt { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("tokenCount")]
    public int TokenCount { get; init; }
}