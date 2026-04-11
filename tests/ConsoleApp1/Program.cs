using ConsoleApp1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleBitware.AspectNet.Abstractions.Extensions.DependencyInjection;


var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddSingleton<TestClassX>();

var app = builder.Build();
app.Services.UseAspectNet();   // <-- captures the real provider and initializes AspectDI

Console.WriteLine("STATIC");
TestClassX.LogMeStatic("xxx", 2);

Console.WriteLine("FROM CONSTRUCTOR");
var testClassX1 = new TestClassX();
testClassX1.LogMe("Constructor", 1);

try
{
    testClassX1.LogMeAsync("async").Wait();
}
catch(Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

try
{
    var v = testClassX1.LogMeIntWAsync("int async").Result;
    Console.WriteLine($"Message 2: {v}");
}
catch(Exception ex)
{
    Console.WriteLine($"Error 2: {ex.Message}");
}

try
{
    testClassX1.LogMeWAsync("valuetask async").AsTask().Wait();
}
catch(Exception ex)
{
    Console.WriteLine($"Error 3: {ex.Message}");
}

try
{
    testClassX1.LogMeWithException("static with exception");
}
catch(Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

//Console.WriteLine($"No={testClassX1.No}");
//
Console.WriteLine("------------------");

Console.WriteLine("FROM DI");
var testClassX2 = app.Services.GetRequiredService<TestClassX>();
testClassX2.LogMe("DI", 2);
Console.WriteLine($"No={testClassX2.No}");

//app.Run();
