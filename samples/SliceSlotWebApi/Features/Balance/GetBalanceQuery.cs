using Cap.MiniCqrs;
using SliceSlotWebApi.Infrastructure;

namespace SliceSlotWebApi.Features.Balance;

public sealed record GetBalanceResponse(Guid SessionId, decimal Balance, decimal PendingWin, decimal Bet, bool IsClosed);

public sealed record GetBalanceQuery(string PlayerId) : IQuery<GetBalanceResponse>;

public sealed class GetBalanceQueryHandler : IQueryHandler<GetBalanceQuery, GetBalanceResponse>
{
    private readonly GameStateStore _store;

    public GetBalanceQueryHandler(GameStateStore store) => _store = store;

    public Task<GetBalanceResponse> Handle(GetBalanceQuery query, CancellationToken cancellationToken = default)
    {
        var snapshot = _store.GetBalanceSnapshot(query.PlayerId);
        var response = new GetBalanceResponse(snapshot.SessionId, snapshot.Balance, snapshot.PendingWin, snapshot.Bet, snapshot.IsClosed);
        return Task.FromResult(response);
    }
}
