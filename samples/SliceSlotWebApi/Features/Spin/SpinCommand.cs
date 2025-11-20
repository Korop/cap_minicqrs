using Cap.MiniCqrs;
using Cap.MiniCqrs.Caching;
using SliceSlotWebApi.Infrastructure;

namespace SliceSlotWebApi.Features.Spin;

public sealed record SpinRequest(string PlayerId, long MessageId);

public sealed record SpinResponse(int SpinNumber, decimal WinAmount, IReadOnlyList<int> Symbols);

public sealed record SpinCommand(string PlayerId, long MessageId) : ICommand<SpinResponse>, ICacheKeyProvider
{
    public string CacheKey => $"spin:{PlayerId}:{MessageId}";
}

public sealed class SpinCommandHandler : ICommandHandler<SpinCommand, SpinResponse>
{
    private readonly GameStateStore _store;

    public SpinCommandHandler(GameStateStore store)
    {
        _store = store;
    }

    public Task<SpinResponse> Handle(SpinCommand command, CancellationToken cancellationToken = default)
    {
        var win = Random.Shared.Next(0, 4) switch
        {
            0 => 0m,
            1 => 5m,
            2 => 10m,
            _ => 25m
        };

        var symbols = Enumerable.Range(0, 3)
            .Select(_ => Random.Shared.Next(1, 7))
            .ToArray();

        var outcome = _store.ApplySpin(command.PlayerId, win, symbols);
        return Task.FromResult(new SpinResponse(outcome.SpinNumber, outcome.WinAmount, outcome.Symbols));
    }

    async Task<object> ICommandHandler.Handle(object command, CancellationToken cancellationToken)
        => await Handle((SpinCommand)command, cancellationToken);
}
