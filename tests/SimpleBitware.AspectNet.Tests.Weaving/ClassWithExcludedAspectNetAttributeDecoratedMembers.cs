using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

public class ClassWithExcludedAspectNetAttributeDecoratedMembers
{
    [RecordActivity]
    [AspectNetExclude]
    public ClassWithExcludedAspectNetAttributeDecoratedMembers()
    {
    }
    
    [RecordActivity]
    [AspectNetExclude]
    public void PublicMethod()
    {
    }
    
    [RecordActivity]
    [AspectNetExclude]
    public static void PublicStaticMethod()
    {
    }
    
    [RecordActivity]
    [AspectNetExclude]
    public int PublicValue { get; set; }
}
