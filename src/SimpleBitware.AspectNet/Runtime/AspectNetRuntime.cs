using SimpleBitware.AspectNet.Abstractions;

namespace SimpleBitware.AspectNet.Runtime;

public static class AspectNetRuntime
{
    public static T HandleAsync<T>(T taskResult, object aspect, AspectNetExitContext context)
    {
        if (taskResult is Task task)
        {
            // We return a new task that wraps the original
            return (T)(object)task.ContinueWith(t =>
            {
                // Here you could manually call aspect.OnExit again 
                // or update the context.
                if (t.IsFaulted && t.Exception != null)
                {
                    // Handle async exception
                }
                return t;
            }).Unwrap();
        }
        return taskResult;
    }
}
