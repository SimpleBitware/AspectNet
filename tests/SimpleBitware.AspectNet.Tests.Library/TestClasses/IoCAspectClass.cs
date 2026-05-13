using SimpleBitware.AspectNet.Attributes;

namespace SimpleBitware.AspectNet.Tests.Library.TestClasses;

public static class IoCAspectClass
{
    [Benchmark]
    public static async Task<string> MethodAsync(string parameter)
    {
        await Task.Delay(100);
        return parameter;
    }
}
