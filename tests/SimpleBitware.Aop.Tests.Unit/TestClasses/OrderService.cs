using Microsoft.Extensions.Logging;
using SimpleBitware.Aop.Attributes;

namespace SimpleBitware.Aop.Tests.Unit.TestClasses;

public partial class OrderService(ILogger<OrderService> logger) : IOrderService
{
    private readonly ILogger<OrderService> _logger = logger;

    [Log("Orders")]
    public async Task PlaceOrderAsync(string id)
    {
        _logger.LogInformation("Placing order {OrderId}", id);
        await Task.Delay(100);
        _logger.LogInformation("Order {OrderId} placed", id);
    }
}
