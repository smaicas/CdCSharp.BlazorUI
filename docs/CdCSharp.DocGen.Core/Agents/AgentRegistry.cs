using CdCSharp.DocGen.Core.Abstractions.Agents;
using CdCSharp.DocGen.Core.Models.Agents;
using CdCSharp.DocGen.Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CdCSharp.DocGen.Core.Agents;

public class AgentRegistry : IAgentRegistry
{
    private readonly Dictionary<string, AgentDefinition> _agents;
    private readonly string _registryPath;
    private readonly ILogger<AgentRegistry> _logger;
    private readonly DocGenOptions _docGenOptions;

    public AgentRegistry(IOptions<DocGenOptions> options, ILogger<AgentRegistry> logger)
    {
        _logger = logger;
        _registryPath = Path.Combine(options.Value.ProjectPath, ".doccache", "agents.json");
        _docGenOptions = options.Value;
        _agents = LoadAgents();
    }

    private Dictionary<string, AgentDefinition> LoadAgents()
    {
        Dictionary<string, AgentDefinition> result = BuiltInAgents.All.ToDictionary(a => a.Id);

        if (_docGenOptions.Cache.Enabled && File.Exists(_registryPath))
        {
            try
            {
                string json = File.ReadAllText(_registryPath);
                List<AgentDefinition>? custom = JsonSerializer.Deserialize<List<AgentDefinition>>(json);

                if (custom != null)
                {
                    foreach (AgentDefinition agent in custom)
                    {
                        result[agent.Id] = agent;
                    }
                    _logger.LogDebug("Loaded {Count} custom agents", custom.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load custom agents");
            }
        }

        return result;
    }

    public IReadOnlyList<AgentDefinition> GetAll() => _agents.Values.ToList();

    public AgentDefinition? Get(string id) =>
        _agents.TryGetValue(id, out AgentDefinition? agent) ? agent : null;

    public AgentDefinition? FindByExpertise(string expertise)
    {
        string lowerExpertise = expertise.ToLowerInvariant();

        return _agents.Values.FirstOrDefault(a =>
            a.Expertise.Topics.Any(t => t.Contains(lowerExpertise, StringComparison.OrdinalIgnoreCase)) ||
            a.Capabilities.Any(c => c.Contains(lowerExpertise, StringComparison.OrdinalIgnoreCase)) ||
            a.Description.Contains(lowerExpertise, StringComparison.OrdinalIgnoreCase));
    }

    public void Register(AgentDefinition definition)
    {
        if (_agents.TryGetValue(definition.Id, out AgentDefinition? existing) && existing.IsBuiltIn)
        {
            _logger.LogWarning("Cannot override built-in agent: {AgentId}", definition.Id);
            return;
        }

        _agents[definition.Id] = definition with { IsBuiltIn = false, CreatedAt = DateTime.UtcNow };
        SaveCustomAgents();
        _logger.LogInformation("Registered agent: {Name} ({Id})", definition.Name, definition.Id);
    }

    public void Remove(string id)
    {
        if (_agents.TryGetValue(id, out AgentDefinition? existing) && existing.IsBuiltIn)
        {
            _logger.LogWarning("Cannot remove built-in agent: {AgentId}", id);
            return;
        }

        _agents.Remove(id);
        SaveCustomAgents();
    }

    public string GetAgentListForPrompt()
    {
        return string.Join("\n", _agents.Values.Select(a =>
            $"- {a.Id}: {a.Description} (topics: {string.Join(", ", a.Expertise.Topics)})"));
    }

    private void SaveCustomAgents()
    {
        if (_docGenOptions.Cache.Enabled == false) { return; }
        try
        {
            List<AgentDefinition> custom = _agents.Values.Where(a => !a.IsBuiltIn).ToList();

            if (custom.Count == 0)
                return;

            string? dir = Path.GetDirectoryName(_registryPath);
            if (dir != null)
                Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(custom, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_registryPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save agents");
        }
    }
}