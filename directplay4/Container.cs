using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

namespace DirectPlay4;

public static class Container
{
    public static IServiceCollection AddDirectPlay4(this IServiceCollection services)
    {
        return services
            .AddHostedService<EnumerationService>()
            .AddHostedService<OutboundService>()
            .AddSingleton(Channel.CreateBounded<OutgoingMessage>(capacity: 100))
            .AddSingleton<ActiveSessions>();
    }
}
