namespace SimpleBitware.AspectNet.Abstractions;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, Inherited =  false)]
public abstract class AbstractInterceptorAttribute : Attribute, IAspectNetAttribute
{
    public int Priority { get; set; } = 0;

    public virtual void OnEntry(AspectNetEntryContext entryContext)
    {
    }

    public virtual void OnExit(AspectNetExitContext context)
    {
    }

    public virtual void OnException(AspectNetExceptionContext context)
    {
    }
}
