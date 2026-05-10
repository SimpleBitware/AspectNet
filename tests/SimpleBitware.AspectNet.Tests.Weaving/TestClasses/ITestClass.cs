using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

public interface ITestClass<out T>
{
    [RecordActivity(Priority = 50)]
    T? MethodWithReturn();
}
