namespace CdCSharp.Theon;

public sealed class TheonOptions
{
    public const string SectionName = "Theon";

    public string ProjectPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = ".theon";
    public int MaxExplorationDepth { get; set; } = 5;

    public LlmOptions Llm { get; set; } = new();
    public ProjectModificationOptions Modification { get; set; } = new();

    public string ResponsesPath => Path.Combine(OutputPath, "responses");
    public string LogsPath => Path.Combine(OutputPath, "logs");
    public string BackupsPath => Path.Combine(OutputPath, "backups");
    public string AnalysisPath => Path.Combine(OutputPath, "analysis");
}

public sealed class LlmOptions
{
    public string BaseUrl { get; set; } = "http://localhost:1234/v1";
    public int TimeoutSeconds { get; set; } = 300;
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