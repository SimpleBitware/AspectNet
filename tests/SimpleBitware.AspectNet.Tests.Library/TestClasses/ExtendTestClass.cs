using SimpleBitware.AspectNet.Tests.LibraryBase.Attributes;
using SimpleBitware.AspectNet.Tests.LibraryBase.TestClasses;

namespace SimpleBitware.AspectNet.Tests.Library.TestClasses;

[RecordActivity(Priority = 7)]
public class ExtendTestClass<T> : TestClassBase<T>
{
}
