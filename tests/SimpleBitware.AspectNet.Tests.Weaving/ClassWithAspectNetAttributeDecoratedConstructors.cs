using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

public class ClassWithAspectNetAttributeDecoratedConstructors
{
    [RecordActivity]
    public ClassWithAspectNetAttributeDecoratedConstructors()
    {
    }
    
    [RecordActivity]
    public ClassWithAspectNetAttributeDecoratedConstructors(int no)
    {
    }
}
