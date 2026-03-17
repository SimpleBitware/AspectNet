namespace SimpleBitware.AspectNet.Abstractions;

public sealed class AspectNetExitContext : AbstractAspectNetContext
{
    public AspectNetExitContext() { }
    
    public AspectNetExitContext(AspectNetEntryContext entryContext, object? returnValue)
    {
        ClassName = entryContext.ClassName;
        MemberName = entryContext.MemberName;
        Parameters = entryContext.Parameters;
        ReturnValue = returnValue;
    }
    
    public object? ReturnValue { get; set; }
}
