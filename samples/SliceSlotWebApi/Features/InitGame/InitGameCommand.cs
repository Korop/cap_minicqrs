using Cap.MiniCqrs;
using Cap.MiniCqrs.Caching;
using SliceSlotWebApi.Infrastructure;

namespace SliceSlotWebApi.Features.InitGame;

public sealed record InitGameRequest(string PlayerId, decimal Bet, long MessageId);

public sealed record InitGameResponse(Guid SessionId, decimal Balance, decimal Bet, string[] AvailableActions);

public sealed record InitGameCommand(string PlayerId, decimal Bet, long MessageId)
    : ICommand<InitGameResponse>, ICacheKeyProvider
{
    public string CacheKey => $"init:{PlayerId}:{MessageId}";
}

public sealed class InitGameCommandHandler : ICommandHandler<InitGameCommand, InitGameResponse>
{
    private static readonly string[] Actions = ["api/game/spin", "api/game/collect"];
    private readonly GameStateStore _store;

    public InitGameCommandHandler(GameStateStore store)
    {
        _store = store;
    }

    public Task<InitGameResponse> Handle(InitGameCommand command, CancellationToken cancellationToken = default)
    {
        var session = _store.InitSession(command.PlayerId, command.Bet);
        var response = new InitGameResponse(session.SessionId, session.Balance, session.ActiveBet, Actions);
        return Task.FromResult(response);
    }

    async Task<object> ICommandHandler.Handle(object command, CancellationToken cancellationToken)
        => await Handle((InitGameCommand)command, cancellationToken);
}
