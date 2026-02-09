namespace SampleConsumer;

public partial class MyService
{
    public MyService() { }

    [Log]
    public int Add(int a, int b)
    {
        return a + b;
    }

    // [Log]
    public string Name { get; set; } = "Default";
    
    // [Log]
    private static string StaticName { get; set; } = "Default";
}
