using System.Text.Json.Serialization;

namespace CdCSharp.Theon.Orchestrator.Models;

public sealed class OrchestratorResponse
{
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("proposed_changes")]
    public List<ProposedChange> ProposedChanges { get; init; } = [];

    [JsonPropertyName("created_files")]
    public List<string> CreatedFiles { get; init; } = [];

    [JsonPropertyName("generated_outputs")]
    public List<string> GeneratedOutputs { get; init; } = [];

    [JsonPropertyName("modified_files")]
    public List<string> ModifiedFiles { get; init; } = [];

    [JsonPropertyName("confidence")]
    public float Confidence { get; init; }

    [JsonPropertyName("needs_confirmation")]
    public bool NeedsConfirmation { get; init; }
}

public sealed class ProposedChange
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("change_type")]
    public ChangeType ChangeType { get; init; }

    [JsonPropertyName("original_content")]
    public string? OriginalContent { get; init; }

    [JsonPropertyName("new_content")]
    public string NewContent { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public ChangeStatus Status { get; set; } = ChangeStatus.Pending;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChangeType
{
    Create,
    Modify,
    Delete
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChangeStatus
{
    Pending,
    Applied,
    Rejected
}

public sealed class ContextQueryResult
{
    [JsonPropertyName("context_name")]
    public string ContextName { get; init; } = string.Empty;

    [JsonPropertyName("question")]
    public string Question { get; init; } = string.Empty;

    [JsonPropertyName("answer")]
    public string Answer { get; init; } = string.Empty;

    [JsonPropertyName("files_examined")]
    public List<string> FilesExamined { get; init; } = [];
}