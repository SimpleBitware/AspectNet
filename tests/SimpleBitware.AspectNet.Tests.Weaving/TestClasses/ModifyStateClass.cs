using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

[ModifyState]
public class ModifyStateClass<T> : TestClassBase<T>
{
}
