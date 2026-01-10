using System;
using Microsoft.Extensions.Logging;
using SimpleBitware.Aop.Runtime.Aspects;

namespace SimpleBitware.Aop.Attributes;

public sealed class LogMethodAspect : IMethodAspect
{
    private readonly ILogger _logger;

    public LogMethodAspect(ILoggerFactory loggerFactory, string category)
    {
        _logger = loggerFactory.CreateLogger(category);
    }

    public void OnBefore(MethodContext context)
        => _logger.LogInformation("Entering {MethodId}", context.MethodId);

    public void OnSuccess(MethodContext context)
        => _logger.LogInformation("Success {MethodId}", context.MethodId);

    public void OnException(MethodContext context, Exception ex)
        => _logger.LogError(ex, "Exception in {MethodId}", context.MethodId);
}