// Infrastructure/SessionManager.cs
using CdCSharp.Theon.Agents;
using CdCSharp.Theon.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CdCSharp.Theon.Infrastructure;

public class SessionManager
{
    private readonly string _sessionsPath;
    private readonly TheonLogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SessionManager(string outputPath, TheonLogger logger)
    {
        _sessionsPath = Path.Combine(outputPath, "sessions");
        _logger = logger;

        Directory.CreateDirectory(_sessionsPath);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public async Task<string> SaveSessionAsync(SessionState state)
    {
        string sessionId = state.SessionId ?? Guid.NewGuid().ToString("N")[..8];
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"session_{sessionId}_{timestamp}.json";
        string filePath = Path.Combine(_sessionsPath, fileName);

        state = state with { SessionId = sessionId, SavedAt = DateTime.UtcNow };

        string json = JsonSerializer.Serialize(state, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);

        _logger.Info($"Session saved: {fileName}");
        return filePath;
    }

    public async Task<SessionState?> LoadSessionAsync(string sessionIdOrPath)
    {
        string filePath;

        if (File.Exists(sessionIdOrPath))
        {
            filePath = sessionIdOrPath;
        }
        else
        {
            string[] matches = Directory.GetFiles(_sessionsPath, $"session_{sessionIdOrPath}*.json");
            if (matches.Length == 0)
            {
                _logger.Warning($"Session not found: {sessionIdOrPath}");
                return null;
            }

            filePath = matches.OrderByDescending(f => f).First();
        }

        string json = await File.ReadAllTextAsync(filePath);
        SessionState? state = JsonSerializer.Deserialize<SessionState>(json, _jsonOptions);

        if (state != null)
        {
            _logger.Info($"Session loaded: {Path.GetFileName(filePath)}");
            _logger.Info($"  Agents: {state.Agents.Count}");
            _logger.Info($"  Saved: {state.SavedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return state;
    }

    public List<SessionInfo> ListSessions()
    {
        if (!Directory.Exists(_sessionsPath))
            return [];

        List<SessionInfo> sessions = [];

        foreach (string file in Directory.GetFiles(_sessionsPath, "session_*.json"))
        {
            try
            {
                string json = File.ReadAllText(file);
                SessionState? state = JsonSerializer.Deserialize<SessionState>(json, _jsonOptions);

                if (state != null)
                {
                    sessions.Add(new SessionInfo
                    {
                        SessionId = state.SessionId ?? "unknown",
                        FilePath = file,
                        SavedAt = state.SavedAt,
                        AgentCount = state.Agents.Count,
                        ProjectPath = state.ProjectPath
                    });
                }
            }
            catch
            {
                // Skip invalid files
            }
        }

        return sessions.OrderByDescending(s => s.SavedAt).ToList();
    }

    public void RestoreAgents(SessionState state, AgentRegistry registry, AgentFactory factory)
    {
        foreach (SerializedAgentState agentState in state.Agents)
        {
            Agent agent = new()
            {
                Name = agentState.Name,
                Expertise = agentState.Expertise
            };

            agent.ConversationHistory.AddRange(agentState.ConversationHistory);
            agent.Context = agentState.Context;

            registry.Register(agent);

            if (agentState.State == Models.AgentState.Sleeping)
            {
                factory.Sleep(agent);
            }

            _logger.Debug($"Restored agent: {agent.Name} ({agent.Id})");
        }
    }
}

public record SessionState
{
    public string? SessionId { get; init; }
    public DateTime SavedAt { get; init; }
    public string ProjectPath { get; init; } = "";
    public List<SerializedAgentState> Agents { get; init; } = [];
    public List<ConversationMessage> OrchestratorHistory { get; init; } = [];
    public MetricsSummary? Metrics { get; init; }
}

public record SerializedAgentState
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Expertise { get; init; } = "";
    public string Context { get; init; } = "";
    public Models.AgentState State { get; init; }
    public List<ConversationMessage> ConversationHistory { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime LastActiveAt { get; init; }
}

public record SessionInfo
{
    public string SessionId { get; init; } = "";
    public string FilePath { get; init; } = "";
    public DateTime SavedAt { get; init; }
    public int AgentCount { get; init; }
    public string ProjectPath { get; init; } = "";
}