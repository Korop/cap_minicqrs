using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Cap.MiniCqrs.Dispatching;

public sealed class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> Send<TResponse, TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TResponse : notnull
        where TCommand : ICommand<TResponse>
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        return handler.Handle(command, cancellationToken);
    }

    public Task<TResponse> Query<TResponse, TQuery>(TQuery query, CancellationToken cancellationToken = default)
        where TResponse : notnull
        where TQuery : IQuery<TResponse>
    {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        return handler.Handle(query, cancellationToken);
    }
}
