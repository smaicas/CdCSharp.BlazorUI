using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using System.Text;

namespace CdCSharp.Theon.Orchestration;

public interface IOrchestrator
{
    Task<OrchestratorResponse> ProcessAsync(string userInput, CancellationToken ct = default);
}

public sealed class Orchestrator : IOrchestrator
{
    private readonly TheonOptions _options;
    private readonly IProjectAnalysis _analysis;
    private readonly ILlmClient _llmClient;
    private readonly IToolParser _toolParser;
    private readonly IScopeFactory _scopeFactory;
    private readonly IFileSystem _fileSystem;
    private readonly ITheonLogger _logger;

    private readonly List<LlmMessage> _conversationHistory = [];
    private int _responseCounter;
    private const float LowConfidenceThreshold = 0.5f;

    public Orchestrator(
        TheonOptions options,
        IProjectAnalysis analysis,
        ILlmClient llmClient,
        IToolParser toolParser,
        IScopeFactory scopeFactory,
        IFileSystem fileSystem,
        ITheonLogger logger)
    {
        _options = options;
        _analysis = analysis;
        _llmClient = llmClient;
        _toolParser = toolParser;
        _scopeFactory = scopeFactory;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<OrchestratorResponse> ProcessAsync(string userInput, CancellationToken ct = default)
    {
        if (_analysis.Project == null)
            throw new InvalidOperationException("Project not analyzed");

        (string? scopeOverride, string query) = ParseUserInput(userInput);

        if (_conversationHistory.Count == 0)
        {
            InitializeConversation();
        }

        _conversationHistory.Add(LlmMessage.User(query));

        List<GeneratedFile> outputFiles = [];
        List<string> modifiedProjectFiles = [];
        string responseFolder = CreateResponseFolder();

        int depth = 0;
        float confidence = 0f;
        string finalContent = "";

        while (depth < _options.MaxExplorationDepth)
        {
            depth++;

            LlmResponse response = await _llmClient.SendAsync(_conversationHistory, ct);
            ParseResult parsed = _toolParser.Parse(response.Content);

            finalContent = parsed.CleanContent;
            confidence = parsed.Confidence;

            _conversationHistory.Add(LlmMessage.Assistant(response.Content));

            List<ToolInvocation> explorations = parsed.Tools.Where(IsExplorationTool).ToList();
            List<ToolInvocation> outputs = parsed.Tools.Where(IsOutputTool).ToList();
            List<ToolInvocation> modifications = parsed.Tools.Where(IsModificationTool).ToList();

            foreach (ToolInvocation tool in outputs)
            {
                GeneratedFile? file = await ProcessOutputToolAsync(tool, responseFolder, outputFiles, ct);
                if (file != null)
                    outputFiles.Add(file);
            }

            foreach (ToolInvocation tool in modifications)
            {
                string? modified = await ProcessModificationToolAsync(tool, ct);
                if (modified != null)
                    modifiedProjectFiles.Add(modified);
            }

            if (explorations.Count == 0 && !parsed.NeedsMoreContext)
                break;

            StringBuilder additionalContext = new();
            foreach (ToolInvocation tool in explorations)
            {
                string? context = await ProcessExplorationToolAsync(tool, ct);
                if (context != null)
                {
                    additionalContext.AppendLine(context);
                    additionalContext.AppendLine();
                }
            }

            if (additionalContext.Length > 0)
            {
                _conversationHistory.Add(LlmMessage.User(Prompts.ContinueWithContext(additionalContext.ToString())));
            }
        }

        if (confidence is > 0 and < LowConfidenceThreshold)
        {
            _logger.Info($"Low confidence ({confidence:P0}), requesting self-review...");
            _conversationHistory.Add(LlmMessage.User(Prompts.SelfReview(finalContent, confidence)));

            LlmResponse reviewResponse = await _llmClient.SendAsync(_conversationHistory, ct);
            ParseResult reviewParsed = _toolParser.Parse(reviewResponse.Content);

            if (reviewParsed.Confidence > confidence)
            {
                finalContent = reviewParsed.CleanContent;
                confidence = reviewParsed.Confidence;
                _conversationHistory.Add(LlmMessage.Assistant(reviewResponse.Content));
            }
        }

        await SaveResponseMarkdownAsync(responseFolder, query, finalContent, outputFiles, modifiedProjectFiles, confidence, ct);

        _logger.Info($"Response saved to: {responseFolder}");

        return new OrchestratorResponse(finalContent, outputFiles, modifiedProjectFiles, confidence);
    }

    private void InitializeConversation()
    {
        ProjectScope projectScope = _scopeFactory.CreateProjectScope();
        string systemPrompt = Prompts.SystemPrompt(_analysis.Project!, _options.Modification.Enabled);
        string projectOverview = Prompts.ProjectOverview(_analysis.Project!);

        _conversationHistory.Add(LlmMessage.System(systemPrompt));
        _conversationHistory.Add(LlmMessage.User($"Project structure:\n\n{projectOverview}"));
        _conversationHistory.Add(LlmMessage.Assistant("I understand the project structure. Ready to help."));
    }

    private static (string? ScopeOverride, string Query) ParseUserInput(string input)
    {
        if (input.StartsWith("@assembly:", StringComparison.OrdinalIgnoreCase))
        {
            int spaceIdx = input.IndexOf(' ');
            if (spaceIdx > 0)
                return (input[10..spaceIdx], input[(spaceIdx + 1)..]);
        }

        if (input.StartsWith("@file:", StringComparison.OrdinalIgnoreCase))
        {
            int spaceIdx = input.IndexOf(' ');
            if (spaceIdx > 0)
                return (input[6..spaceIdx], input[(spaceIdx + 1)..]);
        }

        if (input.StartsWith("@folder:", StringComparison.OrdinalIgnoreCase))
        {
            int spaceIdx = input.IndexOf(' ');
            if (spaceIdx > 0)
                return (input[8..spaceIdx], input[(spaceIdx + 1)..]);
        }

        return (null, input);
    }

    private async Task<string?> ProcessExplorationToolAsync(ToolInvocation tool, CancellationToken ct)
    {
        return tool switch
        {
            ExploreAssemblyTool t => await ExploreAssemblyAsync(t.Name, ct),
            ExploreFileTool t => await ExploreFileAsync(t.Path, ct),
            ExploreFolderTool t => await ExploreFolderAsync(t.Path, ct),
            ExploreFilesTool t => await ExploreFilesAsync(t.Paths, ct),
            _ => null
        };
    }

    private async Task<string?> ExploreAssemblyAsync(string name, CancellationToken ct)
    {
        AssemblyScope? scope = await _scopeFactory.CreateAssemblyScopeAsync(name, ct);
        return scope?.BuildContext();
    }

    private async Task<string?> ExploreFileAsync(string path, CancellationToken ct)
    {
        FileScope? scope = await _scopeFactory.CreateFileScopeAsync(path, ct);
        return scope?.BuildContext();
    }

    private async Task<string?> ExploreFolderAsync(string path, CancellationToken ct)
    {
        FolderScope? scope = await _scopeFactory.CreateFolderScopeAsync(path, ct);
        return scope?.BuildContext();
    }

    private async Task<string?> ExploreFilesAsync(IReadOnlyList<string> paths, CancellationToken ct)
    {
        MultiFileScope? scope = await _scopeFactory.CreateMultiFileScopeAsync(paths, ct);
        return scope?.BuildContext();
    }

    private async Task<GeneratedFile?> ProcessOutputToolAsync(
        ToolInvocation tool,
        string responseFolder,
        List<GeneratedFile> existingFiles,
        CancellationToken ct)
    {
        return tool switch
        {
            GenerateFileTool t => await HandleGenerateFileAsync(t, responseFolder, ct),
            AppendFileTool t => await HandleAppendFileAsync(t, responseFolder, existingFiles, ct),
            OverwriteFileTool t => await HandleOverwriteFileAsync(t, responseFolder, ct),
            _ => null
        };
    }

    private async Task<GeneratedFile> HandleGenerateFileAsync(GenerateFileTool tool, string folder, CancellationToken ct)
    {
        await _fileSystem.WriteOutputFileAsync(folder, tool.Name, tool.Content, ct);
        return new GeneratedFile(tool.Name, tool.Content);
    }

    private async Task<GeneratedFile?> HandleAppendFileAsync(AppendFileTool tool, string folder, List<GeneratedFile> existing, CancellationToken ct)
    {
        GeneratedFile? existingFile = existing.FirstOrDefault(f => f.Name == tool.Name);
        string newContent = existingFile != null
            ? existingFile.Content + "\n" + tool.Content
            : tool.Content;

        await _fileSystem.WriteOutputFileAsync(folder, tool.Name, newContent, ct);

        if (existingFile != null)
            existing.Remove(existingFile);

        return new GeneratedFile(tool.Name, newContent);
    }

    private async Task<GeneratedFile> HandleOverwriteFileAsync(OverwriteFileTool tool, string folder, CancellationToken ct)
    {
        await _fileSystem.WriteOutputFileAsync(folder, tool.Name, tool.Content, ct);
        return new GeneratedFile(tool.Name, tool.Content);
    }

    private async Task<string?> ProcessModificationToolAsync(ToolInvocation tool, CancellationToken ct)
    {
        if (tool is not ModifyProjectFileTool modifyTool)
            return null;

        if (_options.Modification.RequireConfirmation)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nModify project file: {modifyTool.Path}");
            Console.Write("Confirm? (y/N): ");
            Console.ResetColor();

            string? response = Console.ReadLine();
            if (!string.Equals(response, "y", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Info($"Modification rejected: {modifyTool.Path}");
                return null;
            }
        }

        bool success = await _fileSystem.WriteProjectFileAsync(modifyTool.Path, modifyTool.Content, ct);
        if (success)
        {
            await _analysis.RefreshFileAsync(modifyTool.Path, ct);
            return modifyTool.Path;
        }

        return null;
    }

    private string CreateResponseFolder()
    {
        int number = Interlocked.Increment(ref _responseCounter);
        return $"{number:D4}_{DateTime.Now:yyyyMMdd_HHmmss}";
    }

    private async Task SaveResponseMarkdownAsync(
        string folder,
        string query,
        string content,
        List<GeneratedFile> files,
        List<string> modifiedFiles,
        float confidence,
        CancellationToken ct)
    {
        StringBuilder md = new();
        md.AppendLine($"# Response");
        md.AppendLine();
        md.AppendLine($"**Query:** {query}");
        md.AppendLine($"**Timestamp:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        md.AppendLine($"**Confidence:** {confidence:P0}");
        md.AppendLine();
        md.AppendLine("---");
        md.AppendLine();
        md.AppendLine(content);

        if (files.Count > 0)
        {
            md.AppendLine();
            md.AppendLine("---");
            md.AppendLine();
            md.AppendLine("## Generated Files");
            foreach (GeneratedFile file in files)
                md.AppendLine($"- {file.Name}");
        }

        if (modifiedFiles.Count > 0)
        {
            md.AppendLine();
            md.AppendLine("## Modified Project Files");
            foreach (string file in modifiedFiles)
                md.AppendLine($"- {file}");
        }

        await _fileSystem.WriteOutputFileAsync(folder, "response.md", md.ToString(), ct);
    }

    private static bool IsExplorationTool(ToolInvocation tool) =>
        tool is ExploreAssemblyTool or ExploreFileTool or ExploreFolderTool or ExploreFilesTool;

    private static bool IsOutputTool(ToolInvocation tool) =>
        tool is GenerateFileTool or AppendFileTool or OverwriteFileTool;

    private static bool IsModificationTool(ToolInvocation tool) =>
        tool is ModifyProjectFileTool;
}