using Microsoft.Extensions.Hosting;

namespace SimpleBitware.AspectNet.DependencyInjection;

public static class HostExtensions
{
    public static IHost UseAspectNet(this IHost host)
    {
        WeaverDependencyInjector.ServiceProvider = host.Services;
        return host;
    }
}
