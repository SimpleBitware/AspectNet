using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Abstractions.Context;

namespace SimpleBitware.AspectNet.Runtime.Cecil;

public static class AsyncAspectRunner
{
    // For Task and ValueTask
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
            if (context.Exception != null) throw;
        }
        finally
        {
            aspect.OnExit(context);
        }
    }

    // For Task<T> and ValueTask<T>
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
            if (context.Exception != null) throw;
            return default;
        }
        finally
        {
            aspect.OnExit(context);
        }
    }
}
