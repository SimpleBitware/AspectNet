namespace SimpleBitware.AspectNet.Tests.Unit.TestClasses;

public interface IOrderService
{
    Task PlaceOrderAsync(string id);
}