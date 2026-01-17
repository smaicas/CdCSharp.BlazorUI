// Infrastructure/AgentVisualizer.cs

using CdCSharp.Theon.Models;

namespace CdCSharp.Theon.Infrastructure;

public class AgentVisualizer
{
    private readonly AgentRegistry _registry;

    public AgentVisualizer(AgentRegistry registry)
    {
        _registry = registry;
    }

    // ✅ ACTUALIZADO: Recibe HashSet<string> de IDs
    public string GenerateMermaidDiagram(HashSet<string>? involvedAgentIds = null)
    {
        IReadOnlyCollection<Agent> agents = _registry.AllAgents;

        if (agents.Count == 0)
            return "```mermaid\ngraph TD\n    O[Orchestrator]\n    O --> N[No agents created]\n```";

        List<string> lines =
        [
            "```mermaid",
            "graph TD",
            "    classDef active fill:#90EE90,stroke:#228B22",
            "    classDef sleeping fill:#FFE4B5,stroke:#DEB887",
            "    classDef involved fill:#87CEEB,stroke:#4682B4,stroke-width:3px",
            "",
            "    O((Orchestrator))"
        ];

        // ✅ CAMBIO: involvedAgentIds ya es un HashSet
        HashSet<string> involved = involvedAgentIds ?? [];

        foreach (Agent agent in agents)
        {
            string nodeId = SanitizeId(agent.Id);
            string label = $"{agent.Name}\\n({agent.Expertise})";
            string shape = agent.State == AgentState.Active ? $"[{label}]" : $"({label})";

            lines.Add($"    {nodeId}{shape}");
            lines.Add($"    O --> {nodeId}");

            // ✅ CAMBIO: Comparar por ID
            if (involved.Contains(agent.Id))
            {
                lines.Add($"    class {nodeId} involved");
            }
            else if (agent.State == AgentState.Active)
            {
                lines.Add($"    class {nodeId} active");
            }
            else
            {
                lines.Add($"    class {nodeId} sleeping");
            }
        }

        lines.Add("");
        lines.Add("    subgraph Legend");
        lines.Add("        L1[Active Agent]:::active");
        lines.Add("        L2(Sleeping Agent):::sleeping");
        lines.Add("        L3[Involved in Query]:::involved");
        lines.Add("    end");

        lines.Add("```");

        return string.Join("\n", lines);
    }

    public string GenerateInteractionDiagram(List<AgentInteraction> interactions)
    {
        if (interactions.Count == 0)
            return string.Empty;

        List<string> lines =
        [
            "```mermaid",
            "sequenceDiagram",
            "    participant U as User",
            "    participant O as Orchestrator"
        ];

        HashSet<string> participants = [];

        // ✅ MEJORADO: Procesar por ID
        foreach (AgentInteraction interaction in interactions)
        {
            if (!string.IsNullOrEmpty(interaction.FromAgentId) && participants.Add(interaction.FromAgentId))
            {
                Agent? agent = _registry.Get(interaction.FromAgentId);
                string name = agent?.Name ?? interaction.FromAgentId;
                lines.Add($"    participant {SanitizeId(interaction.FromAgentId)} as {name}");
            }
            if (!string.IsNullOrEmpty(interaction.ToAgentId) && participants.Add(interaction.ToAgentId))
            {
                Agent? agent = _registry.Get(interaction.ToAgentId);
                string name = agent?.Name ?? interaction.ToAgentId;
                lines.Add($"    participant {SanitizeId(interaction.ToAgentId)} as {name}");
            }
        }

        lines.Add("");

        foreach (AgentInteraction interaction in interactions)
        {
            string from = string.IsNullOrEmpty(interaction.FromAgentId) ? "O" : SanitizeId(interaction.FromAgentId);
            string to = string.IsNullOrEmpty(interaction.ToAgentId) ? "O" : SanitizeId(interaction.ToAgentId);
            string arrow = interaction.Type switch
            {
                InteractionType.Query => "->>",
                InteractionType.Response => "-->>",
                InteractionType.Validation => "->>",
                InteractionType.Creation => "-))",
                _ => "->>"
            };

            string label = interaction.Summary.Length > 30
                ? interaction.Summary[..30] + "..."
                : interaction.Summary;

            lines.Add($"    {from}{arrow}{to}: {label}");
        }

        lines.Add("```");

        return string.Join("\n", lines);
    }

    // ✅ ACTUALIZADO: Recibe HashSet<string> de IDs
    public string GenerateTextSummary(HashSet<string>? involvedAgentIds = null)
    {
        IReadOnlyCollection<Agent> agents = _registry.AllAgents;

        if (agents.Count == 0)
            return "No agents registered.";

        HashSet<string> involved = involvedAgentIds ?? [];

        List<string> lines =
        [
            "## Agent Summary",
            ""
        ];

        foreach (Agent agent in agents.OrderBy(a => a.Name))
        {
            string status = agent.State == AgentState.Active ? "🟢" : "🟡";
            string marker = involved.Contains(agent.Id) ? " ⭐" : "";

            lines.Add($"- {status} **{agent.Name}**{marker}");
            lines.Add($"  - ID: `{agent.Id}`");
            lines.Add($"  - Expertise: {agent.Expertise}");
            lines.Add($"  - Messages: {agent.ConversationHistory.Count}");
            lines.Add($"  - Last Active: {agent.LastActiveAt:yyyy-MM-dd HH:mm:ss}");
            lines.Add("");
        }

        if (involved.Count > 0)
        {
            lines.Add("_⭐ = Involved in current query_");
        }

        return string.Join("\n", lines);
    }

    private static string SanitizeId(string id)
    {
        return "A_" + new string(id.Where(c => char.IsLetterOrDigit(c)).ToArray());
    }
}

public record AgentInteraction
{
    public string? FromAgentId { get; init; }
    public string? ToAgentId { get; init; }
    public InteractionType Type { get; init; }
    public string Summary { get; init; } = "";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public enum InteractionType
{
    Query,
    Response,
    Validation,
    Creation
}