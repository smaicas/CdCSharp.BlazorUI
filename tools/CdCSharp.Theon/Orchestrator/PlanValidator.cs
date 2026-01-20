using CdCSharp.Theon.Context.Planning;
using CdCSharp.Theon.Core;

namespace CdCSharp.Theon.Orchestrator;

/// <summary>
/// Validates that LLM queries follow the execution plan strictly.
/// </summary>
public sealed class PlanValidator
{
    public Result<PlanStep> ValidateQueryAgainstPlan(
        ExecutionPlan plan,
        string requestedContext,
        List<string>? requestedFiles)
    {
        PlanStep? nextStep = GetNextPendingStep(plan);

        if (nextStep == null)
        {
            return Result<PlanStep>.Failure(
                Error.Custom(
                    "PLAN_COMPLETED",
                    "All plan steps completed. Use generate_output_file to create final output."));
        }

        // Validate context matches
        if (!nextStep.TargetContext.Equals(requestedContext, StringComparison.OrdinalIgnoreCase))
        {
            return Result<PlanStep>.Failure(
                Error.Custom(
                    "PLAN_ORDER_VIOLATION",
                    $"Plan requires querying '{nextStep.TargetContext}' next (step {nextStep.Order}). " +
                    $"You attempted to query '{requestedContext}'. Follow the plan order."));
        }

        // Validate files if specified in plan
        if (nextStep.SuggestedFiles.Count > 0)
        {
            if (requestedFiles == null || requestedFiles.Count == 0)
            {
                return Result<PlanStep>.Failure(
                    Error.Custom(
                        "MISSING_FILES",
                        $"Step {nextStep.Order} requires examining files: {string.Join(", ", nextStep.SuggestedFiles)}. " +
                        "Provide them in the 'files' parameter."));
            }

            // Check if all suggested files are included
            List<string> missingSuggested = nextStep.SuggestedFiles
                .Where(sf => !requestedFiles.Any(rf =>
                    rf.Equals(sf, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (missingSuggested.Count > 0)
            {
                return Result<PlanStep>.Failure(
                    Error.Custom(
                        "INCOMPLETE_FILES",
                        $"Step {nextStep.Order} suggests examining these files, but they are missing: " +
                        $"{string.Join(", ", missingSuggested)}. " +
                        "Include all suggested files for comprehensive analysis."));
            }
        }

        return Result<PlanStep>.Success(nextStep);
    }

    public bool CanGenerateOutput(ExecutionPlan plan)
    {
        return plan.Steps.All(s => s.Status == PlanStepStatus.Completed);
    }

    public string GetPlanProgress(ExecutionPlan plan)
    {
        int completed = plan.Steps.Count(s => s.Status == PlanStepStatus.Completed);
        int total = plan.Steps.Count;
        return $"{completed}/{total} steps completed";
    }

    private PlanStep? GetNextPendingStep(ExecutionPlan plan)
    {
        return plan.Steps
            .Where(s => s.Status == PlanStepStatus.Pending)
            .OrderBy(s => s.Order)
            .FirstOrDefault();
    }
}