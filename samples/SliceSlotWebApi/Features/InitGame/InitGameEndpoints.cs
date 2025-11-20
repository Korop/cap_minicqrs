using Cap.MiniCqrs.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SliceSlotWebApi.Features.InitGame;

public static class InitGameEndpoints
{
    public static IEndpointRouteBuilder MapInitGameEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/game/init", async (InitGameRequest request, ICachedDispatcher dispatcher, CancellationToken ct) =>
        {
            var result = await dispatcher.Send<InitGameResponse, InitGameCommand>(
                new InitGameCommand(request.PlayerId, request.Bet, request.MessageId), ct);
            return Results.Ok(result);
        });

        return endpoints;
    }
}
