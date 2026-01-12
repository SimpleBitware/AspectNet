using Microsoft.Extensions.DependencyInjection;
using SimpleBitware.AspectNet.Runtime.Generated;

namespace SimpleBitware.AspectNet.Tests.Unit;

public static class Setup
{
    public static IServiceProvider ServiceProvider { get; private set; }

    static Setup()
    {
        ServiceProvider = BuildServiceProvider();
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAop();
        
        return services.BuildServiceProvider();
    }
}