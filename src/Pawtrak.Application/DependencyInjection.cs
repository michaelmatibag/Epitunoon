using Microsoft.Extensions.DependencyInjection;

namespace Pawtrak.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application-layer service registrations go here.
        // Concrete implementations live in Pawtrak.Infrastructure and are registered there.
        return services;
    }
}
