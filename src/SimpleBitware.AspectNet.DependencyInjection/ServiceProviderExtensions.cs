namespace SimpleBitware.AspectNet.DependencyInjection;

public static class ServiceProviderExtensions
{
    public static IServiceProvider UseAspectNet(this IServiceProvider serviceProvider)
    {
        WeaverDependencyInjector.ServiceProvider = serviceProvider;
        return serviceProvider;
    }
}
