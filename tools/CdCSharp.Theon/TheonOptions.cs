namespace CdCSharp.Theon;

public class TheonOptions
{
    public string ProjectPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = ".theon";
    public LMStudioOptions LMStudio { get; set; } = new();
    public ConversationOptions Conversation { get; set; } = new();
    public ValidationOptions Validation { get; set; } = new();
}

public class LMStudioOptions
{
    public string BaseUrl { get; set; } = "http://localhost:1234/v1/";
    public int TimeoutSeconds { get; set; } = 7200;
    public string ReasoningTagPattern { get; set; } = @"(?s)<think>.*?</think>";
}

public class ConversationOptions
{
    public int CompressionThreshold { get; set; } = 10;
    public int MessagesToCompress { get; set; } = 7;
    public int MessagesToKeep { get; set; } = 3;
}

public class ValidationOptions
{
    public int MaxIterations { get; set; } = 3;
    public float ConfidenceThreshold { get; set; } = 0.7f;
}