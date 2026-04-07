using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace DirectPlay4;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDirectPlaySessions
        (this IServiceCollection services, params IEnumerable<Session> sessions)
    {
        foreach (Session session in sessions)
        {
            services.AddSingleton(session);
        }

        return services.AddHostedService<EnumerationService>();
    }
}
