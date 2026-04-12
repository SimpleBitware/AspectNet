using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

[RecordActivity]
public class ClassDecoratedWithAspectNetAttributeMethods
{
    public long PublicValueWithPropertyAttribute { get; set; }
    
    public static long PublicStaticValueWithPropertyAttribute { get; set; }

    public void PublicMethod()
    {
    }
    
    public static void PublicStaticMethod()
    {
    }
}
