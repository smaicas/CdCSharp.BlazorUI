using CdCSharp.DocGen.Core.Abstractions.Agents;
using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Abstractions.Formatting;
using CdCSharp.DocGen.Core.Agents.Exceptions;
using CdCSharp.DocGen.Core.Models.Agents;
using CdCSharp.DocGen.Core.Models.AI;
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

    private ProjectStructure? _currentStructure;
    private Dictionary<string, DestructuredAssembly>? _currentDestructured;

    public IReadOnlyList<AgentMessage> ConversationHistory => _conversationHistory.AsReadOnly();
    public IReadOnlyDictionary<string, IAgent> ActiveAgents => _activeAgents;

    public Orchestrator(
    IAiClient aiClient,
    IAgentRegistry registry,
    IAgentFactory agentFactory,  // Cambiar de AgentFactory a IAgentFactory
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

        _agentFactory.SetQueryHandler(HandleAgentQueryAsync);

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

            IAgent agent = await GetOrCreateAgentAsync(task.AgentId, taskContext: task);  // Pasar task

            string context = await BuildTaskContextAsync(task, plan.CriticalContext, destructured);
            agent.LoadExpertiseContext(context);

            foreach (TaskInstruction instruction in task.Instructions)
            {
                _logger.LogDebug("Running instruction: {Id} (memory: {Memory})",
                    instruction.Id, instruction.RequiresMemory);

                string prompt = $"""
                TASK: {instruction.Instruction}
                
                EXPECTED OUTPUT: {instruction.ExpectedOutput}
                """;

                string response = await agent.ExecuteAsync(
                    prompt,
                    instruction.MaxTokens,
                    instruction.RequiresMemory);

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

    public async Task<IAgent> GetOrCreateAgentAsync(
    string agentId,
    AgentCreationRequest? creationRequest = null,
    AgentTask? taskContext = null)
    {
        if (_activeAgents.TryGetValue(agentId, out IAgent? existing))
            return existing;

        AgentDefinition? definition = _registry.Get(agentId);

        if (definition == null && creationRequest != null)
        {
            definition = _agentFactory.BuildDefinition(creationRequest);
            _registry.Register(definition);
            _logger.LogInformation("Created new agent from request: {Name} ({Id})", definition.Name, definition.Id);
        }

        if (definition == null && taskContext != null)
        {
            _logger.LogWarning("Agent {AgentId} not found, inferring from task context", agentId);

            creationRequest = await InferAgentFromTaskAsync(agentId, taskContext);

            if (creationRequest != null)
            {
                definition = _agentFactory.BuildDefinition(creationRequest);
                _registry.Register(definition);
                _logger.LogInformation("Inferred and created agent: {Name} ({Id})", definition.Name, definition.Id);
                RecordAgentCreation(definition.Id, definition.Name, creationRequest.Reason);
            }
        }

        if (definition == null)
        {
            throw new AgentNotFoundException(agentId);
        }

        IAgent agent = _agentFactory.Create(definition);

        if (_currentStructure != null && _currentDestructured != null)
        {
            string context = await _contextBuilder.BuildContextAsync(
                definition.Expertise,
                _currentStructure,
                _currentDestructured);

            agent.LoadExpertiseContext(context);
        }
        else
        {
            _logger.LogWarning("Agent {Id} created without expertise context - structure not yet loaded", agentId);
        }

        _activeAgents[agentId] = agent;
        _logger.LogInformation("Agent activated: {Name} ({Id})", definition.Name, definition.Id);

        return agent;
    }

    private async Task<AgentCreationRequest?> InferAgentFromTaskAsync(string agentId, AgentTask task)
    {
        string filesContext = string.Join("\n", task.Instructions
            .SelectMany(i => i.RequiredFiles.FullContent.Concat(i.RequiredFiles.Destructured))
            .Distinct()
            .Take(10)
            .Select(f => $"  - {f}"));

        string instructionsContext = string.Join("\n", task.Instructions
            .Take(3)
            .Select(i => $"  - {i.Instruction}"));

        string prompt = $$"""
        An agent with ID "{agentId}" was referenced but not defined.
        Based on the task context below, generate a suitable agent definition.

        TASK CONTEXT:
        - Task Name: {{task.Name}}
        - Focus: {{task.Focus}}
        - Target Sections: {string.Join(", ", task.TargetSections)}
        
        INSTRUCTIONS THIS AGENT SHOULD HANDLE:
        {instructionsContext}
        
        FILES THIS AGENT NEEDS TO WORK WITH:
        {filesContext}

        Respond with ONLY a JSON object (no markdown, no explanation):
        {
          "name": "Human-readable agent name",
          "description": "Clear description of what this agent documents",
          "expertise": {
            "assemblies": ["relevant assembly names"],
            "filePatterns": ["*.pattern.cs"],
            "topics": ["topic1", "topic2"]
          },
          "reason": "Why this agent was created"
        }
        """;

        _logger.LogDebug("Inferring agent definition for {AgentId}", agentId);

        try
        {
            AgentCreationRequest? request = await _aiClient.SendAsync<AgentCreationRequest>(prompt, maxTokens: 500);

            if (request != null)
            {
                _logger.LogDebug("Inferred agent: {Name} - {Description}", request.Name, request.Description);
                return request;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to infer agent definition, using fallback");
        }

        // Fallback: crear agente básico basado en el ID y nombre de tarea
        return new AgentCreationRequest
        {
            Name = GenerateAgentNameFromId(agentId),
            Description = $"Specialist for: {task.Name}",
            Expertise = new AgentExpertise
            {
                Topics = ExtractTopicsFromTask(agentId, task),
                Assemblies = task.Instructions
                    .SelectMany(i => i.RequiredFiles.Destructured)
                    .Distinct()
                    .ToList()
            },
            Reason = $"Auto-inferred from task: {task.Name}"
        };
    }

    private static string GenerateAgentNameFromId(string agentId)
    {
        string[] parts = agentId.Split('_');

        List<string> formattedParts = parts.Select(p =>
            p.Length <= 3
                ? p.ToUpperInvariant()
                : char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()
        ).ToList();

        return string.Join(" ", formattedParts);
    }

    private static List<string> ExtractTopicsFromTask(string agentId, AgentTask task)
    {
        List<string> topics = [];

        // Del ID: "cli_specialist" -> ["cli"]
        string mainTopic = agentId.Replace("_specialist", "").Replace("_", " ");
        topics.Add(mainTopic);

        // Del nombre de tarea: extraer palabras clave
        string[] keywords = ["api", "cli", "ui", "data", "service", "component", "config", "auth"];
        string taskLower = task.Name.ToLowerInvariant();

        topics.AddRange(keywords.Where(k => taskLower.Contains(k)));

        return topics.Distinct().ToList();
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
        You are the Documentation Orchestrator for .NET projects. Your role is to analyze codebases and create comprehensive documentation plans.

        ## YOUR RESPONSIBILITIES
        1. Analyze project structure and identify documentation needs
        2. Select appropriate specialist agents for each documentation area
        3. Create new agents when existing ones don't cover specific expertise
        4. Design efficient task sequences that respect token limits
        5. Decide memory requirements for each instruction

        ## AVAILABLE SPECIALIST AGENTS
        {{_registry.GetAgentListForPrompt()}}

        ## AGENT CREATION
        When you identify code areas not covered by existing agents, create new specialists:

        [CREATE_AGENT]
        {
          "name": "Descriptive Agent Name",
          "description": "Clear description of expertise and responsibilities",
          "expertise": {
            "assemblies": ["TargetAssembly"],
            "filePatterns": ["*.specific.cs", "*Pattern*.cs"],
            "namespaces": ["Namespace.To.Focus"],
            "topics": ["specific-topic", "related-area"]
          },
          "reason": "Justification for why this agent is needed"
        }
        [/CREATE_AGENT]

        ## MEMORY MODE GUIDELINES
        For each TaskInstruction, you must decide `requiresMemory`:

        **Use requiresMemory: FALSE when:**
        - Documenting a single file, type, or isolated component
        - The instruction is self-contained and doesn't reference previous outputs
        - Simple enumeration tasks (list all interfaces, document all properties)
        - Tasks that can run in parallel

        **Use requiresMemory: TRUE when:**
        - Instructions build upon each other sequentially
        - Later instructions reference or expand on earlier results
        - Creating cohesive narratives across multiple aspects
        - When "context from above" or "as mentioned" would be needed

        ## OUTPUT QUALITY PRINCIPLES
        - Prefer many small, focused tasks over few large ones
        - Each instruction should produce 500-2000 tokens of output
        - Group related documentation into logical sections
        - Include full file paths for key files agents need to see
        - Balance thoroughness with token efficiency
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

    private string BuildPlanRequestMessage() => """
        Analyze the project structure provided and create a comprehensive documentation plan.

        ## ANALYSIS STEPS
        1. **Identify Project Type**: What kind of .NET project is this? (Library, API, Blazor, etc.)
        2. **Find Key Components**: What are the main architectural pieces?
        3. **Assess Complexity**: How many specialist agents are needed?
        4. **Plan Sections**: What documentation sections will best serve users?

        ## TASK DESIGN PRINCIPLES
        - Each task should focus on ONE coherent area
        - Instructions within a task should be ordered logically
        - Estimate token usage: ~4 chars per token
        - Include SPECIFIC file paths the agent needs

        ## REQUIRED OUTPUT FORMAT
        Respond with valid JSON matching this schema:

        {
          "projectType": "Detected project type with brief justification",
          "criticalContext": "Essential information ALL agents need (architecture decisions, naming conventions, key patterns)",
          "tasks": [
            {
              "agentId": "existing_or_new_agent_id",
              "name": "Human-readable task name",
              "focus": "Specific area this task covers",
              "targetSections": ["section-id-1"],
              "priority": 1,
              "instructions": [
                {
                  "instruction": "Precise instruction for the agent",
                  "expectedOutput": "Description of expected documentation format and content",
                  "maxTokens": 2000,
                  "requiresMemory": false,
                  "requiredFiles": {
                    "destructured": ["AssemblyName"],
                    "fullContent": ["exact/path/to/important/file.cs"]
                  }
                }
              ]
            }
          ],
          "outputSections": [
            {
              "id": "unique-section-id",
              "title": "Section Title for Documentation",
              "order": 1,
              "description": "What this section contains and why it matters"
            }
          ],
          "keyFiles": ["path/to/critical/file.cs", "path/to/important/interface.cs"]
        }

        ## SECTION ORGANIZATION BEST PRACTICES
        1. **Overview** (order: 1) - Project purpose, architecture summary
        2. **Getting Started** (order: 2) - Installation, configuration, basic usage
        3. **Public API** (order: 3) - Interfaces, services, contracts
        4. **Components** (order: 4) - UI components if applicable
        5. **Architecture** (order: 5) - Patterns, dependencies, design decisions
        6. **Advanced Topics** (order: 6) - Extensions, customization, edge cases

        If you need to create new agents, include [CREATE_AGENT] blocks BEFORE the JSON plan.

        Now analyze the project and generate the plan:
        """;

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