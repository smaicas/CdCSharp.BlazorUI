using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;
using System.Text.Json;

namespace CdCSharp.DocGen.Core.Orchestration;

public class SpecialistRegistry
{
    private readonly string _registryPath;
    private readonly ILogger _logger;
    private Dictionary<string, SpecialistDefinition> _specialists;

    private static readonly List<SpecialistDefinition> BuiltInSpecialists =
    [
        new()
        {
            Id = "api_specialist",
            Name = "API Specialist",
            Description = "Documents public API contracts, interfaces, and services",
            DefaultFocus = "Document public API and contracts",
            Capabilities = ["interfaces", "services", "contracts", "public-api", "abstractions"],
            IsBuiltIn = true
        },
        new()
        {
            Id = "architecture_specialist",
            Name = "Architecture Specialist",
            Description = "Explains architectural patterns, structure, and dependencies",
            DefaultFocus = "Explain architectural patterns and design decisions",
            Capabilities = ["patterns", "architecture", "dependencies", "structure", "design"],
            IsBuiltIn = true
        },
        new()
        {
            Id = "component_specialist",
            Name = "Component Specialist",
            Description = "Documents Blazor components, parameters, and usage",
            DefaultFocus = "Document Blazor components and their usage",
            Capabilities = ["blazor", "components", "parameters", "razor", "ui"],
            IsBuiltIn = true
        },
        new()
        {
            Id = "generator_specialist",
            Name = "Generator Specialist",
            Description = "Documents Source Generators and generated code",
            DefaultFocus = "Explain source generators and code generation",
            Capabilities = ["generators", "roslyn", "code-generation", "compile-time"],
            IsBuiltIn = true
        },
        new()
        {
            Id = "frontend_specialist",
            Name = "Frontend Specialist",
            Description = "Documents TypeScript, CSS, themes, and styles",
            DefaultFocus = "Document frontend assets and styling",
            Capabilities = ["typescript", "css", "scss", "themes", "styles", "frontend"],
            IsBuiltIn = true
        },
        new()
        {
            Id = "integration_specialist",
            Name = "Integration Specialist",
            Description = "Documents configuration, DI, and extensions",
            DefaultFocus = "Document integration and configuration",
            Capabilities = ["configuration", "di", "extensions", "setup", "integration"],
            IsBuiltIn = true
        }
    ];

    public SpecialistRegistry(string projectPath, ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
        _registryPath = Path.Combine(projectPath, ".doccache", "specialists.json");
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
                    _logger.Verbose($"Loaded {custom.Count} custom specialists");
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to load custom specialists: {ex.Message}");
            }
        }

        return result;
    }

    public IReadOnlyList<SpecialistDefinition> GetAll() => _specialists.Values.ToList();

    public SpecialistDefinition? Get(string id) =>
        _specialists.TryGetValue(id, out SpecialistDefinition? spec) ? spec : null;

    public void Register(SpecialistDefinition specialist)
    {
        if (_specialists.ContainsKey(specialist.Id) && _specialists[specialist.Id].IsBuiltIn)
        {
            _logger.Warning($"Cannot override built-in specialist: {specialist.Id}");
            return;
        }

        _specialists[specialist.Id] = specialist with { IsBuiltIn = false, CreatedAt = DateTime.UtcNow };
        SaveCustomSpecialists();
        _logger.Info($"Registered new specialist: {specialist.Name}");
    }

    private void SaveCustomSpecialists()
    {
        try
        {
            List<SpecialistDefinition> custom = _specialists.Values.Where(s => !s.IsBuiltIn).ToList();

            if (custom.Count == 0)
                return;

            string dir = Path.GetDirectoryName(_registryPath)!;
            Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(custom, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_registryPath, json);
        }
        catch (Exception ex)
        {
            _logger.Warning($"Failed to save specialists: {ex.Message}");
        }
    }

    public string GetSpecialistListForPrompt()
    {
        return string.Join("\n", _specialists.Values.Select(s =>
            $"- {s.Id}: {s.Description} (capabilities: {string.Join(", ", s.Capabilities)})"));
    }
}