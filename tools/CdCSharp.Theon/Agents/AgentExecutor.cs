using CdCSharp.Theon.AI;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using System.Text.RegularExpressions;

namespace CdCSharp.Theon.Agents;

public partial class AgentExecutor
{
    private readonly LMStudioClient _aiClient;
    private readonly TheonLogger _logger;
    private readonly TheonOptions _options;

    public AgentExecutor(LMStudioClient aiClient, TheonLogger logger, TheonOptions options)
    {
        _aiClient = aiClient;
        _logger = logger;
        _options = options;
    }

    public async Task<AgentExecutionResult> ExecuteAsync(Agent agent, string instruction)
    {
        agent.LastActiveAt = DateTime.UtcNow;

        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.User,
            Content = instruction
        });

        _logger.LogAgentInteraction(agent.Id, InteractionDirection.Input, instruction);

        await CompressIfNeededAsync(agent);

        string traceFile = _logger.TracePrompt(agent.Id, instruction);

        string response = await _aiClient.SendAsync(agent.ConversationHistory.ToList());

        _logger.UpdateTrace(traceFile, response);
        _logger.LogAgentInteraction(agent.Id, InteractionDirection.Output, response);

        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.Assistant,
            Content = response
        });

        return ParseResponse(agent.Id, response);
    }

    private async Task CompressIfNeededAsync(Agent agent)
    {
        ConversationOptions opts = _options.Conversation;

        int messageCount = agent.ConversationHistory.Count(m => m.Role != MessageRole.System);
        if (messageCount < opts.CompressionThreshold)
            return;

        _logger.Debug($"Compressing conversation for {agent.Name} ({messageCount} messages)");

        ConversationMessage? systemMsg = agent.ConversationHistory.FirstOrDefault(m => m.Role == MessageRole.System);

        List<ConversationMessage> toCompress = agent.ConversationHistory
            .Where(m => m.Role != MessageRole.System)
            .Take(opts.MessagesToCompress)
            .ToList();

        List<ConversationMessage> toKeep = agent.ConversationHistory
            .Where(m => m.Role != MessageRole.System)
            .Skip(opts.MessagesToCompress)
            .ToList();

        string summaryPrompt = $"""
            # Compression Task
            
            Summarize the following conversation, preserving:
            - Key decisions made
            - Important information discovered
            - Conclusions reached
            - Any pending questions or tasks
            
            Be concise but don't lose critical details.
            
            ## Conversation to Summarize
            
            {string.Join("\n\n", toCompress.Select(m => $"**[{m.Role}]:**\n{m.Content}"))}
            
            ## Summary
            """;

        List<ConversationMessage> summaryMessages =
        [
            new() { Role = MessageRole.User, Content = summaryPrompt }
        ];

        string summary = await _aiClient.SendAsync(summaryMessages, maxTokens: 500);

        agent.ConversationHistory.Clear();

        if (systemMsg != null)
            agent.ConversationHistory.Add(systemMsg);

        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.User,
            Content = $"""
                # Previous Conversation Summary
                
                The following is a summary of our earlier conversation:
                
                {summary}
                
                Continue from this context.
                """
        });

        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.Assistant,
            Content = "I understand. I have the context from our previous conversation and I'm ready to continue."
        });

        agent.ConversationHistory.AddRange(toKeep);

        _logger.Debug($"Compressed to {agent.ConversationHistory.Count} messages");
    }

    private AgentExecutionResult ParseResponse(string agentId, string response)
    {
        AgentExecutionResult result = new() { AgentId = agentId, RawResponse = response };

        Match confidenceMatch = ConfidenceRegex().Match(response);
        if (confidenceMatch.Success && float.TryParse(confidenceMatch.Groups[1].Value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float conf))
        {
            result.Confidence = Math.Clamp(conf, 0f, 1f);
        }

        foreach (Match match in FileRequestRegex().Matches(response))
        {
            result.FileRequests.Add(match.Groups[1].Value);
        }

        foreach (Match match in QueryAgentRegex().Matches(response))
        {
            result.AgentQueries.Add(new AgentRequest
            {
                Type = AgentRequestType.QueryAgent,
                FromAgentId = agentId,
                TargetExpertise = match.Groups[1].Value,
                Payload = match.Groups[2].Value,
                Reason = "Cross-agent query"
            });
        }

        foreach (Match match in CreateAgentRegex().Matches(response))
        {
            string filesStr = match.Groups.Count > 4 ? match.Groups[4].Value : "";
            result.AgentCreationRequests.Add(new AgentCreationSpec
            {
                Name = match.Groups[1].Value,
                Expertise = match.Groups[2].Value,
                InitialContextFiles = string.IsNullOrEmpty(filesStr)
                    ? []
                    : filesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                ParentAgentId = agentId
            });
        }

        foreach (Match match in GeneratedFileRegex().Matches(response))
        {
            result.GeneratedFiles.Add(new GeneratedFile
            {
                FileName = match.Groups[1].Value,
                Language = match.Groups[2].Value,
                Content = match.Groups[3].Value.Trim()
            });
        }

        Match validationMatch = ValidationRegex().Match(response);
        if (validationMatch.Success)
        {
            result.SuggestedValidators = validationMatch.Groups[1].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        result.CleanContent = CleanResponse(response);

        return result;
    }

    private static string CleanResponse(string response)
    {
        string clean = response;

        clean = ConfidenceRegex().Replace(clean, "");
        clean = FileRequestRegex().Replace(clean, "");
        clean = QueryAgentRegex().Replace(clean, "");
        clean = CreateAgentRegex().Replace(clean, "");
        clean = ValidationRegex().Replace(clean, "");

        clean = Regex.Replace(clean, @"\[GENERATE_FILE:.*?\]", "", RegexOptions.IgnoreCase);
        clean = Regex.Replace(clean, @"\[/GENERATE_FILE\]", "", RegexOptions.IgnoreCase);

        while (clean.Contains("\n\n\n"))
            clean = clean.Replace("\n\n\n", "\n\n");

        return clean.Trim();
    }

    [GeneratedRegex(@"\[CONFIDENCE:\s*([\d.]+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex ConfidenceRegex();

    [GeneratedRegex(@"\[REQUEST_FILE:\s*path=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex FileRequestRegex();

    [GeneratedRegex(@"\[QUERY_AGENT:\s*expertise=""([^""]+)""\s+question=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex QueryAgentRegex();

    [GeneratedRegex(@"\[CREATE_AGENT:\s*name=""([^""]+)""\s+expertise=""([^""]+)""\s+files=""([^""]*)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex CreateAgentRegex();

    [GeneratedRegex(@"\[SUGGEST_VALIDATION:\s*expertise=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex ValidationRegex();

    [GeneratedRegex(@"\[FILE:\s*name=""([^""]+)""\s+language=""([^""]+)""\]\s*```\w*\s*([\s\S]*?)```\s*\[/FILE\]", RegexOptions.IgnoreCase)]
    private static partial Regex GeneratedFileRegex();
}

public class AgentExecutionResult
{
    public string AgentId { get; set; } = "";
    public string RawResponse { get; set; } = "";
    public string CleanContent { get; set; } = "";
    public float Confidence { get; set; } = 1.0f;
    public List<string> FileRequests { get; } = [];
    public List<AgentRequest> AgentQueries { get; } = [];
    public List<AgentCreationSpec> AgentCreationRequests { get; } = [];
    public List<GeneratedFile> GeneratedFiles { get; } = [];
    public List<string> SuggestedValidators { get; set; } = [];

    public bool HasPendingRequests =>
        FileRequests.Count > 0 ||
        AgentQueries.Count > 0 ||
        AgentCreationRequests.Count > 0;
}