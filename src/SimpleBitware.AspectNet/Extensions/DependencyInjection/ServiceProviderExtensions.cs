using SimpleBitware.AspectNet.Abstractions;

namespace SimpleBitware.AspectNet.Extensions.DependencyInjection;

public static class ServiceProviderExtensions
{
    public static IServiceProvider UseAspectNet(this IServiceProvider serviceProvider)
    {
        AspectNetDependencyInjection.ServiceProvider = serviceProvider;
        return serviceProvider;
    }
}