using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

[ExtendedRecordActivity(Priority = 7)]
public static class StaticClass<T>
{
    [RecordActivity(Priority = 10)]
    public static T? StaticNullableProperty
    {
        [AspectNetExclude]
        get;
        set;
    }
    
    [RecordActivity(Priority = 5)]
    static StaticClass()
    {
        Console.WriteLine("Static constructor called");
    }
}
