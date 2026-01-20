using CdCSharp.Theon.AI;

namespace CdCSharp.Theon.Context;

public static class ContextToolDefinitions
{
    public static Tool ReadFile => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "read_file",
            Description = "Load a file PERMANENTLY into your context. Uses your token budget. File will be available for ALL future responses. Use exact paths from File Index.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["path"] = new()
                    {
                        Type = "string",
                        Description = "Exact file path from the File Index"
                    }
                },
                Required = ["path"],
                AdditionalProperties = false
            }
        }
    };

    public static Tool PeekFile => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "peek_file",
            Description = "View a file TEMPORARILY for ONLY the next response. Does NOT consume budget. Does NOT persist in context. Ideal for quick inspection or files already loaded by other contexts.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["path"] = new()
                    {
                        Type = "string",
                        Description = "Exact file path to peek"
                    },
                    ["source_context"] = new()
                    {
                        Type = "string",
                        Description = "Optional: context that has this file loaded (faster)"
                    }
                },
                Required = ["path"],
                AdditionalProperties = false
            }
        }
    };

    public static Tool SearchFiles => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "search_files",
            Description = "Search for files matching a pattern. Returns paths you can then read or peek.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["pattern"] = new()
                    {
                        Type = "string",
                        Description = "Glob pattern (e.g., '**/*.cs', 'Context/**/*.cs')"
                    }
                },
                Required = ["pattern"],
                AdditionalProperties = false
            }
        }
    };

    public static Tool CreateSubContext => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "create_sub_context",
            Description = "Create a sub-context (clone or delegate) to analyze additional files or query different expertise.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["context_type"] = new()
                    {
                        Type = "string",
                        Description = "'clone' (same type, new budget) or 'delegate' (different expertise)",
                        Enum = ["clone", "delegate"]
                    },
                    ["question"] = new()
                    {
                        Type = "string",
                        Description = "Question for the sub-context"
                    },
                    ["files"] = new()
                    {
                        Type = "string",
                        Description = "Comma-separated file paths to analyze"
                    },
                    ["target_type"] = new()
                    {
                        Type = "string",
                        Description = "Target context type (required for 'delegate')",
                        Enum = ["CodeExplorer", "ArchitectureAnalyzer", "DependencyAnalyzer"]
                    }
                },
                Required = ["context_type", "question", "files"],
                AdditionalProperties = false
            }
        }
    };

    public static List<Tool> GetTools(ContextConfiguration config)
    {
        List<Tool> tools = new();

        if (config.CanPeekFiles)
        {
            tools.Add(PeekFile);
        }

        if (config.CanReadFiles)
        {
            tools.Add(ReadFile);
        }

        if (config.CanSearchFiles)
        {
            tools.Add(SearchFiles);
        }

        if (config.CanSpawnClones || config.CanDelegateToContexts)
        {
            tools.Add(CreateSubContext);
        }

        return tools;
    }
}