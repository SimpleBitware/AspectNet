using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

/// <summary>
/// Provides runtime execution logic for asynchronous aspect operations.
/// </summary>
/// <remarks>
/// This class contains methods for wrapping Task and Task&lt;T&gt; operations with aspect processing,
/// handling both void and result-returning asynchronous methods.
/// </remarks>
public static class AsyncAspectRunner
{
    /// <summary>
    /// Wraps a Task with aspect processing.
    /// </summary>
    /// <param name="task">The task to wrap, or null to skip execution.</param>
    /// <param name="context">The aspect context containing execution metadata.</param>
    /// <param name="aspect">The aspect instance to apply.</param>
    /// <returns>A task that completes after aspect processing.</returns>
    /// <remarks>
    /// If the task is null, only OnSuccess and OnExit aspect methods are called.
    /// Otherwise, the task is awaited and aspect lifecycle methods are applied.
    /// </remarks>
    public static async Task WrapAsync(Task? task, AspectNetAttributeContext context, IAspectNetAttribute aspect)
    {
        if (task == null) 
        {
            aspect.OnSuccess(context);
            aspect.OnExit(context);
            return;
        }
        
        try
        {
            await task;
            aspect.OnSuccess(context);
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            aspect.OnException(context);
            if (context.Exception == ex)
                throw;

            if (context.Exception != null)
                throw context.Exception;
        }
        finally
        {
            aspect.OnExit(context);
        }
    }

    /// <summary>
    /// Wraps a generic Task&lt;T&gt; with aspect processing.
    /// </summary>
    /// <typeparam name="T">The result type of the task.</typeparam>
    /// <param name="task">The generic task to wrap, or null to skip execution.</param>
    /// <param name="context">The aspect context containing execution metadata.</param>
    /// <param name="aspect">The aspect instance to apply.</param>
    /// <returns>The result of the task after aspect processing.</returns>
    /// <remarks>
    /// If the task is null, only OnSuccess and OnExit aspect methods are called and default(T) is returned.
    /// Otherwise, the task is awaited, the result is processed through aspects, and the final result is returned.
    /// </remarks>
    public static async Task<T?> WrapAsync<T>(Task<T>? task, AspectNetAttributeContext context, IAspectNetAttribute aspect)
    {
        if (task == null) 
        {
            aspect.OnSuccess(context);
            aspect.OnExit(context);
            return default;
        }
        
        try
        {
            var result = await task;
            context.ReturnValue = result;
            aspect.OnSuccess(context);
            return (T?)context.ReturnValue;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            aspect.OnException(context);
            if (context.Exception == ex)
                throw;
            
            return context.Exception != null ? throw context.Exception : default;
        }
        finally
        {
            aspect.OnExit(context);
        }
    }
}
