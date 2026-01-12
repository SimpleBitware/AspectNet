using Microsoft.Extensions.DependencyInjection;
using SimpleBitware.AspectNet.Tests.Unit.TestClasses;

namespace SimpleBitware.AspectNet.Tests.Unit;

public class UnitTest1
{
    [Test]
    public async Task Test1()
    {
        var svc = Setup.ServiceProvider.GetRequiredService<IOrderService>();
        await svc.PlaceOrderAsync("ABC-123");

        Console.WriteLine("Done. Press any key to exit.");
    }
}