using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

[ExtendedRecordActivity(Priority = 8)]
public class ExtendTestCollection : TestCollection<string>
{
}
