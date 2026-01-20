using CdCSharp.Theon.AI;

namespace CdCSharp.Theon.Context.Tools;

public static class ContextTools
{
    public static Tool ReadFile => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "read_file",
            Description = "Read a file and ADD it to your context permanently. Use exact paths from the File Index. The file will consume your token budget.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["path"] = new()
                    {
                        Type = "string",
                        Description = "Exact file path from the File Index (e.g., 'Context/Context.cs')"
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
            Description = "View a file's content temporarily WITHOUT adding it to your context. Use this when another context has the file loaded, or when you only need to see it once. Does NOT consume your token budget permanently.",
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
                        Description = "Optional: specific context that has this file loaded. If omitted, searches all contexts then falls back to disk."
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
            Description = "Search for files matching a pattern. Returns matching paths that you can then read or peek.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["pattern"] = new()
                    {
                        Type = "string",
                        Description = "Glob pattern (e.g., '**/*Repository*.cs', 'Context/**/*.cs')"
                    }
                },
                Required = ["pattern"],
                AdditionalProperties = false
            }
        }
    };

    public static Tool SpawnClone => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "spawn_clone",
            Description = "Create a clone of yourself to handle a subset of work. Use when you need to analyze more files than fit in your budget, or to parallelize analysis. The clone has its own budget and persists for future queries.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["question"] = new()
                    {
                        Type = "string",
                        Description = "Specific question for the clone to answer"
                    },
                    ["files"] = new()
                    {
                        Type = "string",
                        Description = "Comma-separated list of exact file paths the clone should load and analyze"
                    },
                    ["purpose"] = new()
                    {
                        Type = "string",
                        Description = "Brief description of why this clone is needed (for tracking)"
                    }
                },
                Required = ["question", "files"],
                AdditionalProperties = false
            }
        }
    };

    public static Tool DelegateToContext => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "delegate_to_context",
            Description = "Ask a DIFFERENT type of context for its expertise. Use for cross-domain questions. Check 'Active Contexts' in your prompt to see what contexts exist and what files they have loaded.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["target_context"] = new()
                    {
                        Type = "string",
                        Description = "Name of the context to ask (from Active Contexts list)"
                    },
                    ["question"] = new()
                    {
                        Type = "string",
                        Description = "Specific question for that context"
                    },
                    ["relevant_files"] = new()
                    {
                        Type = "string",
                        Description = "Optional: comma-separated file paths the target should examine"
                    }
                },
                Required = ["target_context", "question"],
                AdditionalProperties = false
            }
        }
    };

    public static List<Tool> GetTools(ContextConfiguration config)
    {
        List<Tool> tools = [];

        if (config.CanReadFiles)
        {
            tools.Add(ReadFile);
            tools.Add(PeekFile);
        }

        if (config.CanSearchFiles)
            tools.Add(SearchFiles);

        if (config.CanSpawnClones)
            tools.Add(SpawnClone);

        if (config.CanDelegateToContexts)
            tools.Add(DelegateToContext);

        return tools;
    }
}