namespace SampleConsumer;

public partial class MyService
{
    [Log]
    public MyService() { }

    [Log]
    public int Add(int a, int b) => a + b;

    // [Log]
    public string Name { get; set; } = "Default";
    
    // [Log]
    private static string StaticName { get; set; } = "Default";
}
