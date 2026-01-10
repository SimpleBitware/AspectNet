namespace SimpleBitware.Aop.Tests.Unit.TestClasses;

public interface IOrderService
{
    Task PlaceOrderAsync(string id);
}