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
            Description = "Read the content of a source file from the project",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["path"] = new()
                    {
                        Type = "string",
                        Description = "Relative path to the file from project root"
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
            Description = "Search for files matching a glob pattern",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["pattern"] = new()
                    {
                        Type = "string",
                        Description = "Glob pattern to match files (e.g., '**/*Repository*.cs')"
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
            Description = "List all source files belonging to a specific assembly/project",
            Parameters = new FunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    ["assembly_name"] = new()
                    {
                        Type = "string",
                        Description = "Name of the assembly (project name without .csproj)"
                    }
                },
                Required = ["assembly_name"],
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

        return tools;
    }
}