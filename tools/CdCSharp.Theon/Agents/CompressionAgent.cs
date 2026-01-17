using CdCSharp.Theon.AI;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;

namespace CdCSharp.Theon.Agents;

public class CompressionAgent
{
    private readonly LMStudioClient _aiClient;
    private readonly TheonLogger _logger;

    private const string SystemPrompt =
"""
    # Compression Specialist
    
    You are a specialized agent for summarizing conversations while preserving critical information.
    
    ## Your Task
    
    When given a conversation history, create a concise summary that captures:
    
    1. **Key Decisions**: Any decisions made during the conversation
    2. **Discoveries**: Important information learned about the codebase
    3. **Conclusions**: Final answers or solutions provided
    4. **Pending Items**: Unanswered questions or incomplete tasks
    5. **Context**: Technical details needed for continuity
    
    ## Output Format
    
    Structure your summary as:

    ## Decisions
    - [List key decisions]
    
    ## Discoveries  
    - [List important findings]
    
    ## Conclusions
    - [List final answers/solutions]
    
    ## Pending
    - [List open items, if any]
    
    ## Technical Context
    - [List relevant technical details]
    
    ## Rules
    
    1. Be concise but complete
    2. Preserve file paths and code references exactly
    3. Keep technical terminology intact
    4. Do not add information not present in the original
    5. Prioritize actionable information over discussion
""";

    public CompressionAgent(LMStudioClient aiClient, TheonLogger logger)
    {
        _aiClient = aiClient;
        _logger = logger;
    }

    public async Task<string> CompressAsync(List<ConversationMessage> messages)
    {
        if (messages.Count == 0)
            return string.Empty;

        _logger.Debug($"CompressionAgent: Compressing {messages.Count} messages");

        string conversationText = FormatConversation(messages);

        List<ConversationMessage> compressionRequest =
        [
            new() { Role = MessageRole.System, Content = SystemPrompt },
            new()
            {
                Role = MessageRole.User,
                Content =
$"""
    # Conversation to Summarize
    
    {conversationText}
    
    ---
    
    Provide a structured summary following the specified format.
"""
            }
        ];

        string summary = await _aiClient.SendAsync(compressionRequest, maxTokens: 800);

        if (string.IsNullOrWhiteSpace(summary))
        {
            _logger.Warning("CompressionAgent: Failed to generate summary, using fallback");
            return GenerateFallbackSummary(messages);
        }

        _logger.Debug($"CompressionAgent: Generated summary ({summary.Length} chars)");
        return summary;
    }

    private static string FormatConversation(List<ConversationMessage> messages)
    {
        List<string> parts = [];

        foreach (ConversationMessage msg in messages)
        {
            string role = msg.Role switch
            {
                MessageRole.User => "USER",
                MessageRole.Assistant => "ASSISTANT",
                MessageRole.AgentQuery => "AGENT_QUERY",
                MessageRole.AgentResponse => "AGENT_RESPONSE",
                _ => msg.Role.ToString().ToUpper()
            };

            string content = msg.Content.Length > 2000
                ? msg.Content[..2000] + "\n[... truncated ...]"
                : msg.Content;

            parts.Add($"**[{role}]** ({msg.Timestamp:HH:mm:ss})\n{content}");
        }

        return string.Join("\n\n---\n\n", parts);
    }

    private static string GenerateFallbackSummary(List<ConversationMessage> messages)
    {
        int userCount = messages.Count(m => m.Role == MessageRole.User);
        int assistantCount = messages.Count(m => m.Role == MessageRole.Assistant);

        return
$"""
    ## Summary (Fallback)
    
    Conversation contained {messages.Count} messages:
    - User messages: {userCount}
    - Assistant messages: {assistantCount}
    
    [Detailed summary unavailable]
""";
    }
}
