using Microsoft.Extensions.DependencyInjection;

namespace SimpleBitware.AspectNet.Abstractions;

/// <summary>
/// Provides dependency injection services for AspectNet aspects.
/// Allows aspects to resolve services from the application service provider.
/// </summary>
public static class AspectNetDependencyInjection
{
    /// <summary>
    /// Gets or sets the service provider used to resolve aspect dependencies.
    /// </summary>
    internal static IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// Retrieves a required service of type <typeparamref name="T"/> from the service provider.
    /// If the service is not available, returns a new instance of <typeparamref name="T"/> using the default constructor.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve. Must be a class with a parameterless constructor.</typeparam>
    /// <returns>An instance of the requested service type.</returns>
    public static T GetRequiredService<T>() where T : class, new()
    {
        var instance = ServiceProvider?.GetService<T>();
        return instance ?? new T();
    }
}
