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
    private readonly CompressionAgent _compressionAgent;
    private readonly GeneratedFilesTracker _filesTracker;

    public AgentExecutor(
        LMStudioClient aiClient,
        TheonLogger logger,
        TheonOptions options,
        CompressionAgent compressionAgent,
        GeneratedFilesTracker filesTracker)
    {
        _aiClient = aiClient;
        _logger = logger;
        _options = options;
        _compressionAgent = compressionAgent;
        _filesTracker = filesTracker;
    }

    public async Task<AgentExecutionResult> ExecuteAsync(Agent agent, string instruction, string? agentsSummary = null)
    {
        agent.LastActiveAt = DateTime.UtcNow;

        // Inject generated files context into instruction
        string filesContext = _filesTracker.GetFilesContext(agent.Id);

        string fullInstruction = instruction;
        if (!string.IsNullOrEmpty(agentsSummary))
        {
            fullInstruction = $"""
            # Available Agents
            
            {agentsSummary}
            
            ---
            
            {filesContext}
            
            ---
            
            # Your Task
            
            {instruction}
            """;
        }
        else
        {
            fullInstruction = $"""
            {filesContext}
            
            ---
            
            {instruction}
            """;
        }

        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.User,
            Content = fullInstruction
        });

        _logger.LogAgentInteraction(agent.Id, InteractionDirection.Input, fullInstruction);

        await CompressIfNeededAsync(agent);

        string traceFile = _logger.TracePrompt(agent.Id, fullInstruction);

        string response = await _aiClient.SendAsync(agent.ConversationHistory.ToList());

        _logger.UpdateTrace(traceFile, response);
        _logger.LogAgentInteraction(agent.Id, InteractionDirection.Output, response);

        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.Assistant,
            Content = response
        });

        AgentExecutionResult result = ParseResponse(agent.Id, response);

        // Track generated files
        if (result.GeneratedFiles.Count > 0)
        {
            _filesTracker.RecordFiles(agent.Id, result.GeneratedFiles);
        }

        return result;
    }

    private async Task CompressIfNeededAsync(Agent agent)
    {
        ConversationOptions opts = _options.Conversation;

        int messageCount = agent.ConversationHistory.Count(m => m.Role != MessageRole.System);
        if (messageCount < opts.CompressionThreshold)
            return;

        _logger.Debug($"Compressing conversation for {agent.Name} ({messageCount} messages)");

        ConversationMessage? systemMsg = agent.ConversationHistory
            .FirstOrDefault(m => m.Role == MessageRole.System);

        List<ConversationMessage> nonSystemMessages = agent.ConversationHistory
            .Where(m => m.Role != MessageRole.System)
            .ToList();

        int messagesToCompress = nonSystemMessages.Count - opts.MessagesToKeep;
        if (messagesToCompress <= 0)
            return;

        List<ConversationMessage> toCompress = nonSystemMessages
            .Take(messagesToCompress)
            .ToList();

        List<ConversationMessage> toKeep = nonSystemMessages
            .TakeLast(opts.MessagesToKeep)
            .ToList();

        string summary = await _compressionAgent.CompressAsync(toCompress);

        agent.ConversationHistory.Clear();

        if (systemMsg != null)
            agent.ConversationHistory.Add(systemMsg);

        // Include generated files context in compression summary
        string filesContext = _filesTracker.GetFilesContext(agent.Id);

        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.User,
            Content = $"""
            # Previous Conversation Summary
            
            {summary}
            
            ---
            
            {filesContext}
            
            ---
            
            Continue from this context.
            """
        });

        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.Assistant,
            Content = "I understand the context from our previous conversation and the files I've generated. Ready to continue."
        });

        agent.ConversationHistory.AddRange(toKeep);

        _logger.Debug($"Compressed {messagesToCompress} messages, kept {opts.MessagesToKeep}. Total: {agent.ConversationHistory.Count}");
    }

    private AgentExecutionResult ParseResponse(string agentId, string response)
    {
        AgentExecutionResult result = new() { AgentId = agentId, RawResponse = response };

        if (IsCorruptedResponse(response))
        {
            _logger.Warning($"Corrupted response detected from agent {agentId}");
            result.Confidence = 0.0f;
            result.CleanContent = "[Error: Invalid response from model. Please try again.]";
            return result;
        }

        Match confidenceMatch = ConfidenceRegex().Match(response);
        if (confidenceMatch.Success && float.TryParse(confidenceMatch.Groups[1].Value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float conf))
        {
            result.Confidence = Math.Clamp(conf, 0f, 1f);
        }
        else
        {
            result.Confidence = 0.1f;
            _logger.Warning($"No confidence found in response, defaulting to 0.1");
        }

        foreach (Match match in FileRequestRegex().Matches(response))
        {
            result.FileRequests.Add(match.Groups[1].Value);
        }

        foreach (Match match in FilePathsRequestRegex().Matches(response))
        {
            result.FilePathsRequests.Add(match.Groups[1].Value);
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

        foreach (Match match in AppendToFileRegex().Matches(response))
        {
            string fileName = match.Groups[1].Value;
            string contentToAppend = match.Groups[2].Value.Trim();

            // Try to find in current result first
            GeneratedFile? existing = result.GeneratedFiles.FirstOrDefault(f => f.FileName == fileName);

            if (existing != null)
            {
                // Append to existing in current result
                result.GeneratedFiles.Remove(existing);
                result.GeneratedFiles.Add(new GeneratedFile
                {
                    FileName = fileName,
                    Language = existing.Language,
                    Content = existing.Content + "\n\n" + contentToAppend
                });
            }
            else
            {
                // Try to get from tracker
                string? trackedContent = _filesTracker.GetFileContent(agentId, fileName);

                if (trackedContent != null)
                {
                    // Append to tracked file
                    result.GeneratedFiles.Add(new GeneratedFile
                    {
                        FileName = fileName,
                        Language = Path.GetExtension(fileName).TrimStart('.'),
                        Content = trackedContent + "\n\n" + contentToAppend
                    });
                }
                else
                {
                    // Create new file with appended content
                    result.GeneratedFiles.Add(new GeneratedFile
                    {
                        FileName = fileName,
                        Language = Path.GetExtension(fileName).TrimStart('.'),
                        Content = contentToAppend
                    });
                }
            }
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

    private static bool IsCorruptedResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return true;

        string[] corruptPatterns =
        [
            "<|channel|>",
            "<|im_start|>",
            "<|im_end|>",
            "<|endoftext|>",
            "<|assistant|>",
            "<|user|>",
            "<|system|>"
        ];

        return corruptPatterns.Any(p => response.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private static string CleanResponse(string response)
    {
        string clean = response;

        clean = ConfidenceRegex().Replace(clean, "");
        clean = FileRequestRegex().Replace(clean, "");
        clean = QueryAgentRegex().Replace(clean, "");
        clean = CreateAgentRegex().Replace(clean, "");
        clean = ValidationRegex().Replace(clean, "");
        clean = FilePathsRequestRegex().Replace(clean, "");

        clean = Regex.Replace(clean, @"\[GENERATE_FILE:.*?\]", "", RegexOptions.IgnoreCase);
        clean = Regex.Replace(clean, @"\[/GENERATE_FILE\]", "", RegexOptions.IgnoreCase);
        clean = Regex.Replace(clean, @"\[APPEND_TO_FILE:.*?\]", "", RegexOptions.IgnoreCase);
        clean = Regex.Replace(clean, @"\[/APPEND_TO_FILE\]", "", RegexOptions.IgnoreCase);

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

    [GeneratedRegex(@"\[GENERATE_FILE:\s*name=""([^""]+)""\s+language=""([^""]+)""\]\s*([\s\S]*?)\[/GENERATE_FILE\]", RegexOptions.IgnoreCase)]
    private static partial Regex GeneratedFileRegex();

    [GeneratedRegex(@"\[APPEND_TO_FILE:\s*name=""([^""]+)""\]\s*([\s\S]*?)\[/APPEND_TO_FILE\]", RegexOptions.IgnoreCase)]
    private static partial Regex AppendToFileRegex();

    [GeneratedRegex(@"\[REQUEST_FILE_PATHS:\s*assembly=""([^""]*)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex FilePathsRequestRegex();
}

public class AgentExecutionResult
{
    public string AgentId { get; set; } = "";
    public string RawResponse { get; set; } = "";
    public string CleanContent { get; set; } = "";
    public float Confidence { get; set; } = 0.0f;
    public List<string> FileRequests { get; } = [];
    public List<string> FilePathsRequests { get; } = [];
    public List<AgentRequest> AgentQueries { get; } = [];
    public List<AgentCreationSpec> AgentCreationRequests { get; } = [];
    public List<GeneratedFile> GeneratedFiles { get; } = [];
    public List<string> SuggestedValidators { get; set; } = [];

    public bool HasPendingRequests =>
        FileRequests.Count > 0 ||
        FilePathsRequests.Count > 0 ||
        AgentQueries.Count > 0 ||
        AgentCreationRequests.Count > 0;
}