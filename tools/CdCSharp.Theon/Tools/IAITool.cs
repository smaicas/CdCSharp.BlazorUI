public interface IAITool
{
    string Name { get; }
    string Description { get; }
    string Syntax { get; }
    bool RequiresClosingTag { get; }
    string? ClosingTag { get; }
}

public static class AITools
{
    public static readonly RequestFilePathsTool RequestFilePaths = new();
    public static readonly RequestFileTool RequestFile = new();
    public static readonly ListGeneratedFilesTool ListGeneratedFiles = new();
    public static readonly QueryAgentTool QueryAgent = new();
    public static readonly CreateAgentTool CreateAgent = new();
    public static readonly GenerateFileTool GenerateFile = new();
    public static readonly AppendToFileTool AppendToFile = new();
    public static readonly ConfidenceTool Confidence = new();
    public static readonly SuggestValidationTool SuggestValidation = new();
    public static readonly TaskCompleteTool TaskComplete = new();

    public static IEnumerable<IAITool> All =>
    [
        RequestFilePaths,
        RequestFile,
        ListGeneratedFiles,
        QueryAgent,
        CreateAgent,
        GenerateFile,
        AppendToFile,
        Confidence,
        SuggestValidation,
        TaskComplete
    ];

    public static string GetToolsDocumentation()
    {
        List<string> docs = [];

        List<IAITool> singleLineTags = All.Where(t => !t.RequiresClosingTag).ToList();
        List<IAITool> blockTags = All.Where(t => t.RequiresClosingTag).ToList();

        docs.Add("SINGLE-LINE TAGS (no closing tag):");
        foreach (IAITool tool in singleLineTags)
        {
            docs.Add($"  {tool.Name}: {tool.Description}");
            docs.Add($"    Syntax: {tool.Syntax}");
        }

        docs.Add("");
        docs.Add("BLOCK TAGS (require closing tag):");
        foreach (IAITool tool in blockTags)
        {
            docs.Add($"  {tool.Name}: {tool.Description}");
            docs.Add($"    Opening: {tool.Syntax}");
            docs.Add($"    Closing: {tool.ClosingTag}");
            docs.Add($"    Content goes between opening and closing tags.");
        }

        return string.Join("\n", docs);
    }
}

public class RequestFilePathsTool : IAITool
{
    public string Name => "REQUEST_FILE_PATHS";
    public string Description => "Get list of files. Empty assembly value returns all project files.";
    public string Syntax => "[REQUEST_FILE_PATHS: assembly=\"AssemblyName\"]";
    public bool RequiresClosingTag => false;
    public string? ClosingTag => null;
}

public class RequestFileTool : IAITool
{
    public string Name => "REQUEST_FILE";
    public string Description => "Get content of a specific file.";
    public string Syntax => "[REQUEST_FILE: path=\"relative/path/to/file.cs\"]";
    public bool RequiresClosingTag => false;
    public string? ClosingTag => null;
}

public class ListGeneratedFilesTool : IAITool
{
    public string Name => "LIST_GENERATED_FILES";
    public string Description => "List files you have generated in this session.";
    public string Syntax => "[LIST_GENERATED_FILES]";
    public bool RequiresClosingTag => false;
    public string? ClosingTag => null;
}

// ✅ ACTUALIZADO: Query por ID
public class QueryAgentTool : IAITool
{
    public string Name => "QUERY_AGENT";
    public string Description => "Send question to another agent by their ID.";
    public string Syntax => "[QUERY_AGENT: id=\"agentId\" question=\"your question\"]";
    public bool RequiresClosingTag => false;
    public string? ClosingTag => null;
}

public class CreateAgentTool : IAITool
{
    public string Name => "CREATE_AGENT";
    public string Description => "Request creation of new specialized agent.";
    public string Syntax => "[CREATE_AGENT: name=\"Name\" expertise=\"expertise\" files=\"file1.cs,file2.cs\"]";
    public bool RequiresClosingTag => false;
    public string? ClosingTag => null;
}

public class GenerateFileTool : IAITool
{
    public string Name => "GENERATE_FILE";
    public string Description => "Create or replace a file. Content between tags is written to file exactly as provided.";
    public string Syntax => "[GENERATE_FILE: name=\"FileName.ext\" language=\"languageid\"]";
    public bool RequiresClosingTag => true;
    public string? ClosingTag => "[/GENERATE_FILE]";
}

public class AppendToFileTool : IAITool
{
    public string Name => "APPEND_TO_FILE";
    public string Description => "Append content to existing generated file.";
    public string Syntax => "[APPEND_TO_FILE: name=\"FileName.ext\"]";
    public bool RequiresClosingTag => true;
    public string? ClosingTag => "[/APPEND_TO_FILE]";
}

public class ConfidenceTool : IAITool
{
    public string Name => "CONFIDENCE";
    public string Description => "Your confidence level 0.0 to 1.0. Required at end of every response.";
    public string Syntax => "[CONFIDENCE: 0.85]";
    public bool RequiresClosingTag => false;
    public string? ClosingTag => null;
}

public class SuggestValidationTool : IAITool
{
    public string Name => "SUGGEST_VALIDATION";
    public string Description => "Request validation from agents with specific expertise.";
    public string Syntax => "[SUGGEST_VALIDATION: expertise=\"area1,area2\"]";
    public bool RequiresClosingTag => false;
    public string? ClosingTag => null;
}

public class TaskCompleteTool : IAITool
{
    public string Name => "TASK_COMPLETE";
    public string Description => "Mark that you have fully addressed the query.";
    public string Syntax => "[TASK_COMPLETE]";
    public bool RequiresClosingTag => false;
    public string? ClosingTag => null;
}