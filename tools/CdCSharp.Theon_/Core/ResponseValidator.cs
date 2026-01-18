// Core/ResponseValidator.cs
namespace CdCSharp.Theon.Core;

public interface IResponseValidator
{
    ValidationResult Validate(ParsedResponse response, int explorationCount, string? taskType = null);
}

public sealed record ValidationResult(
    bool IsValid,
    float AdjustedConfidence,
    string? Reason,
    ValidationIssue[] Issues)
{
    public bool HasIssues => Issues.Length > 0;
}

public sealed record ValidationIssue(
    ValidationSeverity Severity,
    string Category,
    string Description,
    float ConfidencePenalty);

public enum ValidationSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public sealed class ResponseValidator : IResponseValidator
{
    private static readonly string[] GenericPhrases =
    [
        "designed to provide",
        "includes features such as",
        "basic example",
        "refer to the documentation",
        "for more information",
        "see our guidelines",
        "tools for code analysis",
        "perform static analysis",
        "provides functionality",
        "allows you to",
        "helps you",
        "makes it easy",
        "simple and intuitive",
        "powerful features",
        "comprehensive solution",
        "state-of-the-art",
        "cutting-edge",
        "best practices",
        "industry standard",
        "robust and scalable"
    ];

    private static readonly string[] InventionIndicators =
    [
        "for example, you can",
        "such as X, Y, and Z",
        "including but not limited to",
        "and many more",
        "etc.",
        "and so on"
    ];

    private static readonly Dictionary<string, int> MinimumExplorationsByTask = new()
    {
        ["documentation"] = 4,
        ["architecture"] = 5,
        ["review"] = 3,
        ["analysis"] = 3,
        ["explain"] = 2,
        ["bug"] = 2,
        ["default"] = 1
    };

    public ValidationResult Validate(ParsedResponse response, int explorationCount, string? taskType = null)
    {
        List<ValidationIssue> issues = [];

        float confidence = response.Confidence ?? 0f;

        ValidateExplorationDepth(confidence, explorationCount, taskType, issues);
        ValidateSpecificity(response.Content, confidence, issues);
        ValidateAuthenticity(response.Content, issues);
        ValidateConfidenceRealism(confidence, explorationCount, issues);
        ValidateOutputPrerequisites(response, explorationCount, issues);

        float adjustedConfidence = CalculateAdjustedConfidence(confidence, issues);
        bool isValid = !issues.Any(i => i.Severity == ValidationSeverity.Critical);
        string? reason = GenerateValidationReason(issues);

        return new ValidationResult(isValid, adjustedConfidence, reason, issues.ToArray());
    }

    private void ValidateExplorationDepth(
        float confidence,
        int explorationCount,
        string? taskType,
        List<ValidationIssue> issues)
    {
        int minRequired = GetMinimumExplorationForTask(taskType);

        if (explorationCount == 0 && confidence > 0.3f)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Critical,
                "No Exploration",
                $"Claimed confidence {confidence:F2} without exploring any code.",
                0.7f));
        }
        else if (explorationCount < minRequired && confidence > 0.6f)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Error,
                "Insufficient Exploration",
                $"Task requires at least {minRequired} explorations, but only {explorationCount} were performed.",
                0.3f));
        }
    }

    private void ValidateSpecificity(string content, float confidence, List<ValidationIssue> issues)
    {
        string lowerContent = content.ToLowerInvariant();

        List<string> foundGenericPhrases = GenericPhrases
            .Where(phrase => lowerContent.Contains(phrase.ToLowerInvariant()))
            .ToList();

        if (foundGenericPhrases.Count >= 3)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Warning,
                "Generic Content",
                $"Response contains {foundGenericPhrases.Count} generic phrases.",
                Math.Min(0.5f, foundGenericPhrases.Count * 0.1f)));
        }

        if (content.Length < 200 && confidence > 0.7f)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Warning,
                "Insufficient Detail",
                "Response is very brief but claims high confidence.",
                0.2f));
        }
    }

    private void ValidateAuthenticity(string content, List<ValidationIssue> issues)
    {
        string lowerContent = content.ToLowerInvariant();

        List<string> found = InventionIndicators
            .Where(indicator => lowerContent.Contains(indicator.ToLowerInvariant()))
            .ToList();

        if (found.Count >= 2)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Warning,
                "Potential Invention",
                "Response contains phrases that often indicate invented examples.",
                0.15f));
        }
    }

    private void ValidateConfidenceRealism(float confidence, int explorationCount, List<ValidationIssue> issues)
    {
        float expectedMax = explorationCount switch
        {
            0 => 0.3f,
            1 => 0.5f,
            2 => 0.65f,
            3 => 0.75f,
            4 => 0.85f,
            >= 5 => 0.95f,
            _ => 0.3f
        };

        if (confidence > expectedMax + 0.15f)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Error,
                "Unrealistic Confidence",
                $"Confidence {confidence:F2} is too high for {explorationCount} explorations.",
                confidence - expectedMax));
        }
    }

    private void ValidateOutputPrerequisites(ParsedResponse response, int explorationCount, List<ValidationIssue> issues)
    {
        bool hasOutput = response.GeneratedFiles.Count > 0;

        if (hasOutput && explorationCount == 0)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Critical,
                "Output Without Exploration",
                "Generating files without exploring any code.",
                0.6f));
        }
    }

    private float CalculateAdjustedConfidence(float original, List<ValidationIssue> issues)
    {
        if (issues.Count == 0)
            return original;

        float totalPenalty = issues.Sum(i => i.ConfidencePenalty);
        return Math.Max(0.2f, original - totalPenalty);
    }

    private string? GenerateValidationReason(List<ValidationIssue> issues)
    {
        if (issues.Count == 0)
            return null;

        List<ValidationIssue> critical = issues.Where(i => i.Severity == ValidationSeverity.Critical).ToList();
        if (critical.Count > 0)
            return $"CRITICAL: {string.Join("; ", critical.Select(i => i.Description))}";

        List<ValidationIssue> errors = issues.Where(i => i.Severity == ValidationSeverity.Error).ToList();
        if (errors.Count > 0)
            return $"ERRORS: {string.Join("; ", errors.Select(i => i.Description))}";

        List<ValidationIssue> warnings = issues.Where(i => i.Severity == ValidationSeverity.Warning).ToList();
        return $"WARNINGS: {string.Join("; ", warnings.Select(i => i.Category))}";
    }

    private int GetMinimumExplorationForTask(string? taskType)
    {
        if (string.IsNullOrWhiteSpace(taskType))
            return MinimumExplorationsByTask["default"];

        string normalized = taskType.ToLowerInvariant();

        foreach ((string key, int value) in MinimumExplorationsByTask)
        {
            if (normalized.Contains(key))
                return value;
        }

        return MinimumExplorationsByTask["default"];
    }
}