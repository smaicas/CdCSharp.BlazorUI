using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Abstractions.Formatting;
using CdCSharp.DocGen.Core.Abstractions.Orchestration;
using CdCSharp.DocGen.Core.Models.Analysis;
using CdCSharp.DocGen.Core.Models.Orchestration;
using Microsoft.Extensions.Logging;

namespace CdCSharp.DocGen.Core.Orchestration;

public class Orchestrator : IOrchestrator
{
    private readonly IAiClient _ai;
    private readonly ISpecialistRegistry _registry;
    private readonly IPlainTextFormatter _formatter;
    private readonly ILogger<Orchestrator> _logger;

    public Orchestrator(
        IAiClient ai,
        ISpecialistRegistry registry,
        IPlainTextFormatter formatter,
        ILogger<Orchestrator> logger)
    {
        _ai = ai;
        _registry = registry;
        _formatter = formatter;
        _logger = logger;
    }

    public async Task<OrchestrationPlan> CreatePlanAsync(
        ProjectStructure structure,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        _logger.LogInformation("Creating documentation plan...");

        string prompt = BuildOrchestratorPrompt(structure, destructured);

        _logger.LogDebug("Orchestrator prompt: {Length} chars (~{Tokens} tokens)", prompt.Length, prompt.Length / 4);

        OrchestrationPlan? plan = await _ai.SendAsync<OrchestrationPlan>(prompt, maxTokens: 3000);

        if (plan == null || plan.Specialists.Count == 0)
        {
            _logger.LogWarning("Orchestrator returned empty plan, using fallback");
            return CreateFallbackPlan(structure, destructured);
        }

        _logger.LogDebug("Plan received: {SpecialistCount} specialists, {SectionCount} sections",
            plan.Specialists.Count, plan.OutputSections.Count);

        foreach (SpecialistTask specialist in plan.Specialists)
        {
            _logger.LogDebug("Specialist: {Name} ({Id}), Priority: {Priority}, Prompts: {PromptCount}",
                specialist.Name, specialist.SpecialistId, specialist.Priority, specialist.Prompts.Count);
        }

        RegisterNewSpecialists(plan);

        _logger.LogInformation("Plan created: {SpecialistCount} specialists, {SectionCount} sections",
            plan.Specialists.Count, plan.OutputSections.Count);

        return plan;
    }

    private string BuildOrchestratorPrompt(
       ProjectStructure structure,
       Dictionary<string, DestructuredAssembly> destructured)
    {
        string assembliesSection = string.Join(
            Environment.NewLine + Environment.NewLine,
            destructured.Select(kvp =>
            {
                string name = kvp.Key;
                DestructuredAssembly assembly = kvp.Value;

                List<string> keyTypes = assembly.Namespaces
                    .SelectMany(n => n.Types)
                    .Where(t =>
                        t.Kind == TypeKind.Interface ||
                        t.Attributes.Any(a => a.Contains("Generator")))
                    .Take(5)
                    .Select(t => t.Name)
                    .ToList();

                List<string> files = assembly.Namespaces
                    .SelectMany(n => n.Types)
                    .Select(t => t.File)
                    .Distinct()
                    .Take(10)
                    .ToList();

                string keyTypesLine = keyTypes.Count > 0
                    ? $"    Key Types: {string.Join(", ", keyTypes)}"
                    : string.Empty;

                string filesLine = files.Count > 0
                    ? $"    Files: {string.Join(", ", files)}"
                    : string.Empty;

                return $$"""
              {{name}}:
                Namespaces: {{assembly.Namespaces.Count}}
                Types: {{assembly.Namespaces.Sum(n => n.Types.Count)}}
                Components: {{assembly.Components.Count}}
                TypeScript: {{assembly.TypeScript.Count}}
                CSS: {{assembly.Css.Count}}
            {{keyTypesLine}}
            {{filesLine}}
            """.TrimEnd();
            })
        );

        return $$"""
    You are a technical documentation architect. Analyze this C# project and create a documentation plan.

    PROJECT STRUCTURE:
    {{_formatter.FormatStructure(structure)}}

    AVAILABLE FILES BY ASSEMBLY:
    {{assembliesSection}}

    AVAILABLE SPECIALISTS:
    {{_registry.GetSpecialistListForPrompt()}}

    CREATE A DOCUMENTATION PLAN. RESPOND ONLY WITH VALID JSON:
    {
      "projectType": "detected project type",
      "criticalContext": "key information that ALL specialists must know about this project",
      "specialists": [
        {
          "specialistId": "api_specialist",
          "name": "API Specialist",
          "focus": "specific focus for this project",
          "targetSections": ["section-id-1"],
          "prompts": [
            {
              "id": "prompt-1",
              "instruction": "detailed instruction",
              "expectedOutput": "what to produce",
              "maxTokens": 2000,
              "requiredFiles": {
                "destructured": ["AssemblyName"],
                "fullContent": ["path/to/file.cs"]
              }
            }
          ],
          "priority": 1
        }
      ],
      "outputSections": [
        {
          "id": "overview",
          "title": "Overview",
          "order": 1,
          "description": "section description"
        }
      ],
      "keyFiles": ["path/to/critical/file.cs"]
    }

    RULES:
    - Use existing specialistId when possible, or create new ones if needed
    - Each specialist can have multiple prompts if the task is complex
    - Divide the task having in account prompt limits (~10000 tokens including context)
    - requiredFiles.destructured = assembly names for structure info
    - requiredFiles.fullContent = specific files whose complete code the specialist needs
    - keyFiles = critical files to include in final LLM documentation output
    - criticalContext = project-wide info to include in every specialist prompt
    """;
    }

