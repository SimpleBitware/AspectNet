using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

[AspectNetExclude]
public class ClassDecoratedWithExcludedAspectNetAttribute
{
    [RecordActivity]
    public ClassDecoratedWithExcludedAspectNetAttribute()
    {
    }

    [RecordActivity]
    public void PublicMethod()
    {
    }

    [RecordActivity]
    public static void PublicStaticMethod()
    {
    }

    [RecordActivity]
    public long PublicValue { get; set; }
}
