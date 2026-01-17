using CdCSharp.Theon.Analysis;
using System.Text;

namespace CdCSharp.Theon.Core;

/// <summary>
/// Plans multi-file output generation based on task requirements.
/// Provides structured guidance for comprehensive deliverables.
/// </summary>
public interface IOutputPlanner
{
    /// <summary>
    /// Creates an output plan for the given query and project.
    /// </summary>
    OutputPlan CreatePlan(string userQuery, ProjectInfo project, TaskType taskType);
}

/// <summary>
/// Plan for generating multiple output files.
/// </summary>
public sealed record OutputPlan(
    PlannedFile[] Files,
    string Rationale,
    string[] GeneralGuidance)
{
    public bool IsMultiFile => Files.Length > 1;
    public bool IsSingleFile => Files.Length == 1;
    public bool HasPlan => Files.Length > 0;
}

/// <summary>
/// Individual file planned for generation.
/// </summary>
public sealed record PlannedFile(
    string Name,
    string Purpose,
    string[] ContentGuidance,
    string[] RequiredExplorations,
    int Priority);

public sealed class OutputPlanner : IOutputPlanner
{
    public OutputPlan CreatePlan(string userQuery, ProjectInfo project, TaskType taskType)
    {
        return taskType switch
        {
            TaskType.Documentation => PlanDocumentation(project),
            TaskType.Architecture => PlanArchitecture(project),
            TaskType.CodeReview => PlanCodeReview(project),
            TaskType.BugAnalysis => PlanBugAnalysis(project),
            TaskType.Refactoring => PlanRefactoring(project),
            TaskType.SpecificQuery => PlanSpecificQuery(userQuery, project),
            _ => PlanGeneral(project)
        };
    }