    private void RegisterNewSpecialists(OrchestrationPlan plan)
    {
        _logger.LogDebug("Registering new specialists from plan...");

        int newCount = 0;
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
                _logger.LogDebug("Registered new specialist: {Name} ({Id})", task.Name, task.SpecialistId);
                newCount++;
            }
        }

        if (newCount > 0)
            _logger.LogDebug("Total new specialists registered: {Count}", newCount);
    }

    private OrchestrationPlan CreateFallbackPlan(
        ProjectStructure structure,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        _logger.LogDebug("Creating fallback plan...");

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

        _logger.LogDebug("Main assemblies for fallback plan: {Assemblies}", string.Join(", ", mainAssemblies));

        specialists.Add(new SpecialistTask
        {
            SpecialistId = "api_specialist",
            Name = "API Specialist",
            Focus = "Document public API contracts",
            TargetSections = ["public-api"],
            Prompts =
            [
                new SpecialistPrompt
                {
                    Id = "api-1",
                    Instruction = "Document the main public interfaces and their purposes",
                    ExpectedOutput = "API documentation with interface descriptions",
                    MaxTokens = 2000,
                    RequiredFiles = new RequiredFiles { Destructured = mainAssemblies },
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

        if (structure.Summary.TotalComponents > 0)
        {
            _logger.LogDebug("Adding component specialist (found {Count} components)", structure.Summary.TotalComponents);

            specialists.Add(new SpecialistTask
            {
                SpecialistId = "component_specialist",
                Name = "Component Specialist",
                Focus = "Document Blazor components",
                TargetSections = ["components"],
                Prompts =
                [
                    new SpecialistPrompt
                    {
                        Id = "comp-1",
                        Instruction = "Document Blazor components with their parameters and usage",
                        ExpectedOutput = "Component documentation with examples",
                        MaxTokens = 2000,
                        RequiredFiles = new RequiredFiles { Destructured = mainAssemblies },
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

        if (structure.Summary.DetectedPatterns.Count > 0)
        {
            _logger.LogDebug("Adding architecture specialist (detected patterns: {Patterns})",
                string.Join(", ", structure.Summary.DetectedPatterns));

            specialists.Add(new SpecialistTask
            {
                SpecialistId = "architecture_specialist",
                Name = "Architecture Specialist",
                Focus = "Explain architectural patterns",
                TargetSections = ["architecture"],
                Prompts =
                [
                    new SpecialistPrompt
                    {
                        Id = "arch-1",
                        Instruction = $"Explain the architectural patterns used: {string.Join(", ", structure.Summary.DetectedPatterns)}",
                        ExpectedOutput = "Architecture overview",
                        MaxTokens = 1500,
                        RequiredFiles = new RequiredFiles { Destructured = mainAssemblies },
                    }
                ],
                Priority = 3
            });

            sections.Add(new DocumentSection
            {
                Id = "architecture",
                Title = "Architecture",
                Order = sectionOrder,
                Description = "Architectural patterns and design"
            });
        }

        _logger.LogDebug("Fallback plan created: {SpecialistCount} specialists, {SectionCount} sections",
            specialists.Count, sections.Count);

        return new OrchestrationPlan
        {
            ProjectType = structure.Summary.ProjectType,
            CriticalContext = $"This is a {structure.Summary.ProjectType} project.",
            Specialists = specialists,
            OutputSections = sections,
            KeyFiles = []
        };
    }
}