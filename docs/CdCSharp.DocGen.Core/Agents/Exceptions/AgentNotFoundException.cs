namespace CdCSharp.DocGen.Core.Agents.Exceptions;

public class AgentNotFoundException : InvalidOperationException
{
    public string AgentId { get; }

    public AgentNotFoundException(string agentId)
        : base($"Agent not found: {agentId}")
    {
        AgentId = agentId;
    }

    public AgentNotFoundException(string agentId, string message)
        : base(message)
    {
        AgentId = agentId;
    }
}