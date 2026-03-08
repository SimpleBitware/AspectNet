using ConsoleApp1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleBitware.AspectNet.Extensions.DependencyInjection;


var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddSingleton<TestClassX>();

var app = builder.Build();
app.Services.UseAspectNet();   // <-- captures the real provider and initializes AspectDI

Console.WriteLine("FROM CONSTRUCTOR");
var testClassX1 = new TestClassX();
testClassX1.LogMe("Constructor", 1);
TestClassX.LogMe("Constructor static").Wait();
Console.WriteLine($"No={testClassX1.No}");

Console.WriteLine("------------------");

Console.WriteLine("FROM DI");
var testClassX2 = app.Services.GetRequiredService<TestClassX>();
testClassX2.LogMe("DI", 2);
TestClassX.LogMe("DI static").Wait();
Console.WriteLine($"No={testClassX2.No}");

//app.Run();
