using CdCSharp.Theon.Infrastructure;
using System.Text.Json;

namespace CdCSharp.Theon.Context;

/// <summary>
/// Resultado de validación de una llamada a herramienta antes de ejecutarla.
/// Permite detectar errores comunes y proporcionar feedback inmediato con sugerencias.
/// </summary>
public sealed record ToolCallValidation
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Suggestion { get; init; }
    public IReadOnlyList<string> AvailableOptions { get; init; } = Array.Empty<string>();

    public static ToolCallValidation Valid() => new() { IsValid = true };

    public static ToolCallValidation Invalid(string errorMessage, string? suggestion = null, IEnumerable<string>? options = null)
    {
        return new()
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            Suggestion = suggestion,
            AvailableOptions = options?.ToList() ?? []
        };
    }
}

/// <summary>
/// Validador de tool calls antes de su ejecución.
/// Detecta errores comunes y proporciona sugerencias concretas.
/// </summary>
public sealed class ToolCallValidator
{
    private readonly SharedProjectKnowledge _knowledge;
    private readonly IFileSystem _fileSystem;

    public ToolCallValidator(SharedProjectKnowledge knowledge, IFileSystem fileSystem)
    {
        _knowledge = knowledge;
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Valida una tool call antes de ejecutarla.
    /// </summary>
    public ToolCallValidation Validate(string toolName, Dictionary<string, JsonElement>? args)
    {
        return toolName switch
        {
            "read_file" => ValidateReadFile(args),
            "list_assembly_files" => ValidateListAssembly(args),
            "search_files" => ValidateSearchFiles(args),
            "delegate_to_context" => ValidateDelegateToContext(args),
            "explore_project_structure" => ValidateExploreProjectStructure(args),
            _ => ToolCallValidation.Valid()
        };
    }

    private ToolCallValidation ValidateReadFile(Dictionary<string, JsonElement>? args)
    {
        if (args == null || !args.TryGetValue("path", out JsonElement pathElement))
        {
            return ToolCallValidation.Invalid("Missing required argument 'path'");
        }

        string path = pathElement.GetString() ?? "";

        if (string.IsNullOrWhiteSpace(path))
        {
            return ToolCallValidation.Invalid("Path cannot be empty");
        }

        // Verificar que NO sea un directorio conocido
        IReadOnlyList<string> assemblyNames = _knowledge.GetAssemblyNames();
        if (assemblyNames.Any(name => path.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            List<string> filesInAssembly = _knowledge.FileIndex.Values
                .Where(f => f.Path.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                .Select(f => f.Path)
                .Take(5)
                .ToList();

            return ToolCallValidation.Invalid(
                $"'{path}' is an assembly/project name, not a file",
                "Use list_assembly_files to see all files in this assembly, or read_file with a specific file path",
                filesInAssembly.Select(f => $"Example: read_file('{f}')").ToList()
            );
        }

        // Verificar si parece un directorio (no tiene extensión)
        if (!Path.HasExtension(path))
        {
            List<string> matchingFiles = _knowledge.FindFilesByPattern($"{path}/**/*.cs").Take(5).ToList();

            if (matchingFiles.Any())
            {
                return ToolCallValidation.Invalid(
                    $"'{path}' looks like a directory. read_file requires a specific file path",
                    "Use search_files to find files in this directory, or specify a complete file path",
                    matchingFiles.Select(f => $"Example: read_file('{f}')").ToList()
                );
            }
        }

        // Verificar que el archivo existe
        if (!_knowledge.FileExists(path))
        {
            List<string> similarFiles = _knowledge.FindSimilarFiles(path, 5).ToList();

            if (similarFiles.Any())
            {
                return ToolCallValidation.Invalid(
                    $"File '{path}' not found in project",
                    "Did you mean one of these files?",
                    similarFiles
                );
            }
            else
            {
                return ToolCallValidation.Invalid(
                    $"File '{path}' not found in project",
                    "Use explore_project_structure or search_files to find available files"
                );
            }
        }

        return ToolCallValidation.Valid();
    }

    private ToolCallValidation ValidateExploreProjectStructure(Dictionary<string, JsonElement>? args)
    {
        if (args == null || !args.TryGetValue("detail_level", out JsonElement levelElement))
        {
            // detail_level es opcional, usar default
            return ToolCallValidation.Valid();
        }

        string detailLevel = levelElement.GetString() ?? "";

        if (!string.IsNullOrEmpty(detailLevel) &&
            detailLevel != "summary" &&
            detailLevel != "types" &&
            detailLevel != "full")
        {
            return ToolCallValidation.Invalid(
                $"Invalid detail_level '{detailLevel}'",
                "Valid values are: 'summary', 'types', 'full'"
            );
        }

        return ToolCallValidation.Valid();
    }

    private ToolCallValidation ValidateListAssembly(Dictionary<string, JsonElement>? args)
    {
        if (args == null || !args.TryGetValue("assembly_name", out JsonElement nameElement))
        {
            return ToolCallValidation.Invalid("Missing required argument 'assembly_name'");
        }

        string assemblyName = nameElement.GetString() ?? "";

        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return ToolCallValidation.Invalid(
                "Assembly name cannot be empty",
                "Available assemblies:",
                _knowledge.GetAssemblyNames()
            );
        }

        // Verificar que el ensamblado existe
        IReadOnlyList<string> assemblies = _knowledge.GetAssemblyNames();
        if (!assemblies.Any(a => a.Equals(assemblyName, StringComparison.OrdinalIgnoreCase)))
        {
            // Buscar nombres similares
            List<string> similar = assemblies
                .Where(a => a.Contains(assemblyName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (similar.Any())
            {
                return ToolCallValidation.Invalid(
                    $"Assembly '{assemblyName}' not found",
                    "Did you mean one of these?",
                    similar
                );
            }
            else
            {
                return ToolCallValidation.Invalid(
                    $"Assembly '{assemblyName}' not found",
                    "Available assemblies:",
                    assemblies
                );
            }
        }

        return ToolCallValidation.Valid();
    }

    private ToolCallValidation ValidateSearchFiles(Dictionary<string, JsonElement>? args)
    {
        if (args == null || !args.TryGetValue("pattern", out JsonElement patternElement))
        {
            return ToolCallValidation.Invalid("Missing required argument 'pattern'");
        }

        string pattern = patternElement.GetString() ?? "";

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return ToolCallValidation.Invalid(
                "Pattern cannot be empty",
                "Examples: '**/*Repository*.cs', 'Domain/**/*.cs', '**/*Service.cs'"
            );
        }

        // Advertir si el patrón es demasiado amplio
        if (pattern is "*" or "**/*" or "**")
        {
            return ToolCallValidation.Invalid(
                "Pattern is too broad and will return too many results",
                "Be more specific. Examples: '**/*.cs', 'Domain/**/*.cs', '**/*Repository*.cs'"
            );
        }

        return ToolCallValidation.Valid();
    }

    private ToolCallValidation ValidateDelegateToContext(Dictionary<string, JsonElement>? args)
    {
        if (args == null)
        {
            return ToolCallValidation.Invalid("Missing arguments for delegate_to_context");
        }

        if (!args.TryGetValue("target_context", out JsonElement targetElement))
        {
            return ToolCallValidation.Invalid(
                "Missing required argument 'target_context'",
                "Available contexts: CodeExplorer, ArchitectureAnalyzer, DependencyAnalyzer"
            );
        }

        if (!args.TryGetValue("question", out JsonElement questionElement))
        {
            return ToolCallValidation.Invalid("Missing required argument 'question'");
        }

        string question = questionElement.GetString() ?? "";

        if (string.IsNullOrWhiteSpace(question))
        {
            return ToolCallValidation.Invalid("Question cannot be empty when delegating to another context");
        }

        // Advertir si la pregunta es demasiado vaga
        if (question.Length < 10)
        {
            return ToolCallValidation.Invalid(
                "Question is too vague for delegation",
                "Provide a specific, focused question. Example: 'What design patterns are used in the Domain layer?' instead of 'analyze'"
            );
        }

        return ToolCallValidation.Valid();
    }
}