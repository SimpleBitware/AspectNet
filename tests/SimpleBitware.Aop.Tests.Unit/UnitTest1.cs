using Microsoft.Extensions.DependencyInjection;
using SimpleBitware.Aop.Tests.Unit.TestClasses;

namespace SimpleBitware.Aop.Tests.Unit;

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