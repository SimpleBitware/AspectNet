namespace SimpleBitware.AspectNet.Abstractions;

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
    
    public required Exception Exception { get; init; }
}
