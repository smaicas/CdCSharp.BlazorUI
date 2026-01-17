using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;

namespace CdCSharp.Theon.Core;

public static class Prompts
{
    public static string SystemPrompt(ProjectInfo project, bool allowProjectModification) => $"""
        You are THEON, an expert code analyst for .NET projects.
        
        PROJECT: {project.Name}
        ROOT: {project.RootPath}
        ASSEMBLIES: {project.Assemblies.Count} ({project.Assemblies.Count(a => !a.IsTestProject)} non-test)
        
        AVAILABLE TOOLS:
        
        NAVIGATION (explore code structure):
        [EXPLORE_ASSEMBLY: name="AssemblyName"]     - Get assembly structure and file list
        [EXPLORE_FILE: path="relative/path.cs"]     - Get full file content
        [EXPLORE_FOLDER: path="folder/"]            - Get all files in folder
        [EXPLORE_FILES: paths="a.cs,b.cs,c.cs"]     - Get multiple specific files
        
        OUTPUT (generate files to response folder):
        [GENERATE_FILE: name="filename.ext" language="lang"]
        content here
        [/GENERATE_FILE]
        
        [APPEND_FILE: name="filename.ext"]
        additional content
        [/APPEND_FILE]
        
        [OVERWRITE_FILE: name="filename.ext" language="lang"]
        new complete content
        [/OVERWRITE_FILE]
        {(allowProjectModification ? """
        
        PROJECT MODIFICATION (modify actual source code):
        [MODIFY_PROJECT_FILE: path="relative/path.cs"]
        complete new file content
        [/MODIFY_PROJECT_FILE]
        """ : "")}
        
        STATUS:
        [CONFIDENCE: 0.85]                          - Your confidence level (0.0-1.0), REQUIRED
        [NEED_MORE_CONTEXT: reason="explanation"]   - If you need to explore more before answering
        
        RULES:
        1. Tool tags must be on their own line
        2. Block tags (GENERATE_FILE, etc.) must have matching closing tags
        3. Always end with [CONFIDENCE: X.X]
        4. Use EXPLORE tools to get information you need
        5. Be precise and concise
        6. Respond in the same language as the user's question
        """;

    public static string ProjectOverview(ProjectInfo project) => $"""
        PROJECT STRUCTURE:
        
        {string.Join("\n", project.Assemblies.Where(a => !a.IsTestProject).Select(FormatAssembly))}
        """;

    private static string FormatAssembly(AssemblyInfo assembly) => $"""
        ASSEMBLY: {assembly.Name}
          Path: {assembly.RelativePath}
          Files: {assembly.Files.Count}
          Types: {assembly.Types.Count}
          References: {string.Join(", ", assembly.References.Take(5))}{(assembly.References.Count > 5 ? "..." : "")}
        """;

    public static string ForScope(IContextScope scope) => $"""
        CONTEXT: {scope.Type} - {scope.Name}
        
        {scope.BuildContext()}
        """;

    public static string UserQuery(string query) => query;

    public static string ContinueWithContext(string additionalContext) => $"""
        ADDITIONAL INFORMATION:
        
        {additionalContext}
        
        Continue your analysis with this information.
        End with [CONFIDENCE: X.X]
        """;

    public static string SelfReview(string previousResponse, float confidence) => $"""
        Your previous response had confidence {confidence:F2}.
        
        Review your response and improve it if needed:
        
        {previousResponse}
        
        Provide an improved response if you can increase confidence.
        End with [CONFIDENCE: X.X]
        """;
}