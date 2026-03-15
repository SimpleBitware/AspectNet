using SimpleBitware.AspectNet.Abstractions;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class AspectNetRuntime
{
    public static void HandleAsyncExtension(object aspect, AspectNetExitContext context)
    {
        if (context.ReturnValue is Task task && aspect is IAspectNetAttribute aspectInterface)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var exContext = new AspectNetExceptionContext {
                        Exception = t.Exception?.InnerException ?? t.Exception!,
                        MemberName = context.MemberName,
                        ClassName = context.ClassName,
                        Parameters = context.Parameters
                    };
                    // Use reflection or a shared interface to call OnException
                    aspectInterface.OnException(exContext);
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
