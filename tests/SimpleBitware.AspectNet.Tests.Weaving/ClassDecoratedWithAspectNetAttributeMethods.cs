using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

[RecordActivity]
public class ClassDecoratedWithAspectNetAttributeMethods
{
    public long PublicValue { get; set; }
    
    public static long PublicStaticValue { get; set; }

    public int PublicValueException => throw new Exception(nameof(PublicValueException));

    public ClassDecoratedWithAspectNetAttributeMethods()
    {
    }
    
    public ClassDecoratedWithAspectNetAttributeMethods(int no)
    {
        throw new Exception($"Constructor {no}");
    }

    public void PublicMethod()
    {
    }
    
    public void PublicMethodException()
    {
        throw new Exception("PublicMethodException");
    }
    
    public static void PublicStaticMethod()
    {
    }
}
