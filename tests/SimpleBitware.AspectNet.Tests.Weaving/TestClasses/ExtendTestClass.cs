using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

[RecordActivity(Priority = 7)]
public class ExtendTestClass<T> : TestClassBase<T>
{
}
