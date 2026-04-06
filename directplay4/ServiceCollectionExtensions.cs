using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

namespace DirectPlay4;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDirectPlay4(this IServiceCollection services)
    {
        return services
            .AddHostedService<EnumerationService>()
            .AddSingleton(Channel.CreateBounded<OutgoingMessage>(capacity: 100))
            .AddSingleton<ActiveSessions>();
    }
}
