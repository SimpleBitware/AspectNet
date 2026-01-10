using Microsoft.Extensions.DependencyInjection;
using SimpleBitware.Aop.Runtime.Configuration;
using SimpleBitware.Aop.Runtime.Aspects;
using SimpleBitware.Aop.Attributes;
using SimpleBitware.Aop.Runtime.Generated;

namespace SimpleBitware.Aop.Tests.Unit;

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