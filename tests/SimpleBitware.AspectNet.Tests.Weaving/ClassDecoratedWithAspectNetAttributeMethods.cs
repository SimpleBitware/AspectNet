using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

[RecordActivity]
public class ClassDecoratedWithAspectNetAttributeMethods
{
    public int PublicValue { get; set; }
    
    public int? PublicNullableValue { get; set; }
    
    public static int PublicStaticValue { get; set; }

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
