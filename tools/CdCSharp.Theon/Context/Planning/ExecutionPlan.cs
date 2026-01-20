using System.Text.Json.Serialization;

namespace CdCSharp.Theon.Context.Planning;

public sealed class ExecutionPlan
{
    [JsonPropertyName("taskTypes")]
    public List<string> TaskTypes { get; set; } = [];

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;

    [JsonPropertyName("steps")]
    public List<PlanStep> Steps { get; set; } = [];

    [JsonPropertyName("expectedOutputs")]
    public List<ExpectedOutput> ExpectedOutputs { get; set; } = [];

    [JsonIgnore]
    public bool IsValid => Steps.Count > 0;
}

public sealed class PlanStep
{
    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("targetContext")]
    public string TargetContext { get; set; } = string.Empty;

    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("suggestedFiles")]
    public List<string> SuggestedFiles { get; set; } = [];

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    [JsonPropertyName("contributesTo")]
    public List<string> ContributesTo { get; set; } = [];

    [JsonIgnore]
    public PlanStepStatus Status { get; set; } = PlanStepStatus.Pending;

    [JsonIgnore]
    public string? Result { get; set; }
}

public sealed class ExpectedOutput
{
    [JsonPropertyName("taskType")]
    public string TaskType { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public OutputType Type { get; set; } = OutputType.Documentation;
}

public enum PlanStepStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}

public enum OutputType
{
    Documentation,
    CodeChange,
    AnalysisReport,
    ProjectFile
}