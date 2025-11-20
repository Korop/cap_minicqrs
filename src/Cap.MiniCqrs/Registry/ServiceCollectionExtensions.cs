using System.Reflection;
using Cap.MiniCqrs.Dispatching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cap.MiniCqrs.Registry;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMiniCqrs(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<IDispatcher, Dispatcher>();
        return services;
    }

    public static IServiceCollection AddMiniCqrsHandlersFrom(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        foreach (var assembly in assemblies.Where(a => a != null).Distinct())
        {
            foreach (var type in assembly.DefinedTypes)
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                foreach (var implementedInterface in type.ImplementedInterfaces)
                {
                    if (!implementedInterface.IsGenericType)
                    {
                        continue;
                    }

                    var definition = implementedInterface.GetGenericTypeDefinition();
                    if (definition == typeof(ICommandHandler<,>) || definition == typeof(IQueryHandler<,>))
                    {
                        services.TryAddScoped(implementedInterface, type);
                    }
                }
            }
        }

        return services;
    }
}
