using CdCSharp.DocGen.Core.Formatting;
using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;
using System.Text;

namespace CdCSharp.DocGen.Core.Orchestration;

public class Orchestrator
{
    private readonly IAiClient _ai;
    private readonly SpecialistRegistry _registry;
    private readonly PlainTextFormatter _formatter;
    private readonly ILogger _logger;

    public Orchestrator(IAiClient ai, SpecialistRegistry registry, ILogger? logger = null)
    {
        _ai = ai;
        _registry = registry;
        _formatter = new PlainTextFormatter();
        _logger = logger ?? NullLogger.Instance;
    }

    public async Task<OrchestrationPlan> CreatePlanAsync(
        ProjectStructure structure,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        _logger.Progress("Creating documentation plan...");

        string prompt = BuildOrchestratorPrompt(structure, destructured);
        _logger.Verbose($"Orchestrator prompt: {prompt.Length} chars");

        OrchestrationPlan? plan = await _ai.SendAsync<OrchestrationPlan>(prompt, maxTokens: 3000);

        if (plan == null || plan.Specialists.Count == 0)
        {
            _logger.Warning("Orchestrator returned empty plan, using fallback");
            return CreateFallbackPlan(structure, destructured);
        }

        RegisterNewSpecialists(plan);

        _logger.Success($"Plan created: {plan.Specialists.Count} specialists, {plan.OutputSections.Count} sections");

        return plan;
    }

    private string BuildOrchestratorPrompt(
        ProjectStructure structure,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        StringBuilder sb = new();

        sb.AppendLine("You are a technical documentation architect. Analyze this C# project and create a documentation plan.");
        sb.AppendLine();
        sb.AppendLine("PROJECT STRUCTURE:");
        sb.AppendLine(_formatter.FormatStructure(structure));
        sb.AppendLine();

        sb.AppendLine("AVAILABLE FILES BY ASSEMBLY:");
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
                .Where(t => t.Kind == TypeKind.Interface || t.Attributes.Any(a => a.Contains("Generator")))
                .Take(5)
                .Select(t => t.Name)
                .ToList();

            if (keyTypes.Count > 0)
                sb.AppendLine($"    Key Types: {string.Join(", ", keyTypes)}");
        }
        sb.AppendLine();

        sb.AppendLine("AVAILABLE SPECIALISTS:");
        sb.AppendLine(_registry.GetSpecialistListForPrompt());
        sb.AppendLine();

        sb.AppendLine("CREATE A DOCUMENTATION PLAN. RESPOND ONLY WITH VALID JSON:");
        sb.AppendLine(@"{
  ""projectType"": ""detected project type"",
  ""criticalContext"": ""key information that ALL specialists must know about this project"",
  ""specialists"": [
    {
      ""specialistId"": ""api_specialist"",
      ""name"": ""API Specialist"",
      ""focus"": ""specific focus for this project"",
      ""targetSections"": [""section-id-1"", ""section-id-2""],
      ""requiredFiles"": {
        ""destructured"": [""AssemblyName""],
        ""fullContent"": [""path/to/critical/file.cs""]
      },
      ""prompts"": [
        {
          ""id"": ""prompt-1"",
          ""instruction"": ""detailed instruction for this prompt"",
          ""expectedOutput"": ""what kind of content to produce"",
          ""maxTokens"": 2000
        }
      ],
      ""priority"": 1
    }
  ],
  ""outputSections"": [
    {
      ""id"": ""overview"",
      ""title"": ""Overview"",
      ""order"": 1,
      ""description"": ""what this section should contain""
    }
  ],
  ""keyFiles"": [""path/to/critical/file.cs""]
}");
        sb.AppendLine();
        sb.AppendLine("RULES:");
        sb.AppendLine("- Use existing specialistId when possible, or create new ones if needed");
        sb.AppendLine("- Each specialist can have multiple prompts if the task is complex");
        sb.AppendLine("- requiredFiles.destructured = assembly names for structure info");
        sb.AppendLine("- requiredFiles.fullContent = specific files whose complete code is needed");
        sb.AppendLine("- keyFiles = critical files that should be included in LLM documentation");
        sb.AppendLine("- criticalContext = information that must be in every prompt (project purpose, key patterns, etc)");

