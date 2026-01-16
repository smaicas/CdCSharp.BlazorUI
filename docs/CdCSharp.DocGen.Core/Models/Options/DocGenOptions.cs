namespace CdCSharp.DocGen.Core.Models.Options;

public record DocGenOptions
{
    public string ProjectPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = ".docgen";
    public AiProviderOptions Ai { get; set; } = new();
    public CacheOptions Cache { get; set; } = new();
    public PromptTracerOptions PromptTracer { get; set; } = new();
    public ConversationOptions Conversation { get; set; } = new();
}

public record AiProviderOptions
{
    public AiProviderType Provider { get; set; } = AiProviderType.LMStudio;
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "http://localhost:1234/v1/";
    public string Model { get; set; } = "llama-3.3-70b-versatile";
}

public enum AiProviderType
{
    Groq,
    LMStudio
}

public record CacheOptions
{
    public bool Enabled { get; set; } = true;
    public bool EnableAnalysisCache { get; set; } = true;
    public bool EnableQueryCache { get; set; } = true;
    public TimeSpan QueryCacheExpiration { get; set; } = TimeSpan.FromDays(7);
}

public record PromptTracerOptions
{
    public bool Enabled { get; set; } = false;
    public string TraceOutputPath { get; set; } = "trace";
}
public record ConversationOptions
{
    public int MaxTotalTokens { get; init; } = 8000;
    public int SlidingWindowSize { get; init; } = 6;
    public int CompressionThreshold { get; init; } = 10;
}