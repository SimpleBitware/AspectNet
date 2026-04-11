using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

public class ClassWithAspectNetAttributeDecoratedConstructor
{
    [RecordActivity]
    public ClassWithAspectNetAttributeDecoratedConstructor()
    {
    }
    
    [RecordActivity]
    public ClassWithAspectNetAttributeDecoratedConstructor(int no)
    {
    }
}
