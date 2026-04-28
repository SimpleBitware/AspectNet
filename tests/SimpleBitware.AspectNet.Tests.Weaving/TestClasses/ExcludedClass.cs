using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

[AspectNetExclude]
public class ExcludedClass<T> : TestClassBase<T>
{
}
