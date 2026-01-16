using CdCSharp.DocGen.Core.Abstractions.Agents;
using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Abstractions.Formatting;
using CdCSharp.DocGen.Core.Models.Agents;
using CdCSharp.DocGen.Core.Models.Analysis;
using CdCSharp.DocGen.Core.Models.Options;
using CdCSharp.DocGen.Core.Models.Orchestration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CdCSharp.DocGen.Core.Agents;

public partial class Orchestrator : IOrchestrator
{
    private readonly IAiClient _aiClient;
    private readonly IAgentRegistry _registry;
    private readonly IAgentFactory _agentFactory;
    private readonly IExpertiseContextBuilder _contextBuilder;
    private readonly IPlainTextFormatter _formatter;
    private readonly ILogger<Orchestrator> _logger;
    private readonly ConversationOptions _conversationOptions;

    private readonly List<AgentMessage> _conversationHistory = [];
    private readonly ConcurrentDictionary<string, IAgent> _activeAgents = new();
    private string? _compressionSummary;

    private ProjectStructure? _currentStructure;
    private Dictionary<string, DestructuredAssembly>? _currentDestructured;

    public IReadOnlyList<AgentMessage> ConversationHistory => _conversationHistory.AsReadOnly();
    public IReadOnlyDictionary<string, IAgent> ActiveAgents => _activeAgents;

    public Orchestrator(
        IAiClient aiClient,
        IAgentRegistry registry,
        AgentFactory agentFactory,
        IExpertiseContextBuilder contextBuilder,
        IPlainTextFormatter formatter,
        IOptions<DocGenOptions> options,
        ILogger<Orchestrator> logger)
    {
        _aiClient = aiClient;
        _registry = registry;
        _agentFactory = agentFactory;
        _contextBuilder = contextBuilder;
        _formatter = formatter;
        _logger = logger;
        _conversationOptions = options.Value.Conversation;

        agentFactory.SetQueryHandler(HandleAgentQueryAsync);

        _conversationHistory.Add(new AgentMessage
        {
            Role = AgentMessageRole.System,
            Content = BuildOrchestratorSystemPrompt()
        });
    }

    public async Task<OrchestrationPlan> CreatePlanAsync(
        ProjectStructure structure,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        _currentStructure = structure;
        _currentDestructured = destructured;

        _logger.LogInformation("Creating documentation plan...");

        string contextMessage = BuildProjectContextMessage(structure, destructured);

        _conversationHistory.Add(new AgentMessage
        {
            Role = AgentMessageRole.User,
            Content = contextMessage
        });

        string planRequest = BuildPlanRequestMessage();

        _conversationHistory.Add(new AgentMessage
        {
            Role = AgentMessageRole.User,
            Content = planRequest
        });

        string response = await SendToModelAsync(maxTokens: 4000);

        _conversationHistory.Add(new AgentMessage
        {
            Role = AgentMessageRole.Assistant,
            Content = response
        });

        OrchestrationPlan? plan = ParsePlanResponse(response);

        if (plan == null || plan.Tasks.Count == 0)
        {
            _logger.LogWarning("Failed to parse plan, using fallback");
            return CreateFallbackPlan(structure);
        }

        await ProcessAgentCreationRequests(response);

        _logger.LogInformation("Plan created: {Count} tasks", plan.Tasks.Count);

        return plan;
    }

    public async Task<List<AgentResult>> ExecutePlanAsync(
        OrchestrationPlan plan,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        _currentDestructured = destructured;
        List<AgentResult> results = [];

        IOrderedEnumerable<AgentTask> orderedTasks = plan.Tasks.OrderBy(t => t.Priority);

        int taskNumber = 0;
        foreach (AgentTask task in orderedTasks)
        {
            taskNumber++;
            _logger.LogInformation("Executing: {Name} ({Current}/{Total})",
                task.Name, taskNumber, plan.Tasks.Count);

            IAgent agent = await GetOrCreateAgentAsync(task.AgentId);

            string context = await BuildTaskContextAsync(task, plan.CriticalContext, destructured);
            agent.LoadExpertiseContext(context);

            foreach (TaskInstruction instruction in task.Instructions)
            {
                _logger.LogDebug("Running instruction: {Id}", instruction.Id);

                string prompt = $"""
                    TASK: {instruction.Instruction}
                    
                    EXPECTED OUTPUT: {instruction.ExpectedOutput}
                    """;

                string response = await agent.ExecuteAsync(prompt, instruction.MaxTokens);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    results.Add(new AgentResult
                    {
                        AgentId = task.AgentId,
                        TaskId = instruction.Id,
                        Content = response,
                        TargetSections = task.TargetSections,
                        TokenCount = response.Length / 4
                    });
                }
            }

            RecordAgentCompletion(task.AgentId, task.Name);
        }

