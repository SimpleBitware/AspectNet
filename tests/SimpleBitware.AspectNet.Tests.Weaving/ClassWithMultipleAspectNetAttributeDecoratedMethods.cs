using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

public class ClassWithMultipleAspectNetAttributeDecoratedMethods
{
    [RecordActivity]
    [ExtendedRecordActivity]
    public void PublicMethod()
    {
    }
    
    [RecordActivity]
    [ExtendedRecordActivity]
    public async Task<int> PublicMethodAsync(int value)
    {
        await Task.Delay(100);
        return value;
    }
    
    [ExtendedRecordActivity]
    public Task PublicMethod2Async()
    {
        return Task.CompletedTask;
    }
    
    [RecordActivity(Priority = 2)]
    [ExtendedRecordActivity(Priority = 1)]
    public Task<int> PublicAsyncMethodWithAsyncException(int value)
    {
        return Task.FromException<int>(new Exception($"{value}"));
    }
    
    [RecordActivity]
    [ExtendedRecordActivity]
    public Task<int> PublicAsyncMethodWithSyncException(int value)
    {
        throw new Exception($"{value}");
    }
}
