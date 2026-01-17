using System.Collections.Concurrent;

namespace CdCSharp.Theon.Infrastructure;

public class MetricsCollector
{
    private readonly ConcurrentDictionary<string, AgentMetrics> _agentMetrics = new();
    private readonly ConcurrentBag<QueryMetrics> _queryHistory = [];
    private readonly ConcurrentDictionary<string, ValidationMetrics> _validationMetrics = new();
    private readonly TheonLogger _logger;
    private readonly object _lock = new();

    public MetricsCollector(TheonLogger logger)
    {
        _logger = logger;
    }

    public void RecordTokenUsage(string agentId, int inputTokens, int outputTokens)
    {
        AgentMetrics metrics = _agentMetrics.GetOrAdd(agentId, _ => new AgentMetrics { AgentId = agentId });

        lock (_lock)
        {
            metrics.TotalInputTokens += inputTokens;
            metrics.TotalOutputTokens += outputTokens;
            metrics.RequestCount++;
        }
    }

    public void RecordQueryTime(string queryType, string agentId, TimeSpan duration, bool success)
    {
        QueryMetrics query = new()
        {
            QueryType = queryType,
            AgentId = agentId,
            Duration = duration,
            Success = success,
            Timestamp = DateTime.UtcNow
        };

        _queryHistory.Add(query);

        AgentMetrics agentMetrics = _agentMetrics.GetOrAdd(agentId, _ => new AgentMetrics { AgentId = agentId });

        lock (_lock)
        {
            agentMetrics.TotalResponseTime += duration;
            if (success) agentMetrics.SuccessfulRequests++;
        }
    }

    public void RecordValidation(string agentId, bool approved, int iteration)
    {
        ValidationMetrics metrics = _validationMetrics.GetOrAdd(agentId, _ => new ValidationMetrics { AgentId = agentId });

        lock (_lock)
        {
            metrics.TotalValidations++;
            if (approved) metrics.ApprovedCount++;
            metrics.TotalIterations += iteration;
        }
    }

    public MetricsSummary GetSummary()
    {
        Dictionary<string, AgentMetrics> agentSummaries = _agentMetrics.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value
        );

        List<QueryMetrics> recentQueries = _queryHistory
            .OrderByDescending(q => q.Timestamp)
            .Take(100)
            .ToList();

        double overallSuccessRate = _queryHistory.Count > 0
            ? (double)_queryHistory.Count(q => q.Success) / _queryHistory.Count
            : 0;

        Dictionary<string, double> avgResponseTimeByType = _queryHistory
            .GroupBy(q => q.QueryType)
            .ToDictionary(
                g => g.Key,
                g => g.Average(q => q.Duration.TotalMilliseconds)
            );

        double validationApprovalRate = _validationMetrics.Values.Sum(v => v.TotalValidations) > 0
            ? (double)_validationMetrics.Values.Sum(v => v.ApprovedCount) / _validationMetrics.Values.Sum(v => v.TotalValidations)
            : 0;

        return new MetricsSummary
        {
            AgentMetrics = agentSummaries,
            RecentQueries = recentQueries,
            OverallSuccessRate = overallSuccessRate,
            AverageResponseTimeByType = avgResponseTimeByType,
            ValidationApprovalRate = validationApprovalRate,
            TotalTokensUsed = agentSummaries.Values.Sum(a => a.TotalInputTokens + a.TotalOutputTokens),
            TotalQueries = _queryHistory.Count
        };
    }

    public string GenerateReport()
    {
        MetricsSummary summary = GetSummary();

        List<string> lines =
        [
            "# THEON Metrics Report",
            "",
            $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            "",
            "## Overview",
            "",
            $"- Total Queries: {summary.TotalQueries}",
            $"- Total Tokens Used: {summary.TotalTokensUsed:N0}",
            $"- Overall Success Rate: {summary.OverallSuccessRate:P1}",
            $"- Validation Approval Rate: {summary.ValidationApprovalRate:P1}",
            "",
            "## Agent Performance",
            "",
            "| Agent | Requests | Success | Tokens (In/Out) | Avg Response |",
            "|-------|----------|---------|-----------------|--------------|"
        ];

        foreach ((string agentId, AgentMetrics metrics) in summary.AgentMetrics.OrderByDescending(m => m.Value.RequestCount))
        {
            double avgResponse = metrics.RequestCount > 0
                ? metrics.TotalResponseTime.TotalMilliseconds / metrics.RequestCount
                : 0;

            double successRate = metrics.RequestCount > 0
                ? (double)metrics.SuccessfulRequests / metrics.RequestCount
                : 0;

            lines.Add($"| {agentId} | {metrics.RequestCount} | {successRate:P0} | {metrics.TotalInputTokens:N0}/{metrics.TotalOutputTokens:N0} | {avgResponse:F0}ms |");
        }

        lines.Add("");
        lines.Add("## Response Time by Query Type");
        lines.Add("");

        foreach ((string queryType, double avgMs) in summary.AverageResponseTimeByType.OrderByDescending(x => x.Value))
        {
            lines.Add($"- **{queryType}**: {avgMs:F0}ms average");
        }

        return string.Join("\n", lines);
    }
}

public class AgentMetrics
{
    public string AgentId { get; init; } = "";
    public int TotalInputTokens { get; set; }
    public int TotalOutputTokens { get; set; }
    public int RequestCount { get; set; }
    public int SuccessfulRequests { get; set; }
    public TimeSpan TotalResponseTime { get; set; }
}

public class QueryMetrics
{
    public string QueryType { get; init; } = "";
    public string AgentId { get; init; } = "";
    public TimeSpan Duration { get; init; }
    public bool Success { get; init; }
    public DateTime Timestamp { get; init; }
}

public class ValidationMetrics
{
    public string AgentId { get; init; } = "";
    public int TotalValidations { get; set; }
    public int ApprovedCount { get; set; }
    public int TotalIterations { get; set; }
}

public class MetricsSummary
{
    public Dictionary<string, AgentMetrics> AgentMetrics { get; init; } = [];
    public List<QueryMetrics> RecentQueries { get; init; } = [];
    public double OverallSuccessRate { get; init; }
    public Dictionary<string, double> AverageResponseTimeByType { get; init; } = [];
    public double ValidationApprovalRate { get; init; }
    public long TotalTokensUsed { get; init; }
    public int TotalQueries { get; init; }
}