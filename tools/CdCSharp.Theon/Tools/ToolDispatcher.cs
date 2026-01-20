using CdCSharp.Theon.Core;
using System.Text.Json;

namespace CdCSharp.Theon.Tools;

public sealed class ToolDispatcher
{
    private readonly Dictionary<string, Func<Dictionary<string, JsonElement>, ToolContext, CancellationToken, Task<object>>> _handlers = [];

    private ToolDispatcher() { }

    public void Register<TTool, TResult>(
        string toolName,
        Func<Dictionary<string, JsonElement>, TTool> argsMapper,
        IToolHandler<TTool, TResult> handler)
        where TTool : ITool<TResult>
    {
        _handlers[toolName] = async (args, context, ct) =>
        {
            TTool tool = argsMapper(args);
            Result<TResult> result = await handler.HandleAsync(tool, context, ct);
            return result.Match<object>(
                success => success!,
                error => new { error = error.Message, code = error.Code, metadata = error.Metadata });
        };
    }

    public async Task<object> DispatchAsync(
        string toolName,
        Dictionary<string, JsonElement> args,
        ToolContext context,
        CancellationToken ct)
    {
        if (!_handlers.TryGetValue(toolName, out Func<Dictionary<string, JsonElement>, ToolContext, CancellationToken, Task<object>>? handler))
        {
            return new { error = $"Unknown tool: {toolName}" };
        }

        return await handler(args, context, ct);
    }

    public async Task<Result<TResult>> ExecuteAsync<TResult>(
        ITool<TResult> tool,
        ToolContext context,
        CancellationToken ct)
    {
        if (!_handlers.TryGetValue(tool.ToolName, out Func<Dictionary<string, JsonElement>, ToolContext, CancellationToken, Task<object>>? handler))
        {
            return Result<TResult>.Failure(Error.UnknownTool(tool.ToolName));
        }

        try
        {
            object result = await handler(
                [],
                context,
                ct);

            if (result is TResult typedResult)
            {
                return Result<TResult>.Success(typedResult);
            }

            return Result<TResult>.Failure(
                Error.Custom("TYPE_MISMATCH", $"Tool returned unexpected type"));
        }
        catch (Exception ex)
        {
            return Result<TResult>.Failure(
                Error.Custom("TOOL_EXECUTION_ERROR", ex.Message));
        }
    }

    public static ToolDispatcher CreateForContext()
    {
        ToolDispatcher dispatcher = new();

        dispatcher.Register(
            "peek_file",
            args => new PeekFileTool
            {
                Path = args["path"].GetString()!,
                SourceContext = args.TryGetValue("source_context", out JsonElement src) ? src.GetString() : null
            },
            new PeekFileHandler());

        dispatcher.Register(
            "read_file",
            args => new ReadFileTool
            {
                Path = args["path"].GetString()!
            },
            new ReadFileHandler());

        dispatcher.Register(
            "search_files",
            args => new SearchFilesTool
            {
                Pattern = args["pattern"].GetString()!
            },
            new SearchFilesHandler());

        dispatcher.Register(
            "create_sub_context",
            args => new CreateSubContextTool
            {
                ContextType = args["context_type"].GetString() == "clone"
                    ? SubContextType.Clone
                    : SubContextType.Delegate,
                Question = args["question"].GetString()!,
                Files = args["files"].GetString()!
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .ToList(),
                TargetContextType = args.TryGetValue("target_type", out JsonElement target)
                    ? target.GetString()
                    : null
            },
            new CreateSubContextHandler());

        return dispatcher;
    }

    public static ToolDispatcher CreateForOrchestrator()
    {
        ToolDispatcher dispatcher = new();

        dispatcher.Register(
            "create_execution_plan",
            args => new CreateExecutionPlanTool
            {
                UserRequest = args["user_request"].GetString()!
            },
            new CreateExecutionPlanHandler());

        dispatcher.Register(
            "propose_file_change",
            args => new ProposeFileChangeTool
            {
                Path = args["path"].GetString()!,
                Description = args["description"].GetString()!,
                NewContent = args["new_content"].GetString()!
            },
            new ProposeFileChangeHandler());

        dispatcher.Register(
            "create_project_file",
            args => new CreateProjectFileTool
            {
                Path = args["path"].GetString()!,
                Content = args["content"].GetString()!
            },
            new CreateProjectFileHandler());

        dispatcher.Register(
            "generate_output_file",
            args => new GenerateOutputFileTool
            {
                Folder = args["folder"].GetString()!,
                Filename = args["filename"].GetString()!,
                Content = args["content"].GetString()!
            },
            new GenerateOutputFileHandler());

        return dispatcher;
    }
}