using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

public class ClassForTestingModifyState
{
    [ModifyState]
    public static string Value { get; set; } = "Initial Value";

    [ModifyState]
    public static string MethodWithException()
    {
        throw new NotImplementedException();
    }
    
    [ModifyState]
    public static Task AsyncMethodWithSyncException()
    {
        throw new NotImplementedException();
    }
}
