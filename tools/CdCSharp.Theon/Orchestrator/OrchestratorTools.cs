using CdCSharp.Theon.AI;

namespace CdCSharp.Theon.Orchestrator;

public static class OrchestratorTools
{
    public static Tool QueryContext => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "query_context",
            Description = "Query a specialized context to get information about the codebase. Contexts are experts that can read and analyze code.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["context_name"] = new()
                    {
                        Type = "string",
                        Description = "Name of the context to query (CodeExplorer, ArchitectureAnalyzer, DependencyAnalyzer, or a custom context name)"
                    },
                    ["question"] = new()
                    {
                        Type = "string",
                        Description = "The question to ask the context"
                    },
                    ["files"] = new()
                    {
                        Type = "string",
                        Description = "Optional: comma-separated list of file paths to include in the context scope"
                    }
                },
                Required = ["context_name", "question"],
                AdditionalProperties = false
            }
        }
    };

    public static Tool CreateDynamicContext => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "create_dynamic_context",
            Description = "Create a new specialized context for a specific purpose",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["name"] = new()
                    {
                        Type = "string",
                        Description = "Unique name for the context"
                    },
                    ["purpose"] = new()
                    {
                        Type = "string",
                        Description = "Description of what this context specializes in"
                    },
                    ["stateful"] = new()
                    {
                        Type = "string",
                        Description = "Whether the context should maintain conversation history (true/false)"
                    }
                },
                Required = ["name", "purpose"],
                AdditionalProperties = false
            }
        }
    };

    public static Tool ListContexts => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "list_contexts",
            Description = "List all available contexts and their current state",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = [],
                AdditionalProperties = false
            }
        }
    };

    public static Tool ProposeFileChange => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "propose_file_change",
            Description = "Propose a change to an existing file. The change will require user confirmation before being applied.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["path"] = new()
                    {
                        Type = "string",
                        Description = "Relative path to the file"
                    },
                    ["description"] = new()
                    {
                        Type = "string",
                        Description = "Description of the change"
                    },
                    ["new_content"] = new()
                    {
                        Type = "string",
                        Description = "The complete new content of the file"
                    }
                },
                Required = ["path", "description", "new_content"],
                AdditionalProperties = false
            }
        }
    };

    public static Tool CreateProjectFile => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "create_project_file",
            Description = "Create a new source file in the project (e.g., new class, interface). Applied immediately if project modification is enabled.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["path"] = new()
                    {
                        Type = "string",
                        Description = "Relative path for the new file within the project"
                    },
                    ["content"] = new()
                    {
                        Type = "string",
                        Description = "Content of the new file"
                    },
                    ["description"] = new()
                    {
                        Type = "string",
                        Description = "Brief description of the file's purpose"
                    }
                },
                Required = ["path", "content"],
                AdditionalProperties = false
            }
        }
    };

    public static Tool GenerateOutputFile => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "generate_output_file",
            Description = "Generate an output file (documentation, report, export, analysis result). Saved to the output folder, not the project. Always allowed.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["folder"] = new()
                    {
                        Type = "string",
                        Description = "Subfolder name within the output directory (e.g., 'documentation', 'reports')"
                    },
                    ["filename"] = new()
                    {
                        Type = "string",
                        Description = "Name of the output file (e.g., 'api-docs.md', 'analysis.json')"
                    },
                    ["content"] = new()
                    {
                        Type = "string",
                        Description = "Content of the output file"
                    }
                },
                Required = ["folder", "filename", "content"],
                AdditionalProperties = false
            }
        }
    };

    public static Tool ApplyPendingChanges => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "apply_pending_changes",
            Description = "Apply pending file changes that were previously proposed and confirmed by the user",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["change_ids"] = new()
                    {
                        Type = "string",
                        Description = "Comma-separated list of change IDs to apply, or 'all' to apply all pending changes"
                    }
                },
                Required = ["change_ids"],
                AdditionalProperties = false
            }
        }
    };

    public static List<Tool> All =>
    [
        QueryContext,
        CreateDynamicContext,
        ListContexts,
        ProposeFileChange,
        CreateProjectFile,
        GenerateOutputFile,
        ApplyPendingChanges
    ];
}