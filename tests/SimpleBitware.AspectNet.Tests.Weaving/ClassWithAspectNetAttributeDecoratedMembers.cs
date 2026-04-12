using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving;

public class ClassWithAspectNetAttributeDecoratedMembers
{
    [RecordActivity]
    public int PublicValueWithPropertyAttribute { get; set; }
    
    [RecordActivity]
    public static int PublicStaticValueWithPropertyAttribute { get; set; }

    public int PublicValueWithSetterPropertyAttribute
    {
        get;
        
        [RecordActivity]
        set;
    }
    
    public int PublicValueWithGetterPropertyAttribute
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
    public async Task<int> PublicMethodAsync(int value)
    {
        await Task.Delay(100);
        return value;
    }
    
    [RecordActivity]
    public Task<int> PublicAsyncMethodWithAsyncException(int value)
    {
        return Task.FromException<int>(new Exception($"{value}"));
    }
    
    [RecordActivity]
    public Task<int> PublicAsyncMethodWithSyncException(int value)
    {
        throw new Exception($"{value}");
    }
    
    [RecordActivity]
    public ValueTask<int> PublicValueTaskMethod(int value)
    {
        return ValueTask.FromResult(value);
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