    private OutputPlan PlanDocumentation(ProjectInfo project)
    {
        List<AssemblyInfo> assemblies = project.Assemblies.Where(a => !a.IsTestProject).ToList();
        List<string> mainFolders = assemblies
            .SelectMany(a => a.Files)
            .Select(f => f.Contains('/') ? f.Split('/')[0] : "")
            .Where(f => !string.IsNullOrEmpty(f) && !f.EndsWith(".cs"))
            .Distinct()
            .ToList();

        List<PlannedFile> files =
        [
            new PlannedFile(
                Name: "README.md",
                Purpose: "Project overview, setup instructions, and getting started guide",
                ContentGuidance:
                [
                    "Project description based on actual purpose",
                    "Key features derived from explored code",
                    "Installation and setup instructions",
                    "Basic usage examples from real API",
                    "Architecture overview linking to detailed docs",
                    "Contributing guidelines",
                    "License information"
                ],
                RequiredExplorations:
                [
                    $"[EXPLORE_ASSEMBLY: name=\"{project.Name}\"]",
                    "[EXPLORE_FILE: path=\"Program.cs\"]"
                ],
                Priority: 1
            ),

            new PlannedFile(
                Name: "ARCHITECTURE.md",
                Purpose: "Detailed system architecture, design patterns, and component relationships",
                ContentGuidance:
                [
                    "System architecture diagram (text-based)",
                    "Component descriptions from actual implementations",
                    "Dependency graph between assemblies/modules",
                    "Design patterns actually used in code",
                    "Layer architecture with real namespaces",
                    "Key abstractions (interfaces, base classes)",
                    "Extension points and plugin architecture"
                ],
                RequiredExplorations: assemblies
                    .Select(a => $"[EXPLORE_ASSEMBLY: name=\"{a.Name}\"]")
                    .Concat(mainFolders.Select(f => $"[EXPLORE_FOLDER: path=\"{f}/\"]"))
                    .ToArray(),
                Priority: 2
            ),

            new PlannedFile(
                Name: "API-REFERENCE.md",
                Purpose: "Public API documentation with interfaces and key types",
                ContentGuidance:
                [
                    "Public interfaces with method signatures",
                    "Key public classes and their responsibilities",
                    "Extension methods and utilities",
                    "Configuration options",
                    "Event system (if applicable)",
                    "Error types and handling"
                ],
                RequiredExplorations: assemblies
                    .SelectMany(a => a.Files)
                    .Where(f => f.Contains("Interface") || f.StartsWith("I") || f.Contains("Public"))
                    .Take(10)
                    .Select(f => $"[EXPLORE_FILE: path=\"{f}\"]")
                    .ToArray(),
                Priority: 3
            ),

            new PlannedFile(
                Name: "DEVELOPMENT.md",
                Purpose: "Developer guide with conventions, patterns, and workflows",
                ContentGuidance:
                [
                    "Project structure explanation",
                    "Coding conventions observed in code",
                    "Development workflow",
                    "Testing strategy (if tests exist)",
                    "Build and deployment process",
                    "Debugging tips",
                    "Common development tasks"
                ],
                RequiredExplorations: mainFolders
                    .Take(5)
                    .Select(f => $"[EXPLORE_FOLDER: path=\"{f}/\"]")
                    .ToArray(),
                Priority: 4
            )
        ];

        // Add component-specific documentation if there are distinct modules
        if (mainFolders.Count >= 3)
        {
            files.Add(new PlannedFile(
                Name: "COMPONENTS.md",
                Purpose: "Detailed documentation for each major component/module",
                ContentGuidance:
                [
                    "Per-component breakdown",
                    "Component responsibilities",
                    "Internal structure",
                    "Public APIs per component",
                    "Usage patterns and examples"
                ],
                RequiredExplorations: mainFolders
                    .Select(f => $"[EXPLORE_FOLDER: path=\"{f}/\"]")
                    .ToArray(),
                Priority: 5
            ));
        }

        return new OutputPlan(
            Files: files.ToArray(),
            Rationale: $"""
                Comprehensive documentation suite for {project.Name}
                
                This plan generates {files.Count} documentation files covering:
                - User-facing documentation (README)
                - Technical architecture (ARCHITECTURE)
                - API reference (API-REFERENCE)
                - Developer guide (DEVELOPMENT)
                {(files.Count > 4 ? "- Component details (COMPONENTS)" : "")}
                
                Each file serves a specific audience and purpose, ensuring
                complete coverage of the project from multiple perspectives.
                """,
            GeneralGuidance:
            [
                "Explore systematically: assembly → folders → key files",
                "Base ALL content on ACTUAL explored code",
                "Use REAL class names, namespaces, and types",
                "Include CONCRETE code examples from the project",
                "Cross-reference between documentation files",
                "Maintain consistency across all generated files",
                "Ensure examples are executable and accurate"
            ]
        );
    }

    private OutputPlan PlanArchitecture(ProjectInfo project)
    {
        List<AssemblyInfo> assemblies = project.Assemblies.Where(a => !a.IsTestProject).ToList();

        List<PlannedFile> files =
        [
            new PlannedFile(
                Name: "ARCHITECTURE-OVERVIEW.md",
                Purpose: "High-level architecture and design philosophy",
                ContentGuidance:
                [
                    "Architecture style (layered, hexagonal, etc.)",
                    "Core design principles",
                    "System boundaries",
                    "Key architectural decisions",
                    "Technology stack"
                ],
                RequiredExplorations: assemblies
                    .Select(a => $"[EXPLORE_ASSEMBLY: name=\"{a.Name}\"]")
                    .ToArray(),
                Priority: 1
            ),

            new PlannedFile(
                Name: "COMPONENTS-AND-LAYERS.md",
                Purpose: "Detailed component architecture and layer organization",
                ContentGuidance:
                [
                    "Component diagram with dependencies",
                    "Layer structure (if applicable)",
                    "Module responsibilities",
                    "Component interactions",
                    "Dependency rules"
                ],
                RequiredExplorations: assemblies
                    .SelectMany(a => a.Files)
                    .GroupBy(f => f.Contains('/') ? f.Split('/')[0] : f)
                    .Select(g => g.Key)
                    .Where(folder => !folder.EndsWith(".cs"))
                    .Select(folder => $"[EXPLORE_FOLDER: path=\"{folder}/\"]")
                    .ToArray(),
                Priority: 2
            ),

            new PlannedFile(
                Name: "DESIGN-PATTERNS.md",
                Purpose: "Catalog of design patterns actually used in the codebase",
                ContentGuidance:
                [
                    "Identified patterns with evidence",
                    "Pattern usage locations",
                    "Benefits and trade-offs",
                    "Implementation examples from code"
                ],
                RequiredExplorations: assemblies
                    .SelectMany(a => a.Files)
                    .Where(f => f.Contains("Interface") || f.Contains("Factory") ||
                               f.Contains("Strategy") || f.Contains("Repository"))
                    .Select(f => $"[EXPLORE_FILE: path=\"{f}\"]")
                    .ToArray(),
                Priority: 3
            )
        ];

        return new OutputPlan(
            Files: files.ToArray(),
            Rationale: $"""
                Architecture documentation for {project.Name}
                
                Provides comprehensive architectural documentation including:
                - High-level overview and principles
                - Component and layer structure
                - Design pattern catalog
                
                Based on thorough exploration of assemblies, modules, and key abstractions.
                """,
            GeneralGuidance:
            [
                "Identify ACTUAL patterns, not assumed ones",
                "Create diagrams based on REAL dependencies",
                "Verify all components and relationships exist",
                "Use specific examples from explored code",
                "Maintain technical accuracy throughout"
            ]
        );
    }

    private OutputPlan PlanCodeReview(ProjectInfo project)
    {
        return new OutputPlan(
            Files:
            [
                new PlannedFile(
                    Name: "CODE-REVIEW-REPORT.md",
                    Purpose: "Comprehensive code review with findings and recommendations",
                    ContentGuidance:
                    [
                        "Executive summary",
                        "Critical issues with specific locations",
                        "Major improvements needed",
                        "Minor suggestions",
                        "Positive patterns observed",
                        "Prioritized action items"
                    ],
                    RequiredExplorations: [],
                    Priority: 1
                )
            ],
            Rationale: "Single comprehensive review report with categorized findings",
            GeneralGuidance:
            [
                "Explore target scope thoroughly",
                "Reference specific files and lines",
                "Provide actionable recommendations",
                "Include code examples for improvements",
                "Prioritize by impact"
            ]
        );
    }

    private OutputPlan PlanBugAnalysis(ProjectInfo project)
    {
        return new OutputPlan(
            Files:
            [
                new PlannedFile(
                    Name: "BUG-ANALYSIS.md",
                    Purpose: "Root cause analysis and fix recommendation",
                    ContentGuidance:
                    [
                        "Issue description",
                        "Root cause analysis",
                        "Affected components",
                        "Recommended fix with code",
                        "Testing strategy",
                        "Prevention measures"
                    ],
                    RequiredExplorations: [],
                    Priority: 1
                )
            ],
            Rationale: "Focused bug analysis with diagnosis and solution",
            GeneralGuidance:
            [
                "Trace execution flow",
                "Identify root cause, not just symptoms",
                "Provide specific fix with code",
                "Consider side effects",
                "Suggest tests to prevent regression"
            ]
        );
    }

    private OutputPlan PlanRefactoring(ProjectInfo project)
    {
        return new OutputPlan(
            Files:
            [
                new PlannedFile(
                    Name: "REFACTORING-PLAN.md",
                    Purpose: "Detailed refactoring plan with before/after analysis",
                    ContentGuidance:
                    [
                        "Current state analysis",
                        "Identified issues",
                        "Proposed structure",
                        "Before/after comparison",
                        "Migration steps",
                        "Risk assessment",
                        "Testing strategy"
                    ],
                    RequiredExplorations: [],
                    Priority: 1
                ),

                new PlannedFile(
                    Name: "REFACTORED-CODE.md",
                    Purpose: "Generated refactored code (if appropriate)",
                    ContentGuidance:
                    [
                        "Refactored implementations",
                        "Explanation of changes",
                        "Migration notes"
                    ],
                    RequiredExplorations: [],
                    Priority: 2
                )
            ],
            Rationale: "Complete refactoring documentation with plan and implementation",
            GeneralGuidance:
            [
                "Preserve functionality",
                "Improve structure incrementally",
                "Provide clear migration path",
                "Minimize breaking changes",
                "Include rollback strategy"
            ]
        );
    }

    private OutputPlan PlanSpecificQuery(string query, ProjectInfo project)
    {
        return new OutputPlan(
            Files:
            [
                new PlannedFile(
                    Name: "ANSWER.md",
                    Purpose: "Focused answer to specific query",
                    ContentGuidance:
                    [
                        "Direct answer to the question",
                        "Supporting code references",
                        "Related context",
                        "Examples if helpful"
                    ],
                    RequiredExplorations: [],
                    Priority: 1
                )
            ],
            Rationale: "Targeted response to specific question",
            GeneralGuidance:
            [
                "Answer directly and precisely",
                "Reference actual code",
                "Provide necessary context",
                "Be concise but complete"
            ]
        );
    }

    private OutputPlan PlanGeneral(ProjectInfo project)
    {
        return new OutputPlan(
            Files: [],
            Rationale: "No specific output plan. Generate response based on query requirements.",
            GeneralGuidance:
            [
                "Understand the request",
                "Explore relevant code",
                "Generate appropriate output",
                "Use real code references"
            ]
        );
    }

    public static string FormatPlanForPrompt(OutputPlan plan)
    {
        if (!plan.HasPlan)
            return "";

        StringBuilder sb = new();

        sb.AppendLine("# OUTPUT GENERATION PLAN");
        sb.AppendLine();
        sb.AppendLine(plan.Rationale);
        sb.AppendLine();

        if (plan.IsMultiFile)
        {
            sb.AppendLine($"## {plan.Files.Length} Files to Generate");
            sb.AppendLine();

            foreach (PlannedFile? file in plan.Files.OrderBy(f => f.Priority))
            {
                sb.AppendLine($"### {file.Priority}. {file.Name}");
                sb.AppendLine();
                sb.AppendLine($"**Purpose:** {file.Purpose}");
                sb.AppendLine();

                if (file.ContentGuidance.Length > 0)
                {
                    sb.AppendLine("**Content Guidelines:**");
                    foreach (string guidance in file.ContentGuidance)
                    {
                        sb.AppendLine($"- {guidance}");
                    }
                    sb.AppendLine();
                }

                if (file.RequiredExplorations.Length > 0)
                {
                    sb.AppendLine("**Required Explorations:**");
                    foreach (string? exploration in file.RequiredExplorations.Take(3))
                    {
                        sb.AppendLine($"- {exploration}");
                    }
                    if (file.RequiredExplorations.Length > 3)
                    {
                        sb.AppendLine($"- ... and {file.RequiredExplorations.Length - 3} more");
                    }
                    sb.AppendLine();
                }
            }
        }

        if (plan.GeneralGuidance.Length > 0)
        {
            sb.AppendLine("## General Guidance");
            sb.AppendLine();
            foreach (string guidance in plan.GeneralGuidance)
            {
                sb.AppendLine($"- {guidance}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("Follow this plan systematically:");
        sb.AppendLine("1. Perform required explorations");
        sb.AppendLine("2. Generate files in priority order");
        sb.AppendLine("3. Ensure consistency across all files");
        sb.AppendLine("4. Base all content on explored code");

        return sb.ToString();
    }
}