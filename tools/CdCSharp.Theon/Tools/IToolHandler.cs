using CdCSharp.Theon.Core;

namespace CdCSharp.Theon.Tools;

public interface IToolQuery<TResult>
{
    string ToolName { get; }
}

public interface IToolCommand<TResult>
{
    string ToolName { get; }
    bool RequiresConfirmation { get; }
}

public interface IQueryHandler<TQuery, TResult>
    where TQuery : IToolQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, QueryContext context, CancellationToken ct);
}

public interface ICommandHandler<TCommand, TResult>
    where TCommand : IToolCommand<TResult>
{
    Task<Result<TResult>> HandleAsync(TCommand command, CommandContext context, CancellationToken ct);
}