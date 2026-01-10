using System;
using System.Threading.Tasks;

namespace SimpleBitware.Aop.Runtime.Aspects;

public interface IAspectPipeline
{
    // Sync pipeline
    object? Invoke(
        string methodId,
        object? target,
        object?[] args,
        Func<object?> invokeInner);

    // Async pipeline
    Task<object?> InvokeAsync(
        string methodId,
        object? target,
        object?[] args,
        Func<Task<object?>> invokeInnerAsync);
}