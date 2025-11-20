using Cap.MiniCqrs.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SliceSlotWebApi.Features.Collect;

public static class CollectEndpoints
{
    public static IEndpointRouteBuilder MapCollectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/game/collect", async (CollectRequest request, ICachedDispatcher dispatcher, CancellationToken ct) =>
        {
            var result = await dispatcher.Send<CollectResponse, CollectCommand>(
                new CollectCommand(request.PlayerId, request.MessageId), ct);
            return Results.Ok(result);
        });

        return endpoints;
    }
}
