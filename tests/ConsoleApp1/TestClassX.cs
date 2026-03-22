namespace ConsoleApp1;

public class TestClassX
{
    [Log(Priority = 5)]
    [Log2(Priority = 1)]
    public TestClassX( int no = 2)
    {
        this.No = no;
        this.NoProt = no + 1;
        Console.WriteLine(no);
        LogMePrivate("X Constructor", no);
    }
    
    [Log]
    public void LogMe(string message, int no)
    {
        Console.WriteLine($"LogMe: {message} {no}");
    }
    
    [Log]
    private void LogMePrivate(string message, int no)
    {
        Console.WriteLine($"LogMe Private: {message} {no}");
    }
    
    [Log]
    [Log2]
    public async Task LogMeAsync(string message)
    {
        Console.WriteLine($"LogMe: {message}");
        await Task.Delay(1);
        throw new InvalidOperationException("Test exception in static method");
    }
    
    [Log]
    [Log2(Priority = 1)]
    public int No { get; private set; }
    
    [Log]
    protected int NoProt { get; private set; }
}
