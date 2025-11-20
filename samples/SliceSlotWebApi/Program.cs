using Cap.MiniCqrs;
using Cap.MiniCqrs.Caching;
using Cap.MiniCqrs.Caching.Memory;
using Cap.MiniCqrs.Dispatching;
using Cap.MiniCqrs.Registry;
using Microsoft.Extensions.Caching.Hybrid;
using SliceSlotWebApi.Features.Balance;
using SliceSlotWebApi.Features.Collect;
using SliceSlotWebApi.Features.InitGame;
using SliceSlotWebApi.Features.Spin;
using SliceSlotWebApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 512 * 1024;
    options.MaximumKeyLength = 256;
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(7),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };
});

builder.Services.AddMiniCqrs()
                .AddMiniCqrsHandlersFrom(typeof(Program).Assembly);
builder.Services.AddSingleton<GameStateStore>();
builder.Services.AddSingleton<IRequestResultCache>(sp =>
    new MemoryRequestResultCache(sp.GetRequiredService<HybridCache>(), TimeSpan.FromMinutes(1)));
builder.Services.AddScoped<IDispatcher, Dispatcher>();
builder.Services.AddScoped<ICachedDispatcher, StrictCachedDispatcher>();

var app = builder.Build();

app.MapInitGameEndpoints()
   .MapSpinEndpoints()
   .MapCollectEndpoints()
   .MapBalanceEndpoints();

app.Run();
