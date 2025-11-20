using Cap.MiniCqrs.Dispatching;
using Cap.MiniCqrs.Registry;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Cap.MiniCqrs.Tests;

public sealed class DispatcherTests
{
    [Fact]
    public async Task Send_Invokes_CommandHandler()
    {
        var services = new ServiceCollection();
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<ICommandHandler<TestCommand, string>, TestCommandHandler>();

        await using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        var response = await dispatcher.Send<string, TestCommand>(new TestCommand("Ada"));

        Assert.Equal("Echo: Ada", response);
    }

    [Fact]
    public async Task Query_Invokes_QueryHandler()
    {
        var services = new ServiceCollection();
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<IQueryHandler<TestQuery, int>, TestQueryHandler>();

        await using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        var response = await dispatcher.Query<int, TestQuery>(new TestQuery(21));

        Assert.Equal(42, response);
    }

    [Fact]
    public async Task AddMiniCqrsHandlersFrom_RegistersHandlersFromAssembly()
    {
        var services = new ServiceCollection();
        services.AddMiniCqrs()
                .AddMiniCqrsHandlersFrom(typeof(DispatcherTests).Assembly);

        await using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        var greet = await dispatcher.Send<string, ReflectedCommand>(new ReflectedCommand("Ada"));
        var count = await dispatcher.Query<int, ReflectedQuery>(new ReflectedQuery(3));

        Assert.Equal("Hi Ada", greet);
        Assert.Equal(3, count);
    }

}

internal sealed record TestCommand(string Value) : ICommand<string>;

internal sealed class TestCommandHandler : ICommandHandler<TestCommand, string>
{
    public Task<string> Handle(TestCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult($"Echo: {command.Value}");

    Task<object> ICommandHandler.Handle(object command, CancellationToken cancellationToken)
        => Task.FromResult<object>($"Echo: {((TestCommand)command).Value}");
}

internal sealed record TestQuery(int Seed) : IQuery<int>;

internal sealed class TestQueryHandler : IQueryHandler<TestQuery, int>
{
    public Task<int> Handle(TestQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(query.Seed * 2);
}

internal sealed record ReflectedCommand(string Name) : ICommand<string>;

internal sealed class ReflectedCommandHandler : ICommandHandler<ReflectedCommand, string>
{
    public Task<string> Handle(ReflectedCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult($"Hi {command.Name}");

    Task<object> ICommandHandler.Handle(object command, CancellationToken cancellationToken)
        => Task.FromResult<object>($"Hi {((ReflectedCommand)command).Name}");
}

internal sealed record ReflectedQuery(int Value) : IQuery<int>;

internal sealed class ReflectedQueryHandler : IQueryHandler<ReflectedQuery, int>
{
    public Task<int> Handle(ReflectedQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(query.Value);
}
