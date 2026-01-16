using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using System.Collections.Concurrent;

namespace CdCSharp.Theon.Agents;

public class AgentRegistry
{
    private readonly ConcurrentDictionary<string, Agent> _agents = new();
    private readonly TheonLogger _logger;

    public AgentRegistry(TheonLogger logger)
    {
        _logger = logger;
    }

    public IReadOnlyCollection<Agent> AllAgents => _agents.Values.ToList();
    public IReadOnlyCollection<Agent> ActiveAgents => _agents.Values.Where(a => a.State == AgentState.Active).ToList();
    public IReadOnlyCollection<Agent> SleepingAgents => _agents.Values.Where(a => a.State == AgentState.Sleeping).ToList();

    public void Register(Agent agent)
    {
        _agents[agent.Id] = agent;
        _logger.Info($"Registered agent: {agent.Name} ({agent.Id})");
    }

    public Agent? Get(string id)
    {
        return _agents.TryGetValue(id, out Agent? agent) ? agent : null;
    }

    public Agent? FindByExpertise(string expertise)
    {
        string lower = expertise.ToLowerInvariant();

        return _agents.Values.FirstOrDefault(a =>
            a.Expertise.ToLowerInvariant().Contains(lower) ||
            lower.Contains(a.Expertise.ToLowerInvariant()));
    }

    public List<Agent> FindAllByExpertise(string expertise)
    {
        string lower = expertise.ToLowerInvariant();

        return _agents.Values
            .Where(a => a.Expertise.ToLowerInvariant().Contains(lower) ||
                       lower.Contains(a.Expertise.ToLowerInvariant()))
            .ToList();
    }

    public void Remove(string id)
    {
        if (_agents.TryRemove(id, out Agent? agent))
        {
            _logger.Debug($"Removed agent: {agent.Name} ({id})");
        }
    }

    public string GetAgentsSummary()
    {
        if (_agents.IsEmpty)
            return "No agents registered";

        List<string> lines = ["Current agents:"];

        foreach (Agent agent in _agents.Values.OrderBy(a => a.Name))
        {
            string status = agent.State == AgentState.Active ? "●" : "○";
            lines.Add($"  {status} {agent.Name} ({agent.Id}): {agent.Expertise}");
        }

        return string.Join("\n", lines);
    }
}