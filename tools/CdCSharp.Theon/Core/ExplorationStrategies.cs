using CdCSharp.Theon.Analysis;

namespace CdCSharp.Theon.Core;

/// <summary>
/// Provides context-aware exploration strategies based on task classification.
/// Guides the LLM on what to explore and how to approach different types of queries.
/// </summary>
public interface IExplorationStrategies
{
    /// <summary>
    /// Classifies the user query and returns appropriate exploration guidance.
    /// </summary>
    ExplorationStrategy GetStrategy(string userQuery, ProjectInfo project);
}

/// <summary>
/// Exploration strategy with guidance and recommendations.
/// </summary>
public sealed record ExplorationStrategy(
    TaskType Type,
    string Guidance,
    int MinimumExplorations,
    float TargetConfidence,
    string[] RecommendedExplorations,
    string[] QualityCriteria);

public enum TaskType
{
    Documentation,
    Architecture,
    CodeReview,
    BugAnalysis,
    Refactoring,
    SpecificQuery,
    General
}

public sealed class ExplorationStrategies : IExplorationStrategies
{
    public ExplorationStrategy GetStrategy(string userQuery, ProjectInfo project)
    {
        TaskType taskType = ClassifyTask(userQuery);

        return taskType switch
        {
            TaskType.Documentation => DocumentationStrategy(project),
            TaskType.Architecture => ArchitectureStrategy(project),
            TaskType.CodeReview => CodeReviewStrategy(project),
            TaskType.BugAnalysis => BugAnalysisStrategy(project),
            TaskType.Refactoring => RefactoringStrategy(project),
            TaskType.SpecificQuery => SpecificQueryStrategy(userQuery, project),
            _ => GeneralStrategy(project)
        };
    }

    private TaskType ClassifyTask(string query)
    {
        string normalized = query.ToLowerInvariant();

        // Documentation indicators
        if (ContainsAny(normalized, "document", "documenta", "explain", "explica", "describe"))
            return TaskType.Documentation;

        // Architecture indicators
        if (ContainsAny(normalized, "architecture", "arquitectura", "design", "diseño", "structure", "estructura"))
            return TaskType.Architecture;

        // Code review indicators
        if (ContainsAny(normalized, "review", "revisar", "analyze", "analizar", "check", "verificar", "improve", "mejorar"))
            return TaskType.CodeReview;

        // Bug analysis indicators
        if (ContainsAny(normalized, "bug", "error", "issue", "problema", "not working", "no funciona", "fix", "arreglar"))
            return TaskType.BugAnalysis;

        // Refactoring indicators
        if (ContainsAny(normalized, "refactor", "refactorizar", "reorganize", "reorganizar", "optimize", "optimizar"))
            return TaskType.Refactoring;

        // Specific query indicators (mentions specific files/classes)
        if (ContainsAny(normalized, "how does", "cómo funciona", "what is", "qué es", "where is", "dónde está"))
            return TaskType.SpecificQuery;

        return TaskType.General;
    }

    private ExplorationStrategy DocumentationStrategy(ProjectInfo project)
    {
        List<AssemblyInfo> assemblies = project.Assemblies.Where(a => !a.IsTestProject).ToList();

        List<string> recommendedExplorations =
        [
            $"[EXPLORE_ASSEMBLY: name=\"{project.Name}\"]"
        ];

        // Add explorations for main folders
        IEnumerable<string> mainFolders = assemblies
            .SelectMany(a => a.Files)
            .Select(f => f.Split('/')[0])
            .Distinct()
            .Where(folder => !folder.EndsWith(".cs"))
            .Take(4);

        recommendedExplorations.AddRange(
            mainFolders.Select(folder => $"[EXPLORE_FOLDER: path=\"{folder}/\"]")
        );

        return new ExplorationStrategy(
            TaskType.Documentation,
            """
            # DOCUMENTATION TASK DETECTED
            
            ## Objective
            Create comprehensive, accurate documentation based on actual code structure and implementation.
            
            ## Exploration Sequence
            1. Start with assembly exploration to understand overall structure
            2. Explore main module folders to grasp organization
            3. Read key implementation files to understand patterns
            4. Continue exploring until you have a complete picture
            
            ## Quality Requirements
            - Use REAL class names, interfaces, and types from the code
            - Reference ACTUAL design patterns you observe
            - Include SPECIFIC file paths and namespaces
            - Explain REAL dependencies and relationships
            - Provide code examples from ACTUAL implementations
            
            ## Expected Output
            Generate multiple documentation files:
            - README.md: Project overview, setup, basic usage
            - ARCHITECTURE.md: System design, components, patterns
            - API-REFERENCE.md: Public interfaces and key types
            - DEVELOPMENT.md: Developer guide, conventions, workflows
            
            ## Success Criteria
            - All mentioned types/classes exist in the codebase
            - Design patterns match actual implementation
            - Examples are from real code
            - Architecture diagrams reflect actual dependencies
            """,
            MinimumExplorations: 5,
            TargetConfidence: 0.80f,
            RecommendedExplorations: recommendedExplorations.ToArray(),
            QualityCriteria:
            [
                "References specific types from explored code",
                "Explains actual design patterns observed",
                "Includes real file paths and namespaces",
                "Provides concrete code examples",
                "Describes genuine dependencies"
            ]
        );
    }

    private ExplorationStrategy ArchitectureStrategy(ProjectInfo project)
    {
        List<AssemblyInfo> assemblies = project.Assemblies.Where(a => !a.IsTestProject).ToList();

        List<string> recommendedExplorations = assemblies
            .Select(a => $"[EXPLORE_ASSEMBLY: name=\"{a.Name}\"]")
            .ToList();

        // Add interface files exploration
        IEnumerable<string> interfaceFiles = assemblies
            .SelectMany(a => a.Files)
            .Where(f => f.Contains("Interface") || f.StartsWith("I"))
            .Take(5);

        recommendedExplorations.AddRange(
            interfaceFiles.Select(f => $"[EXPLORE_FILE: path=\"{f}\"]")
        );

        return new ExplorationStrategy(
            TaskType.Architecture,
            """
            # ARCHITECTURE ANALYSIS TASK
            
            ## Objective
            Document the system architecture based on actual structure, dependencies, and design patterns.
            
            ## Analysis Approach
            1. Explore all assemblies to understand component boundaries
            2. Examine interface definitions to identify abstractions
            3. Read key implementation files to understand patterns
            4. Analyze dependencies between components
            5. Identify layers, modules, and cross-cutting concerns
            
            ## Focus Areas
            - **Dependency Flow**: How components depend on each other
            - **Abstraction Layers**: Interfaces, base classes, contracts
            - **Design Patterns**: Strategy, Factory, Repository, etc. (ACTUAL usage)
            - **Module Organization**: How code is structured by responsibility
            - **Extension Points**: Where the system is designed to be extended
            
            ## Expected Output
            - Component diagram with real dependencies
            - Layer architecture with actual namespaces
            - Design pattern catalog (only patterns actually used)
            - Dependency graph
            - Extension point documentation
            
            ## Quality Standards
            - All components in diagrams exist in code
            - Dependencies match actual references
            - Patterns identified are truly implemented
            - Namespaces and types are accurate
            """,
            MinimumExplorations: 6,
            TargetConfidence: 0.85f,
            RecommendedExplorations: recommendedExplorations.ToArray(),
            QualityCriteria:
            [
                "Accurate component boundaries",
                "Real dependency relationships",
                "Verified design patterns",
                "Concrete abstraction examples",
                "Actual extension mechanisms"
            ]
        );
    }

    private ExplorationStrategy CodeReviewStrategy(ProjectInfo project)
    {
        return new ExplorationStrategy(
            TaskType.CodeReview,
            """
            # CODE REVIEW TASK
            
            ## Objective
            Analyze code quality, identify issues, and suggest improvements based on actual implementation.
            
            ## Review Process
            1. Determine scope (specific file, folder, or entire project)
            2. Explore target files systematically
            3. Analyze for:
               - Code quality issues
               - Design pattern violations
               - Performance concerns
               - Maintainability problems
               - Security vulnerabilities
               - Best practice deviations
            
            ## Review Criteria
            - **Code Quality**: Readability, naming, complexity
            - **Design**: SOLID principles, coupling, cohesion
            - **Patterns**: Correct usage, anti-patterns
            - **Performance**: Efficiency, resource usage
            - **Security**: Input validation, error handling
            - **Testability**: Dependencies, isolation
            
            ## Expected Output
            Detailed review report with:
            - Specific file and line references
            - Issue categorization (critical, major, minor)
            - Concrete improvement suggestions
            - Example refactorings (if applicable)
            - Priority recommendations
            
            ## Quality Standards
            - All issues reference actual code locations
            - Suggestions are actionable and specific
            - Examples demonstrate real improvements
            - Prioritization reflects actual impact
            """,
            MinimumExplorations: 4,
            TargetConfidence: 0.75f,
            RecommendedExplorations: ["[EXPLORE_FOLDER: path=\"target/\"]"],
            QualityCriteria:
            [
                "Specific line/file references",
                "Concrete issue descriptions",
                "Actionable recommendations",
                "Real code examples",
                "Impact assessment"
            ]
        );
    }

    private ExplorationStrategy BugAnalysisStrategy(ProjectInfo project)
    {
        return new ExplorationStrategy(
            TaskType.BugAnalysis,
            """
            # BUG ANALYSIS TASK
            
            ## Objective
            Diagnose and identify root cause of reported issue through systematic code exploration.
            
            ## Investigation Process
            1. Explore the reported file/component
            2. Examine related files and dependencies
            3. Trace execution flow
            4. Identify potential root causes
            5. Verify hypothesis through additional exploration
            
            ## Analysis Focus
            - **Direct Cause**: What code is failing?
            - **Root Cause**: Why is it failing?
            - **Impact**: What else might be affected?
            - **Fix Approach**: How to resolve correctly?
            
            ## Expected Output
            Diagnosis report with:
            - Root cause analysis
            - Affected components
            - Recommended fix
            - Test suggestions
            - Potential side effects
            
            ## Quality Standards
            - Root cause based on actual code
            - Fix recommendation is specific
            - Impact analysis is thorough
            - References real implementations
            """,
            MinimumExplorations: 3,
            TargetConfidence: 0.70f,
            RecommendedExplorations: [],
            QualityCriteria:
            [
                "Clear root cause identification",
                "Evidence from actual code",
                "Specific fix recommendation",
                "Impact assessment",
                "Testing guidance"
            ]
        );
    }

    private ExplorationStrategy RefactoringStrategy(ProjectInfo project)
    {
        return new ExplorationStrategy(
            TaskType.Refactoring,
            """
            # REFACTORING TASK
            
            ## Objective
            Improve code structure while preserving functionality, based on actual implementation.
            
            ## Refactoring Process
            1. Explore current implementation thoroughly
            2. Identify structural issues
            3. Design improved structure
            4. Plan migration path
            5. Generate refactored code
            
            ## Refactoring Targets
            - **Extract**: Methods, classes, interfaces
            - **Rename**: For clarity and consistency
            - **Move**: Better organization
            - **Simplify**: Reduce complexity
            - **Pattern Application**: Appropriate design patterns
            
            ## Expected Output
            - Current state analysis
            - Proposed refactoring
            - Before/after comparison
            - Migration steps
            - Risk assessment
            
            ## Quality Standards
            - Preserves existing functionality
            - Improves maintainability
            - Follows established patterns
            - Minimizes breaking changes
            """,
            MinimumExplorations: 4,
            TargetConfidence: 0.75f,
            RecommendedExplorations: [],
            QualityCriteria:
            [
                "Functionality preservation",
                "Structural improvement",
                "Clear migration path",
                "Minimal disruption",
                "Pattern consistency"
            ]
        );
    }

    private ExplorationStrategy SpecificQueryStrategy(string query, ProjectInfo project)
    {
        // Try to extract file/class names from query
        List<string> potentialFiles = project.AllFiles
            .Where(f => query.Contains(Path.GetFileNameWithoutExtension(f), StringComparison.OrdinalIgnoreCase))
            .Take(3)
            .ToList();

        string[] recommendations = potentialFiles.Any()
            ? potentialFiles.Select(f => $"[EXPLORE_FILE: path=\"{f}\"]").ToArray()
            : new[] { "[EXPLORE_ASSEMBLY: ...]" };

        return new ExplorationStrategy(
            TaskType.SpecificQuery,
            """
            # SPECIFIC QUERY TASK
            
            ## Objective
            Answer a targeted question about the codebase with precision.
            
            ## Approach
            1. Identify relevant files/components
            2. Explore specific implementations
            3. Provide focused, accurate answer
            4. Include code references
            
            ## Expected Output
            - Direct answer to the question
            - Supporting code references
            - Related context if helpful
            - Confidence based on exploration
            
            ## Quality Standards
            - Answer is specific and accurate
            - References actual code
            - Provides necessary context
            - Admits uncertainty if needed
            """,
            MinimumExplorations: 2,
            TargetConfidence: 0.70f,
            RecommendedExplorations: recommendations,
            QualityCriteria:
            [
                "Direct answer",
                "Specific code references",
                "Accurate information",
                "Appropriate context"
            ]
        );
    }

    private ExplorationStrategy GeneralStrategy(ProjectInfo project)
    {
        return new ExplorationStrategy(
            TaskType.General,
            """
            # GENERAL TASK
            
            ## Approach
            - Understand the request
            - Explore relevant code
            - Provide accurate response
            - Base confidence on exploration
            
            ## Quality Standards
            - Use real code references
            - Avoid generic statements
            - Honest confidence assessment
            """,
            MinimumExplorations: 1,
            TargetConfidence: 0.60f,
            RecommendedExplorations: [$"[EXPLORE_ASSEMBLY: name=\"{project.Name}\"]"],
            QualityCriteria:
            [
                "Accurate information",
                "Real code references",
                "Honest assessment"
            ]
        );
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}