// The generator will emit:
// public static IServiceCollection AddAop(this IServiceCollection services)
// and call AddAopCore() plus register proxies.

using Microsoft.Extensions.DependencyInjection;
using SimpleBitware.Aop.Runtime.Aspects;

namespace SimpleBitware.Aop.Runtime.Configuration;

public static class AopServiceCollectionExtensions
{
    public static IServiceCollection AddAopCore(this IServiceCollection services)
    {
        services.AddSingleton<IAspectRegistry, AspectRegistry>();
        services.AddSingleton<IAspectPipeline, AspectPipeline>();
        return services;
    }
}
