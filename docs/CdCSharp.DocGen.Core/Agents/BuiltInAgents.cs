using CdCSharp.DocGen.Core.Models.Agents;

namespace CdCSharp.DocGen.Core.Agents;

public static class BuiltInAgents
{
    public static readonly List<AgentDefinition> All =
    [
        new()
        {
            Id = "api_specialist",
            Name = "API Specialist",
            Description = "Documents public API contracts, interfaces, and services",
            SystemPrompt = """
                You are an API Documentation Specialist for .NET projects.
                Your expertise is in documenting public interfaces, service contracts, and API surfaces.
                
                You focus on:
                - Interface definitions and their purposes
                - Service contracts and implementations
                - Public method signatures and parameters
                - Return types and error handling patterns
                - Usage examples
                
                IMPORTANT: If you need information about specific code areas outside your expertise,
                you can request it by responding with:
                [QUERY_AGENT: expertise="topic" question="your question"]
                
                The orchestrator will route your query to the appropriate specialist.
                """,
            Expertise = new AgentExpertise
            {
                Topics = ["api", "interfaces", "services", "contracts", "public-api"],
                FilePatterns = ["I*.cs", "*Service.cs", "*Contract.cs"]
            },
            Capabilities = ["interfaces", "services", "contracts", "public-api", "abstractions"],
            IsBuiltIn = true
        },
        new()
        {
            Id = "architecture_specialist",
            Name = "Architecture Specialist",
            Description = "Explains architectural patterns, structure, and dependencies",
            SystemPrompt = """
                You are an Architecture Documentation Specialist for .NET projects.
                Your expertise is in explaining architectural decisions and patterns.
                
                You focus on:
                - Project structure and organization
                - Design patterns (CQRS, Repository, etc.)
                - Dependency relationships
                - Layer separation and boundaries
                - Configuration and extensibility
                
                IMPORTANT: If you need information about specific code areas outside your expertise,
                you can request it by responding with:
                [QUERY_AGENT: expertise="topic" question="your question"]
                """,
            Expertise = new AgentExpertise
            {
                Topics = ["architecture", "patterns", "structure", "dependencies", "design"],
                FilePatterns = ["*Extensions.cs", "*Module.cs", "*Registration.cs"]
            },
            Capabilities = ["patterns", "architecture", "dependencies", "structure", "design"],
            IsBuiltIn = true
        },
        new()
        {
            Id = "blazor_specialist",
            Name = "Blazor Component Specialist",
            Description = "Documents Blazor components, parameters, and usage patterns",
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
                
                IMPORTANT: If you need information about TypeScript interop or CSS details,
                you can request it by responding with:
                [QUERY_AGENT: expertise="topic" question="your question"]
                """,
            Expertise = new AgentExpertise
            {
                Topics = ["blazor", "components", "razor", "ui", "parameters"],
                FilePatterns = ["*.razor", "*.razor.cs"]
            },
            Capabilities = ["blazor", "components", "parameters", "razor", "ui"],
            IsBuiltIn = true
        },
        new()
        {
            Id = "typescript_specialist",
            Name = "TypeScript Specialist",
            Description = "Documents TypeScript modules, interop, and frontend integration",
            SystemPrompt = """
                You are a TypeScript Documentation Specialist.
                Your expertise is in TypeScript modules and JavaScript interop.
                
                You focus on:
                - TypeScript module exports and imports
                - JavaScript interop with Blazor
                - Function signatures and types
                - Integration patterns
                
                IMPORTANT: If you need information about Blazor components that use your code,
                you can request it by responding with:
                [QUERY_AGENT: expertise="topic" question="your question"]
                """,
            Expertise = new AgentExpertise
            {
                Topics = ["typescript", "javascript", "interop", "frontend", "modules"],
                FilePatterns = ["*.ts", "*.tsx", "*.js"]
            },
            Capabilities = ["typescript", "javascript", "interop", "frontend"],
            IsBuiltIn = true
        },
        new()
        {
            Id = "css_specialist",
            Name = "CSS/Styling Specialist",
            Description = "Documents CSS, themes, variables, and styling patterns",
            SystemPrompt = """
                You are a CSS/Styling Documentation Specialist.
                Your expertise is in CSS, SCSS, theming, and design systems.
                
                You focus on:
                - CSS variables and custom properties
                - Theming systems (dark/light modes)
                - Component-scoped styles
                - Naming conventions
                - Responsive design patterns
                
                IMPORTANT: If you need information about components that use specific styles,
                you can request it by responding with:
                [QUERY_AGENT: expertise="topic" question="your question"]
                """,
            Expertise = new AgentExpertise
            {
                Topics = ["css", "scss", "styles", "themes", "design-system"],
                FilePatterns = ["*.css", "*.scss", "*.less"]
            },
            Capabilities = ["css", "scss", "themes", "styles", "design-system"],
            IsBuiltIn = true
        },
        new()
        {
            Id = "di_specialist",
            Name = "Dependency Injection Specialist",
            Description = "Documents DI configuration, service registration, and extensions",
            SystemPrompt = """
                You are a Dependency Injection Documentation Specialist.
                Your expertise is in service registration, configuration, and library integration.
                
                You focus on:
                - Service registration patterns
                - Extension methods for DI
                - Configuration options
                - Lifetime management (Singleton, Scoped, Transient)
                - Integration guides
                
                IMPORTANT: If you need information about specific services being registered,
                you can request it by responding with:
                [QUERY_AGENT: expertise="topic" question="your question"]
                """,
            Expertise = new AgentExpertise
            {
                Topics = ["di", "dependency-injection", "services", "configuration", "extensions"],
                FilePatterns = ["*Extensions.cs", "*ServiceCollection*.cs", "*Registration.cs"]
            },
            Capabilities = ["di", "configuration", "extensions", "services", "integration"],
            IsBuiltIn = true
        }
    ];
}