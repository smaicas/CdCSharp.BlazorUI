using CdCSharp.Theon.Infrastructure;
using System.Text.Json;

namespace CdCSharp.Theon.Context;

public sealed record ToolCallValidation
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Suggestion { get; init; }
    public IReadOnlyList<string>? AvailableOptions { get; init; }

    public static ToolCallValidation Valid() => new() { IsValid = true };

    public static ToolCallValidation Invalid(string errorMessage, string? suggestion = null, IEnumerable<string>? options = null)
    {
        return new()
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            Suggestion = suggestion,
            AvailableOptions = options?.ToList()
        };
    }
}

public sealed class ToolCallValidator
{
    private readonly SharedProjectKnowledge _knowledge;
    private readonly IFileSystem _fileSystem;

    public ToolCallValidator(SharedProjectKnowledge knowledge, IFileSystem fileSystem)
    {
        _knowledge = knowledge;
        _fileSystem = fileSystem;
    }

    public ToolCallValidation Validate(string toolName, Dictionary<string, JsonElement>? args)
    {
        return toolName switch
        {
            "read_file" => ValidateReadFile(args),
            "search_files" => ValidateSearchFiles(args),
            "delegate_to_context" => ValidateDelegateToContext(args),
            _ => ToolCallValidation.Valid()
        };
    }

    private ToolCallValidation ValidateReadFile(Dictionary<string, JsonElement>? args)
    {
        if (args == null || !args.TryGetValue("path", out JsonElement pathElement))
        {
            return ToolCallValidation.Invalid(
                "Missing required argument 'path'",
                "Provide an exact file path from the File Index",
                _knowledge.GetAllFilePaths().Take(10));
        }

        string path = pathElement.GetString() ?? "";

        if (string.IsNullOrWhiteSpace(path))
        {
            return ToolCallValidation.Invalid(
                "Path cannot be empty",
                "Use an exact file path from the File Index in your system prompt",
                _knowledge.GetAllFilePaths().Take(10));
        }

        if (path == "*" || path == "**" || path.Contains("*"))
        {
            return ToolCallValidation.Invalid(
                $"Invalid path '{path}': patterns/globs are not allowed in read_file",
                "Use search_files to find files by pattern, then read_file with the exact path",
                _knowledge.GetAllFilePaths().Take(10));
        }

        if (!Path.HasExtension(path))
        {
            List<string> matchingFiles = _knowledge.FindFilesByPattern($"**/{path}/**/*.cs").Take(5).ToList();

            if (!matchingFiles.Any())
                matchingFiles = _knowledge.FindFilesByPattern($"**/{path}*.cs").Take(5).ToList();

            return ToolCallValidation.Invalid(
                $"'{path}' appears to be a directory, not a file",
                "Use search_files to find files, or provide a complete file path with extension",
                matchingFiles.Any() ? matchingFiles : _knowledge.GetAllFilePaths().Take(10));
        }

        if (!_knowledge.FileExists(path))
        {
            List<string> similarFiles = _knowledge.FindSimilarFiles(path, 5).ToList();

            string suggestion = similarFiles.Any()
                ? "Did you mean one of these files?"
                : "Check the File Index in your system prompt for available files";

            return ToolCallValidation.Invalid(
                $"File not found: '{path}'",
                suggestion,
                similarFiles.Any() ? similarFiles : _knowledge.GetAllFilePaths().Take(10));
        }

        return ToolCallValidation.Valid();
    }

    private ToolCallValidation ValidateSearchFiles(Dictionary<string, JsonElement>? args)
    {
        if (args == null || !args.TryGetValue("pattern", out JsonElement patternElement))
        {
            return ToolCallValidation.Invalid(
                "Missing required argument 'pattern'",
                "Provide a glob pattern like '**/*Repository*.cs' or 'Context/**/*.cs'");
        }

        string pattern = patternElement.GetString() ?? "";

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return ToolCallValidation.Invalid(
                "Pattern cannot be empty",
                "Examples: '**/*Repository*.cs', 'Context/**/*.cs', '**/*Service*.cs'");
        }

        if (pattern is "*" or "**")
        {
            return ToolCallValidation.Invalid(
                "Pattern is too broad",
                "Be more specific. Examples: '**/*.cs', 'Context/**/*.cs', '**/*Service*.cs'");
        }

        return ToolCallValidation.Valid();
    }

    private ToolCallValidation ValidateDelegateToContext(Dictionary<string, JsonElement>? args)
    {
        if (args == null)
        {
            return ToolCallValidation.Invalid(
                "Missing arguments for delegate_to_context",
                "Required: target_context, question. Optional: relevant_files");
        }

        if (!args.TryGetValue("target_context", out JsonElement targetElement))
        {
            return ToolCallValidation.Invalid(
                "Missing required argument 'target_context'",
                "Available contexts: CodeExplorer, ArchitectureAnalyzer, DependencyAnalyzer");
        }

        string target = targetElement.GetString() ?? "";
        if (!new[] { "CodeExplorer", "ArchitectureAnalyzer", "DependencyAnalyzer" }.Contains(target))
        {
            return ToolCallValidation.Invalid(
                $"Unknown context: '{target}'",
                "Available contexts: CodeExplorer, ArchitectureAnalyzer, DependencyAnalyzer");
        }

        if (!args.TryGetValue("question", out JsonElement questionElement))
        {
            return ToolCallValidation.Invalid(
                "Missing required argument 'question'",
                "Provide a specific question for the target context");
        }

        string question = questionElement.GetString() ?? "";
        if (question.Length < 10)
        {
            return ToolCallValidation.Invalid(
                "Question is too vague",
                "Provide a specific, detailed question. Include relevant file paths when possible.");
        }

        return ToolCallValidation.Valid();
    }
}