using Cap.MiniCqrs;
using Cap.MiniCqrs.Registry;
using Microsoft.Extensions.DependencyInjection;

// Minimal sample showing how to wire the dispatcher and handlers using the built-in DI helpers.
var services = new ServiceCollection();
services.AddMiniCqrs()
        .AddMiniCqrsHandlersFrom(typeof(Program).Assembly);

await using var provider = services.BuildServiceProvider();
var dispatcher = provider.GetRequiredService<IDispatcher>();

var greeting = await dispatcher.Send<GreetUserResponse, GreetUserCommand>(new GreetUserCommand("Ada"));
Console.WriteLine(greeting.Message);

var stats = await dispatcher.Query<UserCountResponse, UserCountQuery>(new UserCountQuery());
Console.WriteLine($"Active users in cache: {stats.Count}");

public sealed record GreetUserCommand(string Name) : ICommand<GreetUserResponse>;

public sealed record GreetUserResponse(string Message);

public sealed class GreetUserHandler : ICommandHandler<GreetUserCommand, GreetUserResponse>
{
    public Task<GreetUserResponse> Handle(GreetUserCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult(new GreetUserResponse($"Hello, {command.Name}!"));

    Task<object> ICommandHandler.Handle(object command, CancellationToken cancellationToken)
        => command is GreetUserCommand typed
            ? Task.FromResult<object>(new GreetUserResponse($"Hello, {typed.Name}!"))
            : throw new InvalidOperationException("Unsupported command.");
}

public sealed record UserCountQuery : IQuery<UserCountResponse>;

public sealed record UserCountResponse(int Count);

public sealed class UserCountHandler : IQueryHandler<UserCountQuery, UserCountResponse>
{
    public Task<UserCountResponse> Handle(UserCountQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(new UserCountResponse(5));
}
