using System.Collections.Concurrent;

namespace SliceSlotWebApi.Infrastructure;

public sealed class GameStateStore
{
    private readonly ConcurrentDictionary<string, GameSession> _sessions = new();

    public GameSession InitSession(string playerId, decimal bet)
    {
        var session = new GameSession
        {
            SessionId = Guid.NewGuid(),
            PlayerId = playerId,
            Balance = 1_000m - bet,
            ActiveBet = bet,
            PendingWin = 0m,
            IsClosed = false
        };

        _sessions.AddOrUpdate(playerId, session, (_, _) => session);
        return session;
    }

    public GameSession GetSession(string playerId)
        => _sessions.TryGetValue(playerId, out var session)
            ? session
            : throw new InvalidOperationException($"No active session for player '{playerId}'. Call /init first.");

    public SpinOutcome ApplySpin(string playerId, decimal winAmount, IReadOnlyList<int> symbols)
    {
        var session = GetSession(playerId);
        lock (session.SyncRoot)
        {
            session.LastWin = winAmount;
            session.PendingWin += winAmount;
            session.SpinCount++;
            session.Symbols = symbols.ToArray();
            session.Balance += winAmount;
            return new SpinOutcome(session.SpinCount, session.LastWin, session.Symbols);
        }
    }

    public CollectOutcome Collect(string playerId)
    {
        var session = GetSession(playerId);
        lock (session.SyncRoot)
        {
            var payout = session.PendingWin;
            session.PendingWin = 0m;
            session.IsClosed = true;
            return new CollectOutcome(session.SessionId, payout, session.Balance);
        }
    }

    public BalanceSnapshot GetBalanceSnapshot(string playerId)
    {
        var session = GetSession(playerId);
        lock (session.SyncRoot)
        {
            return new BalanceSnapshot(
                session.SessionId,
                session.PlayerId,
                session.Balance,
                session.PendingWin,
                session.ActiveBet,
                session.IsClosed);
        }
    }
}

public sealed class GameSession
{
    public Guid SessionId { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal ActiveBet { get; set; }
    public decimal LastWin { get; set; }
    public decimal PendingWin { get; set; }
    public bool IsClosed { get; set; }
    public int SpinCount { get; set; }
    public int[] Symbols { get; set; } = Array.Empty<int>();
    public object SyncRoot { get; } = new();
}

public sealed record SpinOutcome(int SpinNumber, decimal WinAmount, IReadOnlyList<int> Symbols);

public sealed record CollectOutcome(Guid SessionId, decimal Payout, decimal BalanceAfterCollect);

public sealed record BalanceSnapshot(Guid SessionId, string PlayerId, decimal Balance, decimal PendingWin, decimal Bet, bool IsClosed);