        return sb.ToString();
    }

    private void RegisterNewSpecialists(OrchestrationPlan plan)
    {
        foreach (SpecialistTask task in plan.Specialists)
        {
            if (_registry.Get(task.SpecialistId) == null)
            {
                _registry.Register(new SpecialistDefinition
                {
                    Id = task.SpecialistId,
                    Name = task.Name,
                    Description = task.Focus,
                    DefaultFocus = task.Focus,
                    Capabilities = task.TargetSections,
                    IsBuiltIn = false
                });
            }
        }
    }

    private OrchestrationPlan CreateFallbackPlan(
        ProjectStructure structure,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        List<SpecialistTask> specialists = [];
        List<DocumentSection> sections = [];
        int sectionOrder = 1;

        sections.Add(new DocumentSection
        {
            Id = "overview",
            Title = "Overview",
            Order = sectionOrder++,
            Description = "Project overview and purpose"
        });

        List<string> mainAssemblies = structure.Assemblies
            .Where(a => !a.IsTestProject)
            .Select(a => a.Name)
            .ToList();

        specialists.Add(new SpecialistTask
        {
            SpecialistId = "api_specialist",
            Name = "API Specialist",
            Focus = "Document public API contracts",
            TargetSections = ["public-api"],
            RequiredFiles = new RequiredFiles { Destructured = mainAssemblies },
            Prompts =
            [
                new SpecialistPrompt
                {
                    Id = "api-1",
                    Instruction = "Document the main public interfaces and their purposes",
                    ExpectedOutput = "API documentation with interface descriptions",
                    MaxTokens = 2000
                }
            ],
            Priority = 1
        });

        sections.Add(new DocumentSection
        {
            Id = "public-api",
            Title = "Public API",
            Order = sectionOrder++,
            Description = "Public interfaces and contracts"
        });

        if (structure.GlobalSummary.TotalComponents > 0)
        {
            specialists.Add(new SpecialistTask
            {
                SpecialistId = "component_specialist",
                Name = "Component Specialist",
                Focus = "Document Blazor components",
                TargetSections = ["components"],
                RequiredFiles = new RequiredFiles { Destructured = mainAssemblies },
                Prompts =
                [
                    new SpecialistPrompt
                    {
                        Id = "comp-1",
                        Instruction = "Document Blazor components with their parameters and usage",
                        ExpectedOutput = "Component documentation with examples",
                        MaxTokens = 2000
                    }
                ],
                Priority = 2
            });

            sections.Add(new DocumentSection
            {
                Id = "components",
                Title = "Components",
                Order = sectionOrder++,
                Description = "Blazor component documentation"
            });
        }

        if (structure.GlobalSummary.DetectedPatterns.Count > 0)
        {
            specialists.Add(new SpecialistTask
            {
                SpecialistId = "architecture_specialist",
                Name = "Architecture Specialist",
                Focus = "Explain architectural patterns",
                TargetSections = ["architecture"],
                RequiredFiles = new RequiredFiles { Destructured = mainAssemblies },
                Prompts =
                [
                    new SpecialistPrompt
                    {
                        Id = "arch-1",
                        Instruction = $"Explain the architectural patterns used: {string.Join(", ", structure.GlobalSummary.DetectedPatterns)}",
                        ExpectedOutput = "Architecture overview",
                        MaxTokens = 1500
                    }
                ],
                Priority = 3
            });

            sections.Add(new DocumentSection
            {
                Id = "architecture",
                Title = "Architecture",
                Order = sectionOrder++,
                Description = "Architectural patterns and design"
            });
        }

        return new OrchestrationPlan
        {
            ProjectType = structure.GlobalSummary.ProjectType,
            CriticalContext = $"This is a {structure.GlobalSummary.ProjectType} project.",
            Specialists = specialists,
            OutputSections = sections,
            KeyFiles = []
        };
    }
}