using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

public class ClassForTestingModifyState
{
    [HideException]
    public static string Value { get; set; } = "Initial Value";

    [HideException]
    public static string MethodWithException()
    {
        throw new NotImplementedException();
    }
    
    [HideException]
    public static Task AsyncMethodWithSyncException()
    {
        throw new NotImplementedException();
    }
}
