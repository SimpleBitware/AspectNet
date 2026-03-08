namespace ConsoleApp1;

public class TestClassX
{
    [Log]
    public TestClassX()
    {
    }
    
    [Log]
    public void LogMe(string message, int no)
    {
        Console.WriteLine($"LogMe: {message} {no}");
    }
    
    [Log]
    public static async Task LogMe(string message)
    {
        Console.WriteLine($"LogMe: {message}");
        await Task.Delay(100);
    }
}