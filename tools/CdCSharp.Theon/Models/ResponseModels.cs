namespace CdCSharp.Theon.Models;

public record ResponseOutput
{
    public required string FolderPath { get; init; }
    public required string Query { get; init; }
    public required string ResponseMarkdown { get; init; }
    public List<GeneratedFile> Files { get; init; } = [];
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public ResponseMetadata Metadata { get; init; } = new();
}

public record ResponseMetadata
{
    public List<string> AgentsInvolved { get; init; } = [];
    public int ValidationRounds { get; init; }
    public float FinalConfidence { get; init; }
    public TimeSpan ProcessingTime { get; init; }
}