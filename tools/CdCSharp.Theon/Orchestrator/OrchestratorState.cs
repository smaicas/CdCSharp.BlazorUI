using CdCSharp.Theon.AI;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Orchestrator.Models;

namespace CdCSharp.Theon.Orchestrator;

public sealed class OrchestratorState
{
    public List<Message> ConversationHistory { get; } = [];
    public Dictionary<string, ProposedChange> PendingChanges { get; } = [];
    public Dictionary<string, IContext> ActiveContexts { get; } = [];
    public List<ContextQueryResult> ContextQueryHistory { get; } = [];
    public int EstimatedTokens { get; private set; }

    public void AddUserMessage(string content)
    {
        ConversationHistory.Add(new Message { Role = "user", Content = content });
        EstimatedTokens += EstimateTokens(content);
    }

    public void AddAssistantMessage(Message message)
    {
        ConversationHistory.Add(message);
        EstimatedTokens += EstimateTokens(message.Content ?? string.Empty);
    }

    public void AddToolResult(string toolCallId, string result)
    {
        ConversationHistory.Add(new Message
        {
            Role = "tool",
            Content = result,
            ToolCallId = toolCallId
        });
        EstimatedTokens += EstimateTokens(result);
    }

    public void ProposeChange(ProposedChange change)
    {
        PendingChanges[change.Id] = change;
    }

    public ProposedChange? GetPendingChange(string id)
    {
        return PendingChanges.GetValueOrDefault(id);
    }

    public IEnumerable<ProposedChange> GetPendingChanges()
    {
        return PendingChanges.Values.Where(c => c.Status == ChangeStatus.Pending);
    }

    public void MarkChangeApplied(string id)
    {
        if (PendingChanges.TryGetValue(id, out ProposedChange? change))
        {
            change.Status = ChangeStatus.Applied;
        }
    }

    public void MarkChangeRejected(string id)
    {
        if (PendingChanges.TryGetValue(id, out ProposedChange? change))
        {
            change.Status = ChangeStatus.Rejected;
        }
    }

    public void RegisterContext(string name, IContext context)
    {
        ActiveContexts[name] = context;
    }

    public IContext? GetContext(string name)
    {
        return ActiveContexts.GetValueOrDefault(name);
    }

    public void AddContextQueryResult(ContextQueryResult result)
    {
        ContextQueryHistory.Add(result);
    }

    public void Clear()
    {
        ConversationHistory.Clear();
        PendingChanges.Clear();
        ContextQueryHistory.Clear();
        EstimatedTokens = 0;

        foreach (IContext context in ActiveContexts.Values)
        {
            context.Reset();
        }
    }

    private static int EstimateTokens(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return (int)(text.Length / 3.5);
    }
}