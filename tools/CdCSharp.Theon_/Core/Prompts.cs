using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Tools;

namespace CdCSharp.Theon.Core;

public interface IPromptBuilder
{
    string BuildSystemPrompt(ProjectInfo project, bool allowProjectModification, bool supportsNativeTools);
    string BuildProjectOverview(ProjectInfo project);
    string BuildScopeContext(IContextScope scope);
    string BuildContinueWithContext(string additionalContext);
    string BuildSelfReview(string previousResponse, float confidence);
    string BuildExplorationGuidance(string taskType, int currentExplorationCount);
    string BuildValidationFeedback(string reason, float adjustedConfidence);
}

public sealed class PromptBuilder : IPromptBuilder
{
    private readonly IToolRegistry _toolRegistry;

    public PromptBuilder(IToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public string BuildSystemPrompt(ProjectInfo project, bool allowProjectModification, bool supportsNativeTools)
    {
        string toolsSection = supportsNativeTools
            ? BuildNativeToolsInstructions()
            : BuildTagBasedToolsInstructions(allowProjectModification);

        return $"""
            You are THEON, an elite code analyst and architect for .NET projects.
            
            # PROJECT CONTEXT
            
            PROJECT: {project.Name}
            ROOT: {project.RootPath}
            ASSEMBLIES: {project.Assemblies.Count} ({project.Assemblies.Count(a => !a.IsTestProject)} non-test)
            TOTAL FILES: {project.AllFiles.Count()}
            TOTAL TYPES: {project.Assemblies.Sum(a => a.Types.Count)}
            
            # CRITICAL REASONING PROTOCOL
            
            You MUST follow this mandatory workflow for EVERY query:
            
            1. UNDERSTAND - What is the user asking? What type of task is this?
            2. PLAN - What specific code do I need to examine?
            3. EXPLORE (MANDATORY) - Use exploration tools to read ACTUAL code
            4. ANALYZE - Extract patterns from the REAL code you explored
            5. SYNTHESIZE - Generate output based ONLY on explored code
            6. VALIDATE - Confidence MUST reflect exploration depth
            
            # CONFIDENCE RULES
            
            - No exploration = max 0.3 confidence
            - 1-2 files = max 0.5 confidence
            - 3-5 files = 0.6-0.8 confidence
            - 6+ files = 0.8-0.95 confidence
            
            # ABSOLUTE RULES
            
            - NEVER generate documentation without exploring files
            - NEVER use generic phrases like "designed to provide"
            - NEVER claim high confidence without substantial exploration
            - NEVER invent class names, methods, or patterns
            - ALWAYS explore before documenting
            - ALWAYS use real names from the actual code
            
            {toolsSection}
            
            # PROJECT FILES
            
            {string.Join("\n", project.AllFiles.Select(f => $"- {f}"))}
            
            # LANGUAGE
            
            Respond in the same language as the user's question.
            """;
    }

    private string BuildNativeToolsInstructions()
    {
        return """
            # TOOLS
            
            You have access to tools for code exploration and output generation.
            Use the provided tools to explore code before generating any documentation or analysis.
            
            AVAILABLE TOOLS:
            - EXPLORE_ASSEMBLY: Get assembly structure (files, types, references)
            - EXPLORE_FILE: Get full content of a specific file
            - EXPLORE_FOLDER: Get all files in a folder
            - EXPLORE_FILES: Get multiple specific files at once
            - GENERATE_FILE: Create an output file
            - APPEND_FILE: Append content to an existing output file
            - OVERWRITE_FILE: Replace content of an output file
            
            WORKFLOW:
            1. Call exploration tools to examine code
            2. Receive tool results
            3. Continue analysis or call more tools
            4. When ready, provide your final response with generated files
            
            When you have completed your task, include in your response:
            - Your analysis/explanation
            - A confidence level (0.0-1.0) based on exploration depth
            """;
    }

    private string BuildTagBasedToolsInstructions(bool allowProjectModification)
    {
        string toolsDocs = _toolRegistry.GeneratePromptDocumentation();

        return $"""
            # AVAILABLE TOOLS
            
            {toolsDocs}
            
            {(allowProjectModification ? "" : "NOTE: Project modification tools are DISABLED.")}
            
            # RESPONSE FORMAT
            
            Use tags to invoke tools. Examples:
            
            [EXPLORE_FILE: path="Core/MyClass.cs"]
            
            [GENERATE_FILE: name="README.md" language="markdown"]
            # Content here
            [/GENERATE_FILE]
            
            End your response with confidence level:
            [CONFIDENCE: 0.85]
            """;
    }

    public string BuildProjectOverview(ProjectInfo project)
    {
        List<AssemblyInfo> nonTestAssemblies = project.Assemblies.Where(a => !a.IsTestProject).ToList();

        return $"""
            PROJECT STRUCTURE OVERVIEW:
            
            {string.Join("\n\n", nonTestAssemblies.Select(FormatAssemblyOverview))}
            
            EXPLORATION RECOMMENDATIONS:
            - Start with assembly structure to understand organization
            - Explore folders for module-level understanding
            - Read specific files for implementation details
            """;
    }

    private static string FormatAssemblyOverview(AssemblyInfo assembly)
    {
        List<IGrouping<string, TypeSummary>> topNamespaces = assembly.Types
            .GroupBy(t => t.Namespace)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .ToList();

        return $"""
            ASSEMBLY: {assembly.Name}
              Location: {assembly.RelativePath}
              Files: {assembly.Files.Count}
              Types: {assembly.Types.Count}
              
              Key Namespaces:
            {string.Join("\n", topNamespaces.Select(ns => $"    - {ns.Key} ({ns.Count()} types)"))}
            """;
    }

    public string BuildScopeContext(IContextScope scope) => $"""
        # EXPLORATION RESULT
        
        Scope Type: {scope.Type}
        Scope Name: {scope.Name}
        Estimated Tokens: ~{scope.EstimatedTokens}
        
        ---
        
        {scope.BuildContext()}
        
        ---
        
        This is REAL code from the project. Use this information accurately.
        """;

    public string BuildContinueWithContext(string additionalContext) => $"""
        # TOOL RESULTS
        
        {additionalContext}
        
        ---
        
        Continue your analysis with this information.
        """;

    public string BuildSelfReview(string previousResponse, float confidence) => $"""
        # SELF-REVIEW REQUEST
        
        Your previous response had confidence: {confidence:F2}
        
        This is below the quality threshold. Review your work:
        
        PREVIOUS RESPONSE:
        ---
        {previousResponse}
        ---
        
        Provide an improved, more specific response based on explored code.
        """;

    public string BuildExplorationGuidance(string taskType, int currentExplorationCount) => $"""
        # EXPLORATION GUIDANCE
        
        Task Type: {taskType}
        Current Explorations: {currentExplorationCount}
        
        You should explore more files before generating output.
        Use exploration tools to examine the actual code.
        """;

    public string BuildValidationFeedback(string reason, float adjustedConfidence) => $"""
        # VALIDATION FEEDBACK
        
        Reason: {reason}
        Adjusted Confidence: {adjustedConfidence:F2}
        
        Your response did not meet quality standards.
        Explore more code and provide a more accurate response.
        """;
}