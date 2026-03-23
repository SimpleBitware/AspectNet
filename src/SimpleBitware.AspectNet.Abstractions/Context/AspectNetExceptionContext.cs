namespace SimpleBitware.AspectNet.Abstractions.Context;

public sealed class AspectNetExceptionContext : AbstractAspectNetContext
{
    public AspectNetExceptionContext() { }
    
    public AspectNetExceptionContext(AspectNetEntryContext entryContext, Exception exception)
    {
        ClassName = entryContext.ClassName;
        MemberName = entryContext.MemberName;
        Parameters = entryContext.Parameters;
        Exception = exception;
    }
    
    /// <summary>
    /// Gets or sets the exception.
    /// Set exception to null to hide the original exception and prevent it to be thrown.
    /// </summary>
    public Exception? Exception { get; set; }
}
