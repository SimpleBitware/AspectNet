using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
public class BenchmarkAttribute: AbstractAspectNetAttribute
{
    private readonly ILogger<BenchmarkAttribute> logger;
    private readonly Stopwatch stopwatch = new();
    public string Format { get; set; } = @"dd\:hh\:mm\:ss\.ffffff";
    
    public BenchmarkAttribute()
    {
        logger = ServiceProvider?.GetRequiredService<ILogger<BenchmarkAttribute>>() ?? throw new ArgumentNullException(nameof(ServiceProvider));
    }

    public override void OnEntry(AspectNetAttributeContext context)
    {
        stopwatch.Start();
        base.OnEntry(context);
    }

    public override void OnExit(AspectNetAttributeContext context)
    {
        base.OnExit(context);
        stopwatch?.Stop();
        LogBenchmark(context);
    }

    private void LogBenchmark(AspectNetAttributeContext context)
    {
        logger.LogInformation("{0}.{1} run for {2}", context.ClassType.FullName, context.MemberName, stopwatch.Elapsed.ToString(Format));
    }
}
