using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

[ExtendedRecordActivity(Priority = 4)]
public class ExtendGapCollection: GapCollection
{
}
