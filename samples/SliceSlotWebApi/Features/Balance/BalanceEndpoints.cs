using Cap.MiniCqrs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SliceSlotWebApi.Features.Balance;

public static class BalanceEndpoints
{
    public static IEndpointRouteBuilder MapBalanceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/game/balance/{playerId}", async (string playerId, IDispatcher dispatcher, CancellationToken ct) =>
        {
            var result = await dispatcher.Query<GetBalanceResponse, GetBalanceQuery>(new GetBalanceQuery(playerId), ct);
            return Results.Ok(result);
        });

        return endpoints;
    }
}
