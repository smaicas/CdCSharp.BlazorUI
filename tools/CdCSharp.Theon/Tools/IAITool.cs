public interface IAITool
{
    string Name { get; }
    string Description { get; }
    string UsageFormat { get; }
    string Example { get; }
}

public static class AITools
{
    public static readonly RequestFilesList RequestFilesList = new();
    public static readonly RequestFileTool RequestFile = new();
    public static readonly ListGeneratedFilesTool ListGeneratedFiles = new();
    public static readonly QueryAgentTool QueryAgent = new();
    public static readonly CreateAgentTool CreateAgent = new();
    public static readonly GenerateFileTool GenerateFile = new();
    public static readonly AppendToFileTool AppendToFile = new();
    public static readonly SetConfidenceTool SetConfidence = new();
    public static readonly SuggestValidationTool SuggestValidation = new();

    public static IEnumerable<IAITool> All =>
    [
        RequestFilesList,
        RequestFile,
        ListGeneratedFiles,
        QueryAgent,
        CreateAgent,
        GenerateFile,
        AppendToFile,
        SetConfidence,
        SuggestValidation
    ];

    public static string GetToolsDocumentation()
    {
        List<string> docs = [];

        foreach (IAITool tool in All)
        {
            docs.Add($"""
                ### {tool.Name}
                {tool.Description}
                
                **Format:** `{tool.UsageFormat}`
                
                **Example:**

                {tool.Example}

                """);
        }

        return string.Join("\n\n", docs);
    }
}

public class RequestFilesList : IAITool
{
    public string Name => "REQUEST_FILE_PATHS";
    public string Description => "Request the list of files in an assembly or the entire project. Use empty assembly for full project listing.";
    public string UsageFormat => "[REQUEST_FILE_PATHS: assembly=\"AssemblyName\"]";
    public string Example => """
        <!-- List files in specific assembly -->
        [REQUEST_FILE_PATHS: assembly="CdCSharp.BlazorUI"]
        
        <!-- List all project files -->
        [REQUEST_FILE_PATHS: assembly=""]
        """;
}

public class RequestFileTool : IAITool
{
    public string Name => "REQUEST_FILE";
    public string Description => "Request the content of a specific file from the project.";
    public string UsageFormat => "[REQUEST_FILE: path=\"full/path/to/file.extension\"]";
    public string Example => "[REQUEST_FILE: path=\"V:/src/Services/UserService.cs\"]";
}

public class ListGeneratedFilesTool : IAITool
{
    public string Name => "LIST_GENERATED_FILES";
    public string Description => "See all files you have generated in previous interactions. This helps you remember what files you've created so you can reference or modify them.";
    public string UsageFormat => "[LIST_GENERATED_FILES]";
    public string Example => """
        <!-- See what files you've created -->
        [LIST_GENERATED_FILES]
        
        <!-- This will show you:
        - File names
        - Languages
        - Sizes
        - When created/modified
        -->
        """;
}

public class QueryAgentTool : IAITool
{
    public string Name => "QUERY_AGENT";
    public string Description => "Ask a question to another specialized agent. The orchestrator will route your query to the appropriate agent or create one if needed.";
    public string UsageFormat => "[QUERY_AGENT: expertise=\"area of expertise\" question=\"your specific question\"]";
    public string Example => "[QUERY_AGENT: expertise=\"dependency injection\" question=\"How is the IUserRepository registered?\"]";
}

public class CreateAgentTool : IAITool
{
    public string Name => "CREATE_AGENT";
    public string Description => "Request creation of a new specialized agent when you need expertise that doesn't exist yet.";
    public string UsageFormat => "[CREATE_AGENT: name=\"Agent Name\" expertise=\"specific expertise\" reason=\"why needed\" files=\"file1.cs,file2.cs\"]";
    public string Example => "[CREATE_AGENT: name=\"Blazor Components Expert\" expertise=\"Blazor UI components and their parameters\" reason=\"Need detailed knowledge of component hierarchy\" files=\"Components/Button.razor,Components/Input.razor\"]";
}

public class GenerateFileTool : IAITool
{
    public string Name => "GENERATE_FILE";
    public string Description => "Generate a new file or replace an existing one. Content goes directly between tags, no markdown formatting needed. You can later see all your generated files with LIST_GENERATED_FILES.";
    public string UsageFormat =>
    """
    [GENERATE_FILE: name="FileName.ext" language="languageid"]
    file content here
    [/GENERATE_FILE]
    """;

    public string Example =>
    """
    [GENERATE_FILE: name="UserService.cs" language="csharp"]
    public class UserService : IUserService
    {
        public Task<User> GetByIdAsync(int id) => throw new NotImplementedException();
    }
    [/GENERATE_FILE]
    """;
}

public class AppendToFileTool : IAITool
{
    public string Name => "APPEND_TO_FILE";
    public string Description => "Append content to an existing generated file. If file doesn't exist, creates it. The system remembers all files you've generated.";
    public string UsageFormat =>
    """
    [APPEND_TO_FILE: name="FileName.ext"]
    content to append
    [/APPEND_TO_FILE]
    """;

    public string Example =>
    """
    [APPEND_TO_FILE: name="README.md"]
    
    ## Additional Section
    This content will be added to the end of README.md
    [/APPEND_TO_FILE]
    """;
}

public class SetConfidenceTool : IAITool
{
    public string Name => "CONFIDENCE";
    public string Description => "Set your confidence level for the response. Values below 0.7 will trigger automatic validation by other agents.";
    public string UsageFormat => "[CONFIDENCE: 0.0-1.0]";
    public string Example => "[CONFIDENCE: 0.85]";
}

public class SuggestValidationTool : IAITool
{
    public string Name => "SUGGEST_VALIDATION";
    public string Description => "Suggest that your response should be validated by agents with specific expertise.";
    public string UsageFormat => "[SUGGEST_VALIDATION: expertise=\"area1,area2\"]";
    public string Example => "[SUGGEST_VALIDATION: expertise=\"security,performance\"]";
}