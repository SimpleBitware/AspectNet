using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

[ExtendedRecordActivity(Priority = 10)]
public class ClassWithMixMultipleAspectNetAttributeDecoratedMethods
{
    [RecordActivity(Priority = 11)]
    public static void PublicStaticMethod()
    {
    }
    
    [RecordActivity(Priority = 2)]
    [RecordActivity(Priority = 1)]
    public static void PublicStaticMethodWithDuplicatedAspects()
    {
    }
    
    [ExtendedRecordActivity(Priority = 2)]
    public static void PublicStaticMethodWithSameAspectAsClass()
    {
    }
    
    [ExtendedRecordActivity(Priority = 3)]
    public static int PublicValue
    {
        [RecordActivity(Priority = 2)]
        [ExtendedRecordActivity(Priority = 1)]
        get;
        set;
    }
}
