using CdCSharp.Theon.Core;
using CdCSharp.Theon.Tools.Commands;
using CdCSharp.Theon.Tools.Queries;
using System.Text.Json;

namespace CdCSharp.Theon.Tools;

public sealed class ToolDispatcher
{
    private readonly Dictionary<string, Func<Dictionary<string, JsonElement>, QueryContext, CommandContext, CancellationToken, Task<object>>> _handlers = [];

    private ToolDispatcher() { }

    public void RegisterQuery<TQuery, TResult>(
        string toolName,
        Func<Dictionary<string, JsonElement>, TQuery> argsMapper,
        IQueryHandler<TQuery, TResult> handler)
        where TQuery : IToolQuery<TResult>
    {
        _handlers[toolName] = async (args, queryCtx, _, ct) =>
        {
            TQuery query = argsMapper(args);
            Result<TResult> result = await handler.HandleAsync(query, queryCtx, ct);
            return result.Match<object>(
                success => success!,
                error => new { error = error.Message, code = error.Code, metadata = error.Metadata });
        };
    }

    public void RegisterCommand<TCommand, TResult>(
        string toolName,
        Func<Dictionary<string, JsonElement>, TCommand> argsMapper,
        ICommandHandler<TCommand, TResult> handler)
        where TCommand : IToolCommand<TResult>
    {
        _handlers[toolName] = async (args, _, commandCtx, ct) =>
        {
            TCommand command = argsMapper(args);
            Result<TResult> result = await handler.HandleAsync(command, commandCtx, ct);
            return result.Match<object>(
                success => success!,
                error => new { error = error.Message, code = error.Code, metadata = error.Metadata });
        };
    }

    public async Task<object> DispatchAsync(
        string toolName,
        Dictionary<string, JsonElement> args,
        QueryContext queryContext,
        CommandContext commandContext,
        CancellationToken ct)
    {
        if (!_handlers.TryGetValue(toolName, out Func<Dictionary<string, JsonElement>, QueryContext, CommandContext, CancellationToken, Task<object>>? handler))
        {
            return new { error = $"Unknown tool: {toolName}" };
        }

        return await handler(args, queryContext, commandContext, ct);
    }

    public async Task<Result<TResult>> ExecuteQueryAsync<TResult>(
        IToolQuery<TResult> query,
        QueryContext context,
        CancellationToken ct)
    {
        IQueryHandler<IToolQuery<TResult>, TResult>? handler = GetQueryHandler<TResult>(query.ToolName);
        if (handler == null)
        {
            return Result<TResult>.Failure(Error.UnknownTool(query.ToolName));
        }

        return await handler.HandleAsync(query, context, ct);
    }

    public async Task<Result<TResult>> ExecuteCommandAsync<TResult>(
        IToolCommand<TResult> command,
        CommandContext context,
        CancellationToken ct)
    {
        ICommandHandler<IToolCommand<TResult>, TResult>? handler = GetCommandHandler<TResult>(command.ToolName);
        if (handler == null)
        {
            return Result<TResult>.Failure(Error.UnknownTool(command.ToolName));
        }

        return await handler.HandleAsync(command, context, ct);
    }

    private IQueryHandler<IToolQuery<TResult>, TResult>? GetQueryHandler<TResult>(string toolName)
    {
        return null;
    }

    private ICommandHandler<IToolCommand<TResult>, TResult>? GetCommandHandler<TResult>(string toolName)
    {
        return null;
    }

    public static ToolDispatcher CreateForContext()
    {
        ToolDispatcher dispatcher = new();

        dispatcher.RegisterQuery(
            "peek_file",
            args => new PeekFileQuery
            {
                Path = args["path"].GetString()!,
                SourceContext = args.TryGetValue("source_context", out JsonElement src) ? src.GetString() : null
            },
            new PeekFileQueryHandler());

        dispatcher.RegisterQuery(
            "search_files",
            args => new SearchFilesQuery
            {
                Pattern = args["pattern"].GetString()!
            },
            new SearchFilesQueryHandler());

        dispatcher.RegisterCommand(
            "read_file",
            args => new LoadFileCommand
            {
                Path = args["path"].GetString()!
            },
            new LoadFileCommandHandler());

        dispatcher.RegisterCommand(
            "create_sub_context",
            args => new CreateSubContextCommand
            {
                ContextType = args["context_type"].GetString() == "clone" ? SubContextType.Clone : SubContextType.Delegate,
                Question = args["question"].GetString()!,
                Files = args["files"].GetString()!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList(),
                TargetContextType = args.TryGetValue("target_type", out JsonElement target) ? target.GetString() : null
            },
            new CreateSubContextCommandHandler());

        return dispatcher;
    }

    public static ToolDispatcher CreateForOrchestrator()
    {
        ToolDispatcher dispatcher = new();

        dispatcher.RegisterCommand(
            "propose_file_change",
            args => new ProposeFileChangeCommand
            {
                Path = args["path"].GetString()!,
                Description = args["description"].GetString()!,
                NewContent = args["new_content"].GetString()!
            },
            new ProposeFileChangeCommandHandler());

        dispatcher.RegisterCommand(
            "create_project_file",
            args => new CreateProjectFileCommand
            {
                Path = args["path"].GetString()!,
                Content = args["content"].GetString()!
            },
            new CreateProjectFileCommandHandler());

        dispatcher.RegisterCommand(
            "generate_output_file",
            args => new GenerateOutputFileCommand
            {
                Folder = args["folder"].GetString()!,
                Filename = args["filename"].GetString()!,
                Content = args["content"].GetString()!
            },
            new GenerateOutputFileCommandHandler());

        return dispatcher;
    }
}