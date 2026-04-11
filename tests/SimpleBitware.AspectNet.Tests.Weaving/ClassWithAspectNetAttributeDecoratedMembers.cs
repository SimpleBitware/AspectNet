using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

public class ClassWithAspectNetAttributeDecoratedMembers
{
    [RecordActivity]
    public long PublicValueWithPropertyAttribute { get; set; }
    
    [RecordActivity]
    public static long PublicStaticValueWithPropertyAttribute { get; set; }

    public long PublicValueWithSetterPropertyAttribute
    {
        get;
        
        [RecordActivity]
        set;
    }
    
    public long PublicValueWithGetterPropertyAttribute
    {
        [RecordActivity]
        get;
        set;
    }
    
    [RecordActivity]
    public void PublicMethod()
    {
    }
    
    [RecordActivity]
    public static void PublicStaticMethod()
    {
    }

    public void WrapperForPrivateMethod()
    {
        PrivateMethod();
    }
    
    [RecordActivity]
    private void PrivateMethod()
    {
    }
}
