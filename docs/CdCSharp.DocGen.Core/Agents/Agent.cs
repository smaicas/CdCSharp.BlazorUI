using CdCSharp.DocGen.Core.Abstractions.Agents;
using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Models.Agents;
using CdCSharp.DocGen.Core.Models.Options;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CdCSharp.DocGen.Core.Agents;

public partial class Agent : IAgent
{
    private readonly List<AgentMessage> _anchorMessages = [];
    private readonly List<AgentMessage> _conversationMessages = [];
    private string? _compressionSummary;

    private readonly IAiClient _aiClient;
    private readonly Func<AgentQuery, Task<AgentQueryResult>> _queryHandler;
    private readonly ILogger _logger;
    private readonly ConversationOptions _options;

    public string Id => Definition.Id;
    public string Name => Definition.Name;
    public AgentDefinition Definition { get; }
    public IReadOnlyList<AgentMessage> ConversationHistory => BuildMessageList();

    public Agent(
        AgentDefinition definition,
        IAiClient aiClient,
        Func<AgentQuery, Task<AgentQueryResult>> queryHandler,
        ILogger logger,
        ConversationOptions options)
    {
        Definition = definition;
        _aiClient = aiClient;
        _queryHandler = queryHandler;
        _logger = logger;
        _options = options;

        _anchorMessages.Add(new AgentMessage
        {
            Role = AgentMessageRole.System,
            Content = definition.SystemPrompt
        });
    }

    public void LoadExpertiseContext(string context)
    {
        _anchorMessages.Add(new AgentMessage
        {
            Role = AgentMessageRole.User,
            Content = $"Here is your expertise context:\n\n{context}"
        });

        _anchorMessages.Add(new AgentMessage
        {
            Role = AgentMessageRole.Assistant,
            Content = "I've reviewed my expertise context. Ready to help."
        });

        _logger.LogDebug("Agent {Id}: expertise context loaded ({Length} chars)", Id, context.Length);
    }

    public async Task<string> ExecuteAsync(string instruction, int maxTokens = 2000)
    {
        await CompressIfNeededAsync();

        _conversationMessages.Add(new AgentMessage
        {
            Role = AgentMessageRole.User,
            Content = instruction
        });

        string response = await SendToModelAsync(maxTokens);

        response = await ProcessAgentQueriesAsync(response, maxTokens);

        if (!string.IsNullOrWhiteSpace(response))
        {
            _conversationMessages.Add(new AgentMessage
            {
                Role = AgentMessageRole.Assistant,
                Content = response
            });
        }

        return response;
    }

    public async Task<AgentQueryResult> QueryAsync(AgentQuery query)
    {
        string instruction = $"""
            Another agent is asking you a question:
            
            Question: {query.Question}
            {(query.Context != null ? $"\nContext: {query.Context}" : "")}
            
            Please provide a focused, concise answer.
            """;

        string response = await ExecuteAsync(instruction, maxTokens: 1000);

        return new AgentQueryResult
        {
            Success = !string.IsNullOrWhiteSpace(response),
            RespondingAgentId = Id,
            Response = response
        };
    }

    public void ClearConversation()
    {
        _conversationMessages.Clear();
        _compressionSummary = null;
        _logger.LogDebug("Agent {Id}: conversation cleared", Id);
    }

    private async Task<string> SendToModelAsync(int maxTokens)
    {
        List<AgentMessage> messages = BuildMessageList();
        List<ChatMessage> chatMessages = messages.Select(m => new ChatMessage(
            m.Role switch
            {
                AgentMessageRole.System => "system",
                AgentMessageRole.Assistant or AgentMessageRole.AgentResponse => "assistant",
                _ => "user"
            },
            m.Content
        )).ToList();

        _logger.LogDebug("Agent {Id}: sending {Count} messages", Id, chatMessages.Count);

        return await _aiClient.SendMessagesAsync(chatMessages, maxTokens);
    }

