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
            Description = "Read the complete source code of a file from the project. Use this when you need to examine implementation details, understand algorithms, or analyze specific code patterns. IMPORTANT: You must provide an exact file path (e.g., 'Domain/Entities/User.cs'), not a directory name.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["path"] = new()
                    {
                        Type = "string",
                        Description = "Exact relative path to the file from project root (e.g., 'Domain/Entities/User.cs'). Must be a complete file path with extension, not a directory."
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
            Description = "Search for files matching a glob pattern. Useful for finding all files of a certain type (e.g., all repositories, all controllers, all tests).",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["pattern"] = new()
                    {
                        Type = "string",
                        Description = "Glob pattern to match files (e.g., '**/*Repository*.cs' for all repository files, '**/Controllers/*.cs' for all controllers, 'Domain/**/*.cs' for all files in Domain)"
                    }
                },
                Required = ["pattern"],
                AdditionalProperties = false
            }
        }
    };

    public static Tool ListAssemblyFiles => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "list_assembly_files",
            Description = "List all source files and type information for a specific assembly/project. Use this to understand the complete structure of a project.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["assembly_name"] = new()
                    {
                        Type = "string",
                        Description = "Name of the assembly (project name without .csproj extension). Check the Project Structure in the system prompt to see available assemblies."
                    }
                },
                Required = ["assembly_name"],
                AdditionalProperties = false
            }
        }
    };

    public static Tool ExploreProjectStructure => new()
    {
        Type = "function",
        Function = new FunctionDefinition
        {
            Name = "explore_project_structure",
            Description = "Get a detailed overview of the project structure. CALL THIS FIRST if you need to understand what files and types exist before reading specific files. The Project Structure in your system prompt provides a compact summary, but this tool gives you more detail when needed.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["detail_level"] = new()
                    {
                        Type = "string",
                        Description = "Level of detail: 'summary' (assemblies and top namespaces only - use for initial orientation), 'types' (include all type names - use when you need to find specific classes/interfaces), 'full' (include member signatures - use when you need detailed API information)",
                        Enum = ["summary", "types", "full"]
                    }
                },
                Required = ["detail_level"],
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
            Description = "Delegate a specific question to another specialized context when you need expertise from a different domain. Use this to collaborate with other contexts rather than making assumptions. IMPORTANT: Only delegate when you genuinely need another perspective - use your own tools (read_file, search_files, explore_project_structure) first.",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["target_context"] = new()
                    {
                        Type = "string",
                        Description = "Name of the context to consult. Available: CodeExplorer (implementation details, algorithms, patterns), ArchitectureAnalyzer (structure, layers, design), DependencyAnalyzer (type relationships, dependencies)",
                        Enum = ["CodeExplorer", "ArchitectureAnalyzer", "DependencyAnalyzer"]
                    },
                    ["question"] = new()
                    {
                        Type = "string",
                        Description = "Specific, focused question to ask the target context. Be clear and precise. Minimum 10 characters. Include relevant file paths when possible."
                    },
                    ["relevant_files"] = new()
                    {
                        Type = "string",
                        Description = "Optional: comma-separated list of file paths the target context should examine (e.g., 'Domain/User.cs,Infrastructure/UserRepository.cs')"
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
            tools.Add(ReadFile);

        if (config.CanSearchFiles)
            tools.Add(SearchFiles);

        if (config.CanListAssemblies)
            tools.Add(ListAssemblyFiles);

        tools.Add(ExploreProjectStructure);

        if (config.CanDelegateToContexts)
            tools.Add(DelegateToContext);

        return tools;
    }
}