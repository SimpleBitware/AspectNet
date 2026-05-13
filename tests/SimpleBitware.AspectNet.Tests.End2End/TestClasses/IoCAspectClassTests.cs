using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SimpleBitware.AspectNet.Abstractions.Extensions.DependencyInjection;
using SimpleBitware.AspectNet.Attributes;
using SimpleBitware.AspectNet.Tests.Library.TestClasses;

namespace SimpleBitware.AspectNet.Tests.End2End.TestClasses;

public partial class IoCAspectClassTests
{
    private readonly Mock<ILogger<BenchmarkAttribute>> loggerMock = new();
    private readonly List<string> capturedMessages = [];
    private const string BenchmarkDefaultLogPattern =
        @"^SimpleBitware\.AspectNet\.Tests\.Library\.TestClasses\.IoCAspectClass\.Method run for " +
        @"(?<days>\d{2}):(?<hours>(?:[01]\d|2[0-3])):(?<minutes>[0-5]\d):(?<seconds>[0-5]\d)\.(?<micro>\d{6})$";
    
    private const string BenchmarkModifiedLogPattern =
        @"^SimpleBitware\.AspectNet\.Tests\.Library\.TestClasses\.IoCAspectClass\.MethodAsync run for " +
        @"(?<days>\d{2}):(?<hours>(?:[01]\d|2[0-3])):(?<minutes>[0-5]\d):(?<seconds>[0-5]\d)\.(?<micro>\d{2})$";
    
    [GeneratedRegex(BenchmarkDefaultLogPattern)]
    private static partial Regex BenchmarkDefaultLogRegex();
    
    [GeneratedRegex(BenchmarkModifiedLogPattern)]
    private static partial Regex BenchmarkModifiedLogRegex();
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        loggerMock
            .Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!))
            .Callback((LogLevel level, EventId eventId, object state, Exception ex, Delegate formatter) =>
            {
                var message = formatter.DynamicInvoke(state, ex)?.ToString();
                if(message is null)
                    return;
                
                capturedMessages.Add(message);
            });
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(loggerMock.Object);
        serviceCollection.BuildServiceProvider().UseAspectNet();
    }
    
    [Test]
    public void Should_Log_Benchmark_Using_Default_Format()
    {
        //when
        IoCAspectClass.Method(Guid.NewGuid().ToString());

        //then
        Assert.That(capturedMessages.Any(x => BenchmarkDefaultLogRegex().IsMatch(x)), Is.True);
    }
    
    [Test]
    public async Task Should_Log_Benchmark_Using_Specified_Format()
    {
        //when
        await IoCAspectClass.MethodAsync(Guid.NewGuid().ToString());

        //then
        Assert.That(capturedMessages.Any(x => BenchmarkModifiedLogRegex().IsMatch(x)), Is.True);
    }
}
