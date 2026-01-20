namespace CdCSharp.Theon.Context.Planning;

public static class PlannerContextConfiguration
{
    public const string ContextName = "Planner";
    public const string ContextType = "Planner";

    public static ContextConfiguration Create(string model, IReadOnlyList<ContextMetadata> availableContexts)
    {
        string contextsSection = BuildAvailableContextsSection(availableContexts);

        return new()
        {
            Name = ContextName,
            Model = model,
            ContextType = ContextType,
            Speciality = "Task analysis and execution planning",
            SystemPrompt = BuildSystemPrompt(contextsSection),
            IsStateful = false,
            MaxTokenBudget = 8000,

            CanReadFiles = false,
            CanPeekFiles = true,
            CanSearchFiles = true,

            CanDelegateToContexts = false,
            CanSpawnClones = false
        };
    }

    private static string BuildAvailableContextsSection(IReadOnlyList<ContextMetadata> contexts)
    {
        IEnumerable<string> contextDescriptions = contexts
            .Where(c => c.ContextType != ContextType && !c.IsClone)
            .Select(c => $"- **{c.ContextType}**: {c.Speciality}");

        return string.Join("\n", contextDescriptions);
    }

    private static string BuildSystemPrompt(string availableContexts) => $$"""
        You are an execution planner for code analysis tasks. Your role is to create detailed,
        actionable plans that ensure thorough investigation before generating any output.

        ## Your Responsibilities
        1. Analyze the user's request to understand what information is needed
        2. Identify which specialized contexts should be consulted
        3. Determine which files are most relevant to examine
        4. Create a step-by-step plan that covers multiple perspectives
        5. Handle composite tasks (e.g., "refactor and document") by planning for all parts

        ## Available Contexts
        {{availableContexts}}

        ## Planning Rules
        - NEVER create a plan with zero steps
        - ALWAYS include at least 2 different contexts for comprehensive tasks
        - For documentation tasks: include architecture analysis + key code examination
        - For analysis tasks: include all relevant contexts
        - For composite tasks: create steps that address EACH requested task type
        - Be SPECIFIC about which files to examine - use exact paths from the File Index
        - Each step must have a clear PURPOSE that contributes to the final output
        - Order steps logically: understand structure first, then dive into details

        ## File Access
        - Use `peek_file` to examine files WITHOUT consuming budget
        - Use `search_files` to find relevant files by pattern
        - Files you peek are available ONLY for creating the plan
        - Contexts in the plan will use `read_file` to load files permanently

        ## Task Type Classification (can be multiple)
        - "documentation": README, API docs, guides → need structure + key code examples
        - "analysis": Code review, architecture review → need all perspectives
        - "investigation": Bug finding, understanding flow → need code + dependency analysis
        - "refactoring": Code changes → need all contexts to understand impact
        - "generation": Creating new code → need architecture + existing patterns

        ## CRITICAL: Response Format
        You MUST respond with ONLY a JSON object matching the ExecutionPlan schema.
        Do NOT include ANY text outside the JSON.
        Do NOT use markdown code fences.
        
        The system uses structured output - your response will be automatically validated
        against the schema, so it MUST be valid JSON.
        """;

    public static object GetResponseSchema(IReadOnlyList<ContextMetadata> availableContexts)
    {
        string[] contextTypes = availableContexts
            .Where(c => c.ContextType != ContextType && !c.IsClone)
            .Select(c => c.ContextType)
            .ToArray();

        return new
        {
            type = "object",
            properties = new
            {
                taskTypes = new
                {
                    type = "array",
                    items = new { type = "string" },
                    description = "All task types identified in the request"
                },
                reasoning = new { type = "string", description = "Why this plan is appropriate" },
                steps = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            order = new { type = "integer" },
                            targetContext = new { type = "string", @enum = contextTypes },
                            question = new { type = "string" },
                            suggestedFiles = new { type = "array", items = new { type = "string" } },
                            purpose = new { type = "string" },
                            contributesTo = new
                            {
                                type = "array",
                                items = new { type = "string" },
                                description = "Which task types this step contributes to"
                            }
                        },
                        required = new[] { "order", "targetContext", "question", "suggestedFiles", "purpose", "contributesTo" },
                        additionalProperties = false
                    }
                },
                expectedOutputs = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            taskType = new { type = "string" },
                            description = new { type = "string" },
                            outputType = new { type = "string", @enum = new[] { "documentation", "codeChange", "analysisReport", "projectFile" } }
                        },
                        required = new[] { "taskType", "description", "outputType" },
                        additionalProperties = false
                    }
                }
            },
            required = new[] { "taskTypes", "reasoning", "steps", "expectedOutputs" },
            additionalProperties = false
        };
    }
}