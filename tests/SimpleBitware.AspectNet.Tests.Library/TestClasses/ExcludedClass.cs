using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.LibraryBase.TestClasses;

namespace SimpleBitware.AspectNet.Tests.Library.TestClasses;

[AspectNetExclude]
public class ExcludedClass<T> : TestClassBase<T>
{
}