        return results;
    }

    public async Task<AgentQueryResult> HandleAgentQueryAsync(AgentQuery query)
    {
        _logger.LogDebug("Handling query from {From}: {Question}",
            query.FromAgentId, query.Question[..Math.Min(50, query.Question.Length)]);

        IAgent? targetAgent = null;

        if (!string.IsNullOrEmpty(query.TargetAgentId))
        {
            _activeAgents.TryGetValue(query.TargetAgentId, out targetAgent);
        }

        if (targetAgent == null && !string.IsNullOrEmpty(query.TargetExpertise))
        {
            AgentDefinition? definition = _registry.FindByExpertise(query.TargetExpertise);

            if (definition != null)
            {
                targetAgent = await GetOrCreateAgentAsync(definition.Id);
            }
            else
            {
                targetAgent = await CreateAgentForExpertiseAsync(query.TargetExpertise);
            }
        }

        if (targetAgent == null)
        {
            return new AgentQueryResult
            {
                Success = false,
                Response = "No suitable agent found for this query."
            };
        }

        AgentQueryResult result = await targetAgent.QueryAsync(query);

        RecordAgentInteraction(query.FromAgentId, targetAgent.Id, query.Question, result.Response);

        return result;
    }

    public async Task<IAgent> GetOrCreateAgentAsync(string agentId, AgentCreationRequest? creationRequest = null)
    {
        if (_activeAgents.TryGetValue(agentId, out IAgent? existing))
            return existing;

        AgentDefinition? definition = _registry.Get(agentId);

        if (definition == null && creationRequest != null)
        {
            definition = _agentFactory.BuildDefinition(creationRequest);
            _registry.Register(definition);
            _logger.LogInformation("Created new agent: {Name} ({Id})", definition.Name, definition.Id);
        }

        if (definition == null)
            throw new AgentNotFoundException($"Agent not found: {agentId}");

        IAgent agent = _agentFactory.Create(definition);

        if (_currentStructure != null && _currentDestructured != null)
        {
            string context = await _contextBuilder.BuildContextAsync(
                definition.Expertise,
                _currentStructure,
                _currentDestructured);

            agent.LoadExpertiseContext(context);
        }

        _activeAgents[agentId] = agent;
        _logger.LogInformation("Agent activated: {Name} ({Id})", definition.Name, definition.Id);

        return agent;
    }

    private async Task<IAgent> CreateAgentForExpertiseAsync(string expertise)
    {
        _logger.LogInformation("Creating agent for expertise: {Expertise}", expertise);

        AgentCreationRequest request = new()
        {
            Name = $"{expertise} Specialist",
            Description = $"Specialist for {expertise} related documentation",
            Expertise = new AgentExpertise
            {
                Topics = [expertise]
            },
            Reason = "Created on-demand by orchestrator"
        };

        AgentDefinition definition = _agentFactory.BuildDefinition(request);
        _registry.Register(definition);

        return await GetOrCreateAgentAsync(definition.Id);
    }

    private async Task<string> SendToModelAsync(int maxTokens)
    {
        List<ChatMessage> messages = _conversationHistory
            .Select(m => new ChatMessage(
                m.Role == AgentMessageRole.System ? "system" :
                m.Role == AgentMessageRole.Assistant ? "assistant" : "user",
                m.Content))
            .ToList();

        return await _aiClient.SendMessagesAsync(messages, maxTokens);
    }

    private string BuildOrchestratorSystemPrompt()
    {
        return $$"""
            You are the Documentation Orchestrator for .NET projects.
            
            Your responsibilities:
            1. Analyze project structure and decide which agents are needed
            2. Create documentation plans with specific tasks
            3. Create new agents when needed for specific code areas
            4. Coordinate communication between agents
            
            AVAILABLE AGENTS:
            {{_registry.GetAgentListForPrompt()}}
            
            TO CREATE A NEW AGENT, include in your response:
            [CREATE_AGENT]
            {
              "name": "Agent Name",
              "description": "What this agent does",
              "expertise": {
                "assemblies": ["AssemblyName"],
                "filePatterns": ["*.cs"],
                "topics": ["topic1", "topic2"]
              },
              "reason": "Why this agent is needed"
            }
            [/CREATE_AGENT]
            
            When creating a plan, respond with valid JSON following the OrchestrationPlan schema.
            """;
    }

    private string BuildProjectContextMessage(
        ProjectStructure structure,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        StringBuilder sb = new();

        sb.AppendLine("PROJECT STRUCTURE:");
        sb.AppendLine(_formatter.FormatStructure(structure));
        sb.AppendLine();

        sb.AppendLine("ASSEMBLIES AND CONTENTS:");
        foreach ((string name, DestructuredAssembly assembly) in destructured)
        {
            sb.AppendLine($"  {name}:");
            sb.AppendLine($"    Namespaces: {assembly.Namespaces.Count}");
            sb.AppendLine($"    Types: {assembly.Namespaces.Sum(n => n.Types.Count)}");
            sb.AppendLine($"    Components: {assembly.Components.Count}");
            sb.AppendLine($"    TypeScript: {assembly.TypeScript.Count}");
            sb.AppendLine($"    CSS: {assembly.Css.Count}");

            List<string> keyTypes = assembly.Namespaces
                .SelectMany(n => n.Types)
                .Where(t => t.Kind == TypeKind.Interface ||
                           t.Attributes.Any(a => a.Contains("Generator")))
                .Take(5)
                .Select(t => t.Name)
                .ToList();

            if (keyTypes.Count > 0)
                sb.AppendLine($"    Key Types: {string.Join(", ", keyTypes)}");
        }

        return sb.ToString();
    }

    private string BuildPlanRequestMessage()
    {
        return """
            Create a documentation plan for this project.
            
            Consider:
            1. Which existing agents should be used?
            2. Are there code areas that need NEW agents?
            3. How should tasks be divided to respect token limits?
            4. Which files need to be included for each agent?
            
            Respond with a valid JSON OrchestrationPlan:
            {
              "projectType": "detected type",
              "criticalContext": "important project-wide info",
              "tasks": [
                {
                  "agentId": "existing_agent_id",
                  "name": "Task Name",
                  "focus": "specific focus",
                  "targetSections": ["section-id"],
                  "priority": 1,
                  "instructions": [
                    {
                      "instruction": "what to do",
                      "expectedOutput": "what to produce",
                      "maxTokens": 2000,
                      "requiredFiles": {
                        "destructured": ["AssemblyName"],
                        "fullContent": ["path/to/file.cs"]
                      }
                    }
                  ]
                }
              ],
              "outputSections": [
                {
                  "id": "section-id",
                  "title": "Section Title",
                  "order": 1,
                  "description": "what this section contains"
                }
              ],
              "keyFiles": ["important/file.cs"]
            }
            
            If you need to create new agents, include [CREATE_AGENT] blocks BEFORE the JSON.
            """;
    }

    private async Task ProcessAgentCreationRequests(string response)
    {
        MatchCollection matches = CreateAgentRegex().Matches(response);

        foreach (Match match in matches)
        {
            try
            {
                string json = match.Groups[1].Value;
                AgentCreationRequest? request = JsonSerializer.Deserialize<AgentCreationRequest>(json);

                if (request != null)
                {
                    AgentDefinition definition = _agentFactory.BuildDefinition(request);
                    _registry.Register(definition);

                    _logger.LogInformation("Created agent from plan: {Name} ({Id})",
                        definition.Name, definition.Id);

                    RecordAgentCreation(definition.Id, definition.Name, request.Reason);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse agent creation request");
            }
        }
    }

    private OrchestrationPlan? ParsePlanResponse(string response)
    {
        try
        {
            string cleanedResponse = CreateAgentRegex().Replace(response, "").Trim();

            int start = cleanedResponse.IndexOf('{');
            int end = cleanedResponse.LastIndexOf('}');

            if (start >= 0 && end > start)
            {
                string json = cleanedResponse[start..(end + 1)];
                return JsonSerializer.Deserialize<OrchestrationPlan>(json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse orchestration plan");
        }

        return null;
    }

    private async Task<string> BuildTaskContextAsync(
        AgentTask task,
        string criticalContext,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        StringBuilder sb = new();

        if (!string.IsNullOrWhiteSpace(criticalContext))
        {
            sb.AppendLine("CRITICAL CONTEXT:");
            sb.AppendLine(criticalContext);
            sb.AppendLine();
        }

        foreach (string assemblyName in task.Instructions.SelectMany(i => i.RequiredFiles.Destructured).Distinct())
        {
            if (destructured.TryGetValue(assemblyName, out DestructuredAssembly? assembly))
            {
                sb.AppendLine(_formatter.FormatDestructured(assembly));
                sb.AppendLine();
            }
        }

        foreach (string filePath in task.Instructions.SelectMany(i => i.RequiredFiles.FullContent).Distinct())
        {
            string fullPath = Path.Combine(_currentStructure?.RootPath ?? "", filePath);
            if (File.Exists(fullPath))
            {
                try
                {
                    string content = await File.ReadAllTextAsync(fullPath);
                    sb.AppendLine($"=== FILE: {filePath} ===");
                    sb.AppendLine(content.Length > 8000 ? content[..8000] + "\n// truncated" : content);
                    sb.AppendLine();
                }
                catch { }
            }
        }

        return sb.ToString();
    }

    private void RecordAgentCreation(string agentId, string name, string reason)
    {
        _conversationHistory.Add(new AgentMessage
        {
            Role = AgentMessageRole.System,
            Content = $"Created agent: {name} ({agentId}). Reason: {reason}"
        });
    }

    private void RecordAgentCompletion(string agentId, string name)
    {
        _conversationHistory.Add(new AgentMessage
        {
            Role = AgentMessageRole.System,
            Content = $"Agent {name} ({agentId}) completed its tasks."
        });
    }

    private void RecordAgentInteraction(string fromId, string toId, string question, string response)
    {
        _conversationHistory.Add(new AgentMessage
        {
            Role = AgentMessageRole.System,
            Content = $"Agent interaction: {fromId} → {toId}\nQ: {question[..Math.Min(100, question.Length)]}...\nA: {response[..Math.Min(100, response.Length)]}..."
        });
    }

    private OrchestrationPlan CreateFallbackPlan(ProjectStructure structure)
    {
        List<string> mainAssemblies = structure.Assemblies
            .Where(a => !a.IsTestProject)
            .Select(a => a.Name)
            .ToList();

        return new OrchestrationPlan
        {
            ProjectType = structure.Summary.ProjectType,
            CriticalContext = $"This is a {structure.Summary.ProjectType} project.",
            Tasks =
            [
                new AgentTask
                {
                    AgentId = "api_specialist",
                    Name = "API Documentation",
                    Focus = "Document public API",
                    TargetSections = ["public-api"],
                    Priority = 1,
                    Instructions =
                    [
                        new TaskInstruction
                        {
                            Instruction = "Document the main public interfaces",
                            ExpectedOutput = "API documentation",
                            MaxTokens = 2000,
                            RequiredFiles = new RequiredFiles { Destructured = mainAssemblies }
                        }
                    ]
                }
            ],
            OutputSections =
            [
                new DocumentSection
                {
                    Id = "public-api",
                    Title = "Public API",
                    Order = 1,
                    Description = "Public interfaces"
                }
            ],
            KeyFiles = []
        };
    }

    [GeneratedRegex(@"\[CREATE_AGENT\]\s*(\{[\s\S]*?\})\s*\[/CREATE_AGENT\]", RegexOptions.Compiled)]
    private static partial Regex CreateAgentRegex();
}