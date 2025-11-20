using Cap.MiniCqrs.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SliceSlotWebApi.Features.Spin;

public static class SpinEndpoints
{
    public static IEndpointRouteBuilder MapSpinEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/game/spin", async (SpinRequest request, ICachedDispatcher dispatcher, CancellationToken ct) =>
        {
            var result = await dispatcher.Send<SpinResponse, SpinCommand>(
                new SpinCommand(request.PlayerId, request.MessageId), ct);
            return Results.Ok(result);
        });

        return endpoints;
    }
}
