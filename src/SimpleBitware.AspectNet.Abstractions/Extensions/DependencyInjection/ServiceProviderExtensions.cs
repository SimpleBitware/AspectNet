namespace SimpleBitware.AspectNet.Abstractions.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring AspectNet with dependency injection.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Registers the service provider to be used by AspectNet for dependency resolution.
    /// This method should be called to enable aspects to retrieve services from the dependency injection container.
    /// </summary>
    /// <param name="serviceProvider">The service provider to register for AspectNet dependency resolution.</param>
    /// <returns>The same service provider instance for method chaining.</returns>
    public static IServiceProvider UseAspectNet(this IServiceProvider serviceProvider)
    {
        AspectNetDependencyInjection.ServiceProvider = serviceProvider;
        return serviceProvider;
    }
}
