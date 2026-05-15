using SimpleBitware.AspectNet.Tests.Library.Attributes;

namespace SimpleBitware.AspectNet.Tests.Library.TestClasses;

[ExtendedRecordActivity(Priority = 8)]
public class ExtendTestCollection : TestCollection<string>
{
}
