using CdCSharp.Theon.Agents;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;

namespace CdCSharp.Theon.Orchestration;

/// <summary>
/// Manages the validation workflow for agent responses.
/// Creates dynamic validator agents and iteratively improves responses until confidence threshold is met.
/// </summary>
public class ValidationOrchestrator
{
    private readonly AgentFactory _agentFactory;
    private readonly AgentExecutor _agentExecutor;
    private readonly AgentRegistry _registry;
    private readonly TheonLogger _logger;
    private readonly TheonOptions _options;

    public ValidationOrchestrator(
        AgentFactory agentFactory,
        AgentExecutor agentExecutor,
        AgentRegistry registry,
        TheonLogger logger,
        TheonOptions options)
    {
        _agentFactory = agentFactory;
        _agentExecutor = agentExecutor;
        _registry = registry;
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Validates and potentially improves a response through iterative validation cycles.
    /// </summary>
    public async Task<ValidationResult> ValidateAndImproveAsync(
        AgentExecutionResult originalResult,
        string originalQuery,
        List<string> involvedAgents)
    {
        ValidationResult result = new()
        {
            FinalContent = originalResult.CleanContent,
            FinalConfidence = originalResult.Confidence,
            GeneratedFiles = [.. originalResult.GeneratedFiles],
            Iterations = 0,
            ValidationHistory = []
        };

        // If confidence is already high enough, skip validation
        if (originalResult.Confidence >= _options.Validation.ConfidenceThreshold)
        {
            _logger.Info($"Response confidence {originalResult.Confidence:P0} meets threshold, skipping validation");
            return result;
        }

        _logger.Info($"Starting validation process (confidence: {originalResult.Confidence:P0}, threshold: {_options.Validation.ConfidenceThreshold:P0})");

        // Get or create the master validator agent
        Agent validator = await GetOrCreateMasterValidatorAsync();
        involvedAgents.Add(validator.Name);

        for (int iteration = 1; iteration <= _options.Validation.MaxIterations; iteration++)
        {
            result.Iterations = iteration;
            _logger.Info($"Validation iteration {iteration}/{_options.Validation.MaxIterations}");

            // Build validation prompt with full context
            string validationPrompt = BuildValidationPrompt(
                originalQuery,
                result.FinalContent,
                result.GeneratedFiles,
                result.FinalConfidence,
                iteration);

            // Execute validation
            AgentExecutionResult validationResult = await ExecuteValidatorWithRequests(
                validator,
                validationPrompt,
                involvedAgents);

            // Record this validation step
            ValidationStep step = new()
            {
                Iteration = iteration,
                ValidatorResponse = validationResult.CleanContent,
                Confidence = validationResult.Confidence,
                Approved = validationResult.Confidence >= _options.Validation.ConfidenceThreshold,
                Suggestions = ExtractSuggestions(validationResult.CleanContent)
            };
            result.ValidationHistory.Add(step);

            // Update final content and confidence
            result.FinalContent = validationResult.CleanContent;
            result.FinalConfidence = validationResult.Confidence;

            // Merge any new generated files
            foreach (GeneratedFile file in validationResult.GeneratedFiles)
            {
                GeneratedFile? existing = result.GeneratedFiles.FirstOrDefault(f => f.FileName == file.FileName);
                if (existing != null)
                {
                    result.GeneratedFiles.Remove(existing);
                }
                result.GeneratedFiles.Add(file);
            }

            // Check if validation passed
            if (step.Approved)
            {
                _logger.Info($"Validation approved at iteration {iteration} with confidence {result.FinalConfidence:P0}");
                return result;
            }

            // If we haven't reached max iterations, validator will improve in next cycle
            if (iteration < _options.Validation.MaxIterations)
            {
                _logger.Debug($"Validation not approved (confidence: {result.FinalConfidence:P0}), continuing to next iteration");
            }
        }

        _logger.Warning($"Validation did not reach threshold after {_options.Validation.MaxIterations} iterations. Final confidence: {result.FinalConfidence:P0}");
        return result;
    }

    private async Task<Agent> GetOrCreateMasterValidatorAsync()
    {
        // Try to find existing master validator
        Agent? existing = _registry.FindByExpertise("master validation and quality assurance");

        if (existing != null)
        {
            if (existing.State == AgentState.Sleeping)
                _agentFactory.Wake(existing);
            return existing;
        }

        // Create new master validator
        _logger.Info("Creating Master Validator agent");
        return await _agentFactory.CreateAsync(new AgentCreationSpec
        {
            Name = "Master Validator",
            Expertise = "master validation and quality assurance",
            InitialContextFiles = []
        });
    }

    private string BuildValidationPrompt(
        string originalQuery,
        string currentContent,
        List<GeneratedFile> generatedFiles,
        float currentConfidence,
        int iteration)
    {
        string filesSection = generatedFiles.Count > 0
            ? $"""
            ## Generated Files
            {string.Join("\n", generatedFiles.Select(f => $"- {f.FileName} ({f.Content.Length} characters)"))}
            """
            : "";

        return $"""
        # Validation Task (Iteration {iteration})

        You are the Master Validator. Your role is to validate and improve responses to ensure they meet quality standards.

        ## Original Query
        {originalQuery}

        ## Current Response (Confidence: {currentConfidence:P0})
        {currentContent}

        {filesSection}

        ## Your Mission

        1. **Analyze the response** against the original query
        2. **Identify gaps, errors, or areas for improvement**
        3. **If confidence < {_options.Validation.ConfidenceThreshold:P0}:**
           - You MUST improve the response yourself
           - You can use all available tools:
             * [REQUEST_FILE: path="..."] to see files
             * [QUERY_AGENT: expertise="..." question="..."] to consult specialists
             * [CREATE_AGENT: name="..." expertise="..." files="..."] to create new specialists
             * [GENERATE_FILE: name="..." language="..."] to create/update files
             * [APPEND_TO_FILE: name="..."] to add to existing files
           - Produce an IMPROVED version of the response
        4. **Set your confidence level** based on the quality of your final output
        5. **Always end with [CONFIDENCE: X.X]**

        ## Confidence Guidelines
        - **0.9-1.0**: Response fully answers query, no gaps, high quality
        - **0.7-0.9**: Response is good but has minor issues
        - **0.5-0.7**: Response is incomplete or has significant gaps
        - **Below 0.5**: Response does not adequately answer the query

        ## Critical Rules
        - If you need expertise you don't have, CREATE specialized agents
        - If you need to verify facts, REQUEST files
        - If response is incomplete, IMPROVE it yourself
        - Your output becomes the new response for the next iteration
        - BE THOROUGH: This is iteration {iteration} of {_options.Validation.MaxIterations}

        Provide your improved response now.
        """;
    }

    private async Task<AgentExecutionResult> ExecuteValidatorWithRequests(
        Agent validator,
        string prompt,
        List<string> involvedAgents)
    {
        string agentsSummary = _registry.GetAgentsSummary();
        AgentExecutionResult result = await _agentExecutor.ExecuteAsync(validator, prompt, agentsSummary);

        // Handle any pending requests (file access, agent queries, etc.)
        // This is similar to Orchestrator.ExecuteWithRequestsAsync but simpler
        int maxDepth = 5;
        int depth = 0;

        while (result.HasPendingRequests && depth < maxDepth)
        {
            depth++;

            // For now, just log that validator made requests
            // Full implementation would handle file requests, agent queries, etc.
            _logger.Debug($"Validator made {result.FileRequests.Count + result.AgentQueries.Count} requests at depth {depth}");

            // TODO: Handle file requests
            // TODO: Handle agent queries
            // TODO: Handle agent creation

            // Re-execute with additional context
            // agentsSummary = _registry.GetAgentsSummary();
            // result = await _agentExecutor.ExecuteAsync(validator, continuation, agentsSummary);

            break; // Temporary - remove when implementing full request handling
        }

        return result;
    }

    private List<string> ExtractSuggestions(string validatorResponse)
    {
        List<string> suggestions = [];

        // Extract lines that look like suggestions
        string[] lines = validatorResponse.Split('\n');
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("-") || trimmed.StartsWith("*") || trimmed.StartsWith("•"))
            {
                suggestions.Add(trimmed.TrimStart('-', '*', '•', ' '));
            }
        }

        return suggestions;
    }
}

/// <summary>
/// Result of the validation process
/// </summary>
public record ValidationResult
{
    public string FinalContent { get; set; } = "";
    public float FinalConfidence { get; set; }
    public List<GeneratedFile> GeneratedFiles { get; set; } = [];
    public int Iterations { get; set; }
    public List<ValidationStep> ValidationHistory { get; set; } = [];
}

/// <summary>
/// Details of a single validation iteration
/// </summary>
public record ValidationStep
{
    public int Iteration { get; init; }
    public string ValidatorResponse { get; init; } = "";
    public float Confidence { get; init; }
    public bool Approved { get; init; }
    public List<string> Suggestions { get; init; } = [];
}