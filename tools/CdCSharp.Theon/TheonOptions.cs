namespace CdCSharp.Theon;

public sealed class TheonOptions
{
    public const string SectionName = "Theon";

    public string ProjectPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = ".theon";
    public int MaxExplorationDepth { get; set; } = 20; // 5

    public LlmOptions Llm { get; set; } = new();
    public ProjectModificationOptions Modification { get; set; } = new();
    public ExplorationOptions Exploration { get; set; } = new();

    public string ResponsesPath => Path.Combine(OutputPath, "responses");
    public string LogsPath => Path.Combine(OutputPath, "logs");
    public string BackupsPath => Path.Combine(OutputPath, "backups");
    public string AnalysisPath => Path.Combine(OutputPath, "analysis");
}

public sealed class LlmOptions
{
    public string BaseUrl { get; set; } = "http://localhost:1234/v1";
    public int TimeoutSeconds { get; set; } = 7200;
    public double Temperature { get; set; } = 0.7;
    public string? ReasoningTagPattern { get; set; }
}

public sealed class ProjectModificationOptions
{
    public bool Enabled { get; set; } = false;
    public bool RequireConfirmation { get; set; } = true;
    public bool CreateBackup { get; set; } = true;
    public bool AllowNewFiles { get; set; } = false;
}

/// <summary>
/// Options for controlling exploration quality and validation.
/// </summary>
public sealed class ExplorationOptions
{
    /// <summary>
    /// Reject responses without exploration for analysis/documentation tasks.
    /// Default: true
    /// </summary>
    public bool RequireExplorationForAnalysis { get; set; } = true;

    /// <summary>
    /// Minimum files to explore for documentation tasks.
    /// Default: 3
    /// </summary>
    public int MinimumFilesForDocumentation { get; set; } = 10;

    /// <summary>
    /// Penalize confidence when generic phrases are detected.
    /// Default: true
    /// </summary>
    public bool PenalizeGenericResponses { get; set; } = true;

    /// <summary>
    /// Confidence threshold below which self-review is triggered.
    /// Default: 0.6
    /// </summary>
    public float LowConfidenceThreshold { get; set; } = 0.8f;

    /// <summary>
    /// Enable validation of LLM responses before accepting them.
    /// Default: true
    /// </summary>
    public bool EnableResponseValidation { get; set; } = true;

    /// <summary>
    /// Maximum validation failures before accepting response anyway.
    /// Default: 2
    /// </summary>
    public int MaxValidationRetries { get; set; } = 10;

    /// <summary>
    /// Enable automatic output planning based on task type.
    /// Default: true
    /// </summary>
    public bool EnableOutputPlanning { get; set; } = true;

    /// <summary>
    /// Enable exploration strategy guidance based on task classification.
    /// Default: true
    /// </summary>
    public bool EnableExplorationStrategies { get; set; } = true;
}