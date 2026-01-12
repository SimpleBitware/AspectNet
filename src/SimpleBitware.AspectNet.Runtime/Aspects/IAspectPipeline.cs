using System;
using System.Threading.Tasks;

namespace SimpleBitware.AspectNet.Runtime.Aspects;

public interface IAspectPipeline
{
    // Sync pipeline
    object? Invoke(
        string methodId,
        object? target,
        object?[] args,
        Func<object?> invokeInner);

    // Async pipeline
    Task<T> InvokeAsync<T>(
        string methodId,
        object? target,
        object?[] args,
        Func<Task<T>> invokeInnerAsync);

    Task InvokeAsync(
        string methodId,
        object? target,
        object?[] args,
        Func<Task> invokeInnerAsync);
}
