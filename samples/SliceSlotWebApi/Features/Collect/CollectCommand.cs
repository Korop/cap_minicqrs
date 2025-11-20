using Cap.MiniCqrs;
using Cap.MiniCqrs.Caching;
using SliceSlotWebApi.Infrastructure;

namespace SliceSlotWebApi.Features.Collect;

public sealed record CollectRequest(string PlayerId, long MessageId);

public sealed record CollectResponse(Guid SessionId, decimal Payout, decimal BalanceAfterCollect);

public sealed record CollectCommand(string PlayerId, long MessageId) : ICommand<CollectResponse>, ICacheKeyProvider
{
    public string CacheKey => $"collect:{PlayerId}:{MessageId}";
}

public sealed class CollectCommandHandler : ICommandHandler<CollectCommand, CollectResponse>
{
    private readonly GameStateStore _store;

    public CollectCommandHandler(GameStateStore store)
    {
        _store = store;
    }

    public Task<CollectResponse> Handle(CollectCommand command, CancellationToken cancellationToken = default)
    {
        var outcome = _store.Collect(command.PlayerId);
        return Task.FromResult(new CollectResponse(outcome.SessionId, outcome.Payout, outcome.BalanceAfterCollect));
    }

    async Task<object> ICommandHandler.Handle(object command, CancellationToken cancellationToken)
        => await Handle((CollectCommand)command, cancellationToken);
}
