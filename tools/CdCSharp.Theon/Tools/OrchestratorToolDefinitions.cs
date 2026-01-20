using CdCSharp.Theon.AI;

namespace CdCSharp.Theon.Tools;

public static class OrchestratorToolDefinitions
{
    public static Tool QueryContext => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "query_context",
            Description = "Ask a specialized context to analyze code. The context will read files and provide analysis.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["context_name"] = new()
                    {
                        Type = "string",
                        Description = "Context to query",
                        Enum = ["CodeExplorer", "ArchitectureAnalyzer", "DependencyAnalyzer"]
                    },
                    ["question"] = new()
                    {
                        Type = "string",
                        Description = "Specific question for the context"
                    },
                    ["files"] = new()
                    {
                        Type = "string",
                        Description = "Optional: comma-separated file paths to examine"
                    }
                },
                Required = ["context_name", "question"],
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
            Description = "Propose a change to an existing file. Requires user confirmation.",
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
                        Description = "Complete new content of the file"
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
            Description = "Create a new file in the project. Applied immediately if modification is enabled.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["path"] = new()
                    {
                        Type = "string",
                        Description = "Relative path for the new file"
                    },
                    ["content"] = new()
                    {
                        Type = "string",
                        Description = "Content of the new file"
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
            Description = "Generate a documentation or report file. Saved to the output folder.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["folder"] = new()
                    {
                        Type = "string",
                        Description = "Subfolder in output directory"
                    },
                    ["filename"] = new()
                    {
                        Type = "string",
                        Description = "Output file name"
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

    public static List<Tool> All =>
    [
        QueryContext,
        ProposeFileChange,
        CreateProjectFile,
        GenerateOutputFile
    ];
}