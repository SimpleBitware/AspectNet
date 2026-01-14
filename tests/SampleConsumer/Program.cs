using System;
class Program
{
    [Log]
    static void Main()
    {
        var s = new MyService();
        Console.WriteLine($"Sum: {s.Add(2,3)}");
        s.Name = "Updated";
        Console.WriteLine($"Name: {s.Name}");
    }
}
