namespace CdCSharp.Theon;

public sealed class TheonOptions
{
    public required string ProjectPath { get; set; }
    public string OutputPath { get; set; } = ".theon";
    public string[] IgnoreFiles { get; set; } = [".gitignore", "theonignore.txt"];
    public LlmOptions Llm { get; set; } = new();
    public ProjectModificationOptions Modification { get; set; } = new();
    public string ResponsesPath => Path.Combine(OutputPath, "responses");
    public string LogsPath => Path.Combine(OutputPath, "logs");
    public string BackupsPath => Path.Combine(OutputPath, "backups");
}

public sealed class LlmOptions
{
    public string BaseUrl { get; set; } = "http://localhost:1234/v1";
    public string CompletionsUrl { get; set; } = "v1/chat/completions";
    public string Model { get; set; } = "openai/gpt-oss-20b";
    public int TimeoutSeconds { get; set; } = 7200;
    public double Temperature { get; set; } = 0.7;
}

public sealed class ProjectModificationOptions
{
    public bool Enabled { get; set; } = false;
    public bool CreateBackup { get; set; } = true;
}