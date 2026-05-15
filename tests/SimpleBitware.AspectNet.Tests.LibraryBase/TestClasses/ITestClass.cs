using SimpleBitware.AspectNet.Tests.LibraryBase.Attributes;

namespace SimpleBitware.AspectNet.Tests.LibraryBase.TestClasses;

public interface ITestClass<out T>
{
    [RecordActivity(Priority = 50)]
    T? MethodWithReturn();
}
