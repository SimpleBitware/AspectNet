using Microsoft.Extensions.DependencyInjection;

namespace SimpleBitware.AspectNet.Abstractions;

public static class AspectNetDependencyInjection
{
    internal static IServiceProvider? ServiceProvider { get; set; }

    public static T GetRequiredService<T>() where T : class, new()
    {
        var instance = ServiceProvider?.GetService<T>();
        return instance ?? new T();
    }
}
