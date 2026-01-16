using CdCSharp.DocGen.Core.Abstractions.Orchestration;
using CdCSharp.DocGen.Core.Models.Options;
using CdCSharp.DocGen.Core.Models.Orchestration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CdCSharp.DocGen.Core.Orchestration;

public class SpecialistRegistry : ISpecialistRegistry
{
    private readonly string _registryPath;
    private readonly ILogger<SpecialistRegistry> _logger;
    private readonly Dictionary<string, SpecialistDefinition> _specialists;

    private static readonly List<SpecialistDefinition> BuiltInSpecialists =
[
    new()
    {
        Id = "api_specialist",
        Name = "API Specialist",
        Description = "Documents public API contracts, interfaces, and services",
        DefaultFocus = "Document public API and contracts",
        Capabilities = ["interfaces", "services", "contracts", "public-api", "abstractions"],
        SystemPrompt = """
            You are an API Documentation Specialist for .NET projects.
            Your expertise is in documenting public interfaces, service contracts, and API surfaces.
            
            You focus on:
            - Interface definitions and their purposes
            - Service contracts and their implementations
            - Public method signatures and parameters
            - Return types and error handling patterns
            - Usage examples and best practices
            
            Always write clear, professional technical documentation.
            Use markdown formatting. Include code examples where relevant.
            """,
        IsBuiltIn = true
    },
    new()
    {
        Id = "architecture_specialist",
        Name = "Architecture Specialist",
        Description = "Explains architectural patterns, structure, and dependencies",
        DefaultFocus = "Explain architectural patterns and design decisions",
        Capabilities = ["patterns", "architecture", "dependencies", "structure", "design"],
        SystemPrompt = """
            You are an Architecture Documentation Specialist for .NET projects.
            Your expertise is in explaining architectural decisions and patterns.
            
            You focus on:
            - Project structure and organization
            - Design patterns used (CQRS, Repository, etc.)
            - Dependency relationships between components
            - Layer separation and boundaries
            - Configuration and extensibility points
            
            Always explain the "why" behind architectural decisions.
            Use diagrams descriptions when helpful.
            """,
        IsBuiltIn = true
    },
    new()
    {
        Id = "component_specialist",
        Name = "Component Specialist",
        Description = "Documents Blazor components, parameters, and usage",
        DefaultFocus = "Document Blazor components and their usage",
        Capabilities = ["blazor", "components", "parameters", "razor", "ui"],
        SystemPrompt = """
            You are a Blazor Component Documentation Specialist.
            Your expertise is in documenting UI components for Blazor applications.
            
            You focus on:
            - Component parameters and their types
            - Event callbacks and user interactions
            - Cascading parameters and state management
            - Render fragments and templating
            - CSS isolation and styling
            - Usage examples with code snippets
            
            Always provide practical examples showing how to use each component.
            """,
        IsBuiltIn = true
    },
    new()
    {
        Id = "generator_specialist",
        Name = "Generator Specialist",
        Description = "Documents Source Generators and generated code",
        DefaultFocus = "Explain source generators and code generation",
        Capabilities = ["generators", "roslyn", "code-generation", "compile-time"],
        SystemPrompt = """
            You are a Source Generator Documentation Specialist for .NET.
            Your expertise is in Roslyn source generators and compile-time code generation.
            
            You focus on:
            - Generator triggers and syntax detection
            - Generated code patterns and outputs
            - Attributes that control generation
            - Integration with the build process
            - Debugging and diagnostics
            
            Explain both the generator implementation and the generated output.
            """,
        IsBuiltIn = true
    },
    new()
    {
        Id = "frontend_specialist",
        Name = "Frontend Specialist",
        Description = "Documents TypeScript, CSS, themes, and styles",
        DefaultFocus = "Document frontend assets and styling",
        Capabilities = ["typescript", "css", "scss", "themes", "styles", "frontend"],
        SystemPrompt = """
            You are a Frontend Documentation Specialist.
            Your expertise is in TypeScript, CSS, and frontend integration.
            
            You focus on:
            - TypeScript modules and their exports
            - CSS variables and theming systems
            - Style organization and naming conventions
            - JavaScript interop patterns
            - Build and bundling configuration
            
            Document how frontend assets integrate with the .NET backend.
            """,
        IsBuiltIn = true
    },
    new()
    {
        Id = "integration_specialist",
        Name = "Integration Specialist",
        Description = "Documents configuration, DI, and extensions",
        DefaultFocus = "Document integration and configuration",
        Capabilities = ["configuration", "di", "extensions", "setup", "integration"],
        SystemPrompt = """
            You are an Integration Documentation Specialist for .NET.
            Your expertise is in dependency injection, configuration, and library integration.
            
            You focus on:
            - Service registration and DI setup
            - Configuration options and settings
            - Extension methods for easy integration
            - Middleware and pipeline configuration
            - Getting started guides
            
            Always provide step-by-step integration instructions.
            """,
        IsBuiltIn = true
    }
];

    public SpecialistRegistry(IOptions<DocGenOptions> options, ILogger<SpecialistRegistry> logger)
    {
        _logger = logger;
        _registryPath = Path.Combine(options.Value.ProjectPath, ".doccache", "specialists.json");
        _specialists = LoadSpecialists();
    }

    private Dictionary<string, SpecialistDefinition> LoadSpecialists()
    {
        Dictionary<string, SpecialistDefinition> result = BuiltInSpecialists.ToDictionary(s => s.Id);

        if (File.Exists(_registryPath))
        {
            try
            {
                string json = File.ReadAllText(_registryPath);
                List<SpecialistDefinition>? custom = JsonSerializer.Deserialize<List<SpecialistDefinition>>(json);

                if (custom != null)
                {
                    foreach (SpecialistDefinition spec in custom)
                    {
                        result[spec.Id] = spec;
                    }
                    _logger.LogDebug("Loaded {Count} custom specialists", custom.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load custom specialists");
            }
        }

        return result;
    }

    public IReadOnlyList<SpecialistDefinition> GetAll() => _specialists.Values.ToList();

    public SpecialistDefinition? Get(string id) =>
        _specialists.TryGetValue(id, out SpecialistDefinition? spec) ? spec : null;

    public void Register(SpecialistDefinition specialist)
    {
        if (_specialists.TryGetValue(specialist.Id, out SpecialistDefinition? existing) && existing.IsBuiltIn)
        {
            _logger.LogWarning("Cannot override built-in specialist: {SpecialistId}", specialist.Id);
            return;
        }

        _specialists[specialist.Id] = specialist with { IsBuiltIn = false, CreatedAt = DateTime.UtcNow };
        SaveCustomSpecialists();
        _logger.LogInformation("Registered new specialist: {Name}", specialist.Name);
    }

    private void SaveCustomSpecialists()
    {
        try
        {
            List<SpecialistDefinition> custom = _specialists.Values.Where(s => !s.IsBuiltIn).ToList();

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
            _logger.LogWarning(ex, "Failed to save specialists");
        }
    }

    public string GetSpecialistListForPrompt()
    {
        return string.Join("\n", _specialists.Values.Select(s =>
            $"- {s.Id}: {s.Description} (capabilities: {string.Join(", ", s.Capabilities)})"));
    }
}