// Agents/Agent.cs
using CdCSharp.DocGen.Core.Abstractions.Agents;
using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Models.Agents;
using CdCSharp.DocGen.Core.Models.AI;
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
    private readonly IConversationCompressor _compressor;
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
        IConversationCompressor compressor,
        Func<AgentQuery, Task<AgentQueryResult>> queryHandler,
        ILogger logger,
        ConversationOptions options)
    {
        Definition = definition;
        _aiClient = aiClient;
        _compressor = compressor;
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
        return await ExecuteAsync(instruction, maxTokens, requiresMemory: true);
    }

    public async Task<string> ExecuteAsync(string instruction, int maxTokens, bool requiresMemory)
    {
        if (!requiresMemory)
        {
            return await ExecuteStatelessAsync(instruction, maxTokens);
        }

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

    private async Task<string> ExecuteStatelessAsync(string instruction, int maxTokens)
    {
        _logger.LogDebug("Agent {Id}: executing stateless instruction", Id);

        List<ChatMessage> messages =
        [
            .. _anchorMessages.Select(ToChat),
            new ChatMessage("user", instruction),
        ];

        string response = await _aiClient.SendMessagesAsync(messages, maxTokens);

        response = await ProcessAgentQueriesStatelessAsync(response, instruction, maxTokens);

        return response;
    }

    private async Task<string> ProcessAgentQueriesStatelessAsync(string response, string originalInstruction, int maxTokens)
    {
        MatchCollection matches = AgentQueryRegex().Matches(response);

        if (matches.Count == 0)
            return response;

        _logger.LogDebug("Agent {Id}: processing {Count} agent queries (stateless)", Id, matches.Count);

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

            AgentQueryResult result = await _queryHandler(query);

            if (result.Success)
            {
                string continuation = $"""
                    Original instruction: {originalInstruction}
                    
                    I received this response from the {result.RespondingAgentId}:
                    
                    {result.Response}
                    
                    Please complete your response incorporating this information.
                    """;

                List<ChatMessage> messages =
                [
                    .. _anchorMessages.Select(ToChat),
                    new ChatMessage("user", continuation),
                ];

                response = await _aiClient.SendMessagesAsync(messages, maxTokens);
            }
        }

        return AgentQueryRegex().Replace(response, "").Trim();
    }

    public async Task<AgentQueryResult> QueryAsync(AgentQuery query)
    {
        string instruction = $"""
            Another agent is asking you a question:
            
            Question: {query.Question}
            {(query.Context != null ? $"\nContext: {query.Context}" : "")}
            
            Please provide a focused, concise answer.
            """;

        string response = await ExecuteAsync(instruction, maxTokens: 1000, requiresMemory: false);

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
        List<ChatMessage> chatMessages = messages.Select(ToChat).ToList();

        _logger.LogDebug("Agent {Id}: sending {Count} messages", Id, chatMessages.Count);

        return await _aiClient.SendMessagesAsync(chatMessages, maxTokens);
    }

    private static ChatMessage ToChat(AgentMessage m) => new(
        m.Role switch
        {
            AgentMessageRole.System => "system",
            AgentMessageRole.Assistant or AgentMessageRole.AgentResponse => "assistant",
            _ => "user"
        },
        m.Content
    );

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
        List<ChatMessage> anchorChat = _anchorMessages.Select(ToChat).ToList();
        List<ChatMessage> historyChat = _conversationMessages.Select(ToChat).ToList();

        ConversationWindow window = _compressor.BuildWindow(
            anchorChat,
            historyChat,
            _compressionSummary,
            _options.SlidingWindowSize,
            _options.CompressionThreshold);

        List<AgentMessage> result = [.. _anchorMessages];

        if (!string.IsNullOrEmpty(window.UpdatedSummary) && window.UpdatedSummary != _compressionSummary)
        {
            result.Add(new AgentMessage
            {
                Role = AgentMessageRole.User,
                Content = $"Summary of our previous work:\n{window.UpdatedSummary}"
            });
            result.Add(new AgentMessage
            {
                Role = AgentMessageRole.Assistant,
                Content = "Understood. I'll continue from where we left off."
            });
        }

        int windowStart = Math.Max(0, _conversationMessages.Count - _options.SlidingWindowSize);
        result.AddRange(_conversationMessages.Skip(windowStart));

        return result;
    }

    private async Task CompressIfNeededAsync()
    {
        List<ChatMessage> anchorChat = _anchorMessages.Select(ToChat).ToList();
        List<ChatMessage> historyChat = _conversationMessages.Select(ToChat).ToList();

        ConversationWindow window = _compressor.BuildWindow(
            anchorChat,
            historyChat,
            _compressionSummary,
            _options.SlidingWindowSize,
            _options.CompressionThreshold);

        if (!window.RequiresCompression)
            return;

        _logger.LogDebug("Agent {Id}: compressing {Count} messages", Id, window.MessagesToRemove);

        List<ChatMessage> messagesToCompress = historyChat.Take(window.MessagesToRemove).ToList();
        string newSummary = await _compressor.CompressMessagesAsync(messagesToCompress);

        if (!string.IsNullOrWhiteSpace(newSummary))
        {
            _compressionSummary = string.IsNullOrEmpty(_compressionSummary)
                ? newSummary
                : $"{_compressionSummary}\n\n{newSummary}";

            _conversationMessages.RemoveRange(0, window.MessagesToRemove);

            _logger.LogDebug("Agent {Id}: compressed to summary ({Length} chars)",
                Id, _compressionSummary.Length);
        }
    }

    [GeneratedRegex(@"\[QUERY_AGENT:\s*expertise=""(?<expertise>[^""]+)""\s+question=""(?<question>[^""]+)""\]", RegexOptions.Compiled)]
    private static partial Regex AgentQueryRegex();
}