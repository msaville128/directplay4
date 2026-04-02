using Microsoft.Extensions.DependencyInjection;

namespace DirectPlay4;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDirectPlay4(this IServiceCollection services)
    {
        return services
            .AddHostedService<EnumerationService>();
    }
}
