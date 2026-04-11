using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

public class ClassWithAspectNetAttributeDecoratedStaticConstructor
{
    [RecordActivity]
    static ClassWithAspectNetAttributeDecoratedStaticConstructor()
    {
    }
    
    public static int Value { get; set; }
}