    private async Task<string> ProcessAgentQueriesAsync(string response, int maxTokens)
    {
        MatchCollection matches = AgentQueryRegex().Matches(response);

        if (matches.Count == 0)
            return response;

        _logger.LogDebug("Agent {Id}: processing {Count} agent queries", Id, matches.Count);

        foreach (Match match in matches)
        {
            string expertise = match.Groups["expertise"].Value;
            string question = match.Groups["question"].Value;

            AgentQuery query = new()
            {
                FromAgentId = Id,
                TargetExpertise = expertise,
                Question = question
            };

            _logger.LogDebug("Agent {Id}: querying for expertise '{Expertise}'", Id, expertise);

            AgentQueryResult result = await _queryHandler(query);

            if (result.Success)
            {
                _conversationMessages.Add(new AgentMessage
                {
                    Role = AgentMessageRole.AgentQuery,
                    Content = $"Query to {result.RespondingAgentId}: {question}",
                    FromAgentId = Id,
                    ToAgentId = result.RespondingAgentId
                });

                _conversationMessages.Add(new AgentMessage
                {
                    Role = AgentMessageRole.AgentResponse,
                    Content = result.Response,
                    FromAgentId = result.RespondingAgentId,
                    ToAgentId = Id
                });

                string continuation = $"""
                    I received this response from the {result.RespondingAgentId}:
                    
                    {result.Response}
                    
                    Please continue your response incorporating this information.
                    """;

                _conversationMessages.Add(new AgentMessage
                {
                    Role = AgentMessageRole.User,
                    Content = continuation
                });

                response = await SendToModelAsync(maxTokens);
            }
        }

        return AgentQueryRegex().Replace(response, "").Trim();
    }

    private List<AgentMessage> BuildMessageList()
    {
        List<AgentMessage> messages = [.. _anchorMessages];

        if (!string.IsNullOrEmpty(_compressionSummary))
        {
            messages.Add(new AgentMessage
            {
                Role = AgentMessageRole.User,
                Content = $"Summary of our previous work:\n{_compressionSummary}"
            });
            messages.Add(new AgentMessage
            {
                Role = AgentMessageRole.Assistant,
                Content = "Understood. I'll continue from where we left off."
            });
        }

        int windowStart = Math.Max(0, _conversationMessages.Count - _options.SlidingWindowSize);
        messages.AddRange(_conversationMessages.Skip(windowStart));

        return messages;
    }

    private async Task CompressIfNeededAsync()
    {
        int messagesToCompress = _conversationMessages.Count - _options.SlidingWindowSize;

        if (messagesToCompress < _options.CompressionThreshold)
            return;

        _logger.LogDebug("Agent {Id}: compressing {Count} messages", Id, messagesToCompress);

        List<AgentMessage> oldMessages = _conversationMessages.Take(messagesToCompress).ToList();

        string compressionPrompt = BuildCompressionPrompt(oldMessages);
        string newSummary = await _aiClient.SendAsync(compressionPrompt, maxTokens: 500);

        if (!string.IsNullOrWhiteSpace(newSummary))
        {
            _compressionSummary = string.IsNullOrEmpty(_compressionSummary)
                ? newSummary
                : $"{_compressionSummary}\n\n{newSummary}";

            _conversationMessages.RemoveRange(0, messagesToCompress);

            _logger.LogDebug("Agent {Id}: compressed to summary ({Length} chars)",
                Id, _compressionSummary.Length);
        }
    }

    private static string BuildCompressionPrompt(List<AgentMessage> messages)
    {
        string conversation = string.Join("\n\n", messages.Select(m =>
            $"[{m.Role}]: {m.Content}"));

        return $"""
            Summarize the key points from this conversation in 3-5 concise bullets.
            Focus on: what was documented, decisions made, important findings.
            
            Conversation:
            {conversation}
            
            Summary (bullets only):
            """;
    }

    [GeneratedRegex(@"\[QUERY_AGENT:\s*expertise=""(?<expertise>[^""]+)""\s+question=""(?<question>[^""]+)""\]", RegexOptions.Compiled)]
    private static partial Regex AgentQueryRegex();
}