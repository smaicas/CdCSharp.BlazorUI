namespace CdCSharp.Theon.Context.Planning;

public sealed record ExecutionPlan
{
    public required List<string> TaskTypes { get; init; }
    public required string Reasoning { get; init; }
    public required List<PlanStep> Steps { get; init; }
    public required List<ExpectedOutput> ExpectedOutputs { get; init; }

    public bool IsValid => Steps.Count > 0;
}

public sealed record PlanStep
{
    public required int Order { get; init; }
    public required string TargetContext { get; init; }
    public required string Question { get; init; }
    public required List<string> SuggestedFiles { get; init; }
    public required string Purpose { get; init; }
    public required List<string> ContributesTo { get; init; }

    public PlanStepStatus Status { get; set; } = PlanStepStatus.Pending;
    public string? Result { get; set; }
}

public sealed record ExpectedOutput
{
    public required string TaskType { get; init; }
    public required string Description { get; init; }
    public required OutputType Type { get; init; }
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