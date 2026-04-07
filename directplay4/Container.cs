using Microsoft.Extensions.DependencyInjection;

namespace DirectPlay4;

public static class Container
{
    public static IServiceCollection AddDirectPlay4(this IServiceCollection services, ServerInfo? serverInfo = null)
    {
        return services
            .AddHostedService<EnumerationService>()
            .AddHostedService<SessionService>()
            .AddSingleton(serverInfo ?? new())
            .AddSingleton<ActiveSessions>();
    }
}
