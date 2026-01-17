namespace CdCSharp.Theon.Core;

/// <summary>
/// Validates LLM responses to ensure quality and accuracy standards are met.
/// Detects generic content, unrealistic confidence, and insufficient exploration.
/// </summary>
public interface IResponseValidator
{
    /// <summary>
    /// Validates a parsed LLM response against quality criteria.
    /// </summary>
    /// <param name="response">The parsed response from the LLM</param>
    /// <param name="explorationCount">Number of exploration tools used before this response</param>
    /// <param name="taskType">Type of task being performed (optional, for context-aware validation)</param>
    /// <returns>Validation result with adjusted confidence and feedback</returns>
    ValidationResult Validate(ParseResult response, int explorationCount, string? taskType = null);
}

/// <summary>
/// Result of response validation with quality assessment.
/// </summary>
public sealed record ValidationResult(
    bool IsValid,
    float AdjustedConfidence,
    string? Reason,
    ValidationIssue[] Issues)
{
    public bool HasIssues => Issues.Length > 0;
}

/// <summary>
/// Specific validation issue found in the response.
/// </summary>
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
    // Generic phrases that indicate lack of specific knowledge
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

    // Patterns that suggest invented content
    private static readonly string[] InventionIndicators =
    [
        "for example, you can",
        "such as X, Y, and Z",
        "including but not limited to",
        "and many more",
        "etc.",
        "and so on"
    ];

    // Task-specific minimum exploration requirements
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

    public ValidationResult Validate(ParseResult response, int explorationCount, string? taskType = null)
    {
        List<ValidationIssue> issues = [];

        // Rule 1: Confidence without exploration
        ValidateExplorationDepth(response, explorationCount, taskType, issues);

        // Rule 2: Generic content detection
        ValidateSpecificity(response, issues);

        // Rule 3: Invention indicators
        ValidateAuthenticity(response, issues);

        // Rule 4: Confidence-exploration correlation
        ValidateConfidenceRealism(response, explorationCount, issues);

        // Rule 5: Output generation without prior exploration
        ValidateOutputPrerequisites(response, explorationCount, issues);

        // Calculate adjusted confidence based on issues
        float adjustedConfidence = CalculateAdjustedConfidence(response.Confidence, issues);

        // Determine if response is valid (no critical issues)
        bool isValid = !issues.Any(i => i.Severity == ValidationSeverity.Critical);

        // Generate summary reason
        string? reason = GenerateValidationReason(issues);

        return new ValidationResult(isValid, adjustedConfidence, reason, issues.ToArray());
    }

    private void ValidateExplorationDepth(
        ParseResult response,
        int explorationCount,
        string? taskType,
        List<ValidationIssue> issues)
    {
        int minRequired = GetMinimumExplorationForTask(taskType);

        if (explorationCount == 0 && response.Confidence > 0.3f)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Critical,
                "No Exploration",
                $"Claimed confidence {response.Confidence:F2} without exploring any code. " +
                "This indicates invented or generic content.",
                0.7f
            ));
        }
        else if (explorationCount < minRequired && response.Confidence > 0.6f)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Error,
                "Insufficient Exploration",
                $"Task type '{taskType ?? "general"}' requires at least {minRequired} explorations, " +
                $"but only {explorationCount} were performed. High confidence is unrealistic.",
                0.3f
            ));
        }
    }

    private void ValidateSpecificity(ParseResult response, List<ValidationIssue> issues)
    {
        string content = response.CleanContent.ToLowerInvariant();

        List<string> foundGenericPhrases = GenericPhrases
            .Where(phrase => content.Contains(phrase.ToLowerInvariant()))
            .ToList();

        if (foundGenericPhrases.Count >= 3)
        {
            float penalty = Math.Min(0.5f, foundGenericPhrases.Count * 0.1f);

            issues.Add(new ValidationIssue(
                ValidationSeverity.Warning,
                "Generic Content",
                $"Response contains {foundGenericPhrases.Count} generic phrases: " +
                $"[{string.Join(", ", foundGenericPhrases.Take(3))}...]. " +
                "This suggests lack of specific knowledge about the actual codebase.",
                penalty
            ));
        }

        // Check for very short responses with high confidence
        if (response.CleanContent.Length < 200 && response.Confidence > 0.7f)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Warning,
                "Insufficient Detail",
                "Response is very brief but claims high confidence. " +
                "Detailed analysis should provide more information.",
                0.2f
            ));
        }
    }

    private void ValidateAuthenticity(ParseResult response, List<ValidationIssue> issues)
    {
        string content = response.CleanContent.ToLowerInvariant();

        List<string> inventionIndicators = InventionIndicators
            .Where(indicator => content.Contains(indicator.ToLowerInvariant()))
            .ToList();

        if (inventionIndicators.Count >= 2)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Warning,
                "Potential Invention",
                $"Response contains phrases that often indicate invented examples: " +
                $"[{string.Join(", ", inventionIndicators)}]. " +
                "Ensure all information is from explored code.",
                0.15f
            ));
        }
    }

    private void ValidateConfidenceRealism(
        ParseResult response,
        int explorationCount,
        List<ValidationIssue> issues)
    {
        // Expected maximum confidence based on exploration depth
        float expectedMaxConfidence = explorationCount switch
        {
            0 => 0.3f,
            1 => 0.5f,
            2 => 0.65f,
            3 => 0.75f,
            4 => 0.85f,
            >= 5 => 0.95f,
            _ => 0.3f
        };

        // Allow some tolerance
        float tolerance = 0.15f;

        if (response.Confidence > expectedMaxConfidence + tolerance)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Error,
                "Unrealistic Confidence",
                $"Confidence {response.Confidence:F2} is too high for {explorationCount} explorations. " +
                $"Expected maximum: ~{expectedMaxConfidence:F2}. " +
                "Confidence should reflect actual code examination.",
                response.Confidence - expectedMaxConfidence
            ));
        }
    }

    private void ValidateOutputPrerequisites(
        ParseResult response,
        int explorationCount,
        List<ValidationIssue> issues)
    {
        // Check if generating files without prior exploration
        bool hasOutputTools = response.Tools.Any(t =>
            t is GenerateFileTool or OverwriteFileTool or ModifyProjectFileTool);

        if (hasOutputTools && explorationCount == 0)
        {
            issues.Add(new ValidationIssue(
                ValidationSeverity.Critical,
                "Output Without Exploration",
                "Attempting to generate files without exploring any code. " +
                "This will result in generic, inaccurate content.",
                0.6f
            ));
        }
    }

    private float CalculateAdjustedConfidence(float originalConfidence, List<ValidationIssue> issues)
    {
        if (issues.Count == 0)
            return originalConfidence;

        // Sum all penalties
        float totalPenalty = issues.Sum(i => i.ConfidencePenalty);

        // Apply penalty with a floor
        float adjusted = Math.Max(0.2f, originalConfidence - totalPenalty);

        return adjusted;
    }

    private string? GenerateValidationReason(List<ValidationIssue> issues)
    {
        if (issues.Count == 0)
            return null;

        List<ValidationIssue> criticalIssues = issues.Where(i => i.Severity == ValidationSeverity.Critical).ToList();
        List<ValidationIssue> errorIssues = issues.Where(i => i.Severity == ValidationSeverity.Error).ToList();

        if (criticalIssues.Count > 0)
        {
            return $"CRITICAL: {string.Join("; ", criticalIssues.Select(i => i.Description))}";
        }

        if (errorIssues.Count > 0)
        {
            return $"ERRORS: {string.Join("; ", errorIssues.Select(i => i.Description))}";
        }

        List<ValidationIssue> warnings = issues.Where(i => i.Severity == ValidationSeverity.Warning).ToList();
        return $"WARNINGS: {string.Join("; ", warnings.Select(i => i.Category))}";
    }

    private int GetMinimumExplorationForTask(string? taskType)
    {
        if (string.IsNullOrWhiteSpace(taskType))
            return MinimumExplorationsByTask["default"];

        string normalized = taskType.ToLowerInvariant();

        foreach ((string? key, int value) in MinimumExplorationsByTask)
        {
            if (normalized.Contains(key))
                return value;
        }

        return MinimumExplorationsByTask["default"];
    }
}