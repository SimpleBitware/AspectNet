using SimpleBitware.AspectNet.Abstractions.Context;

namespace SimpleBitware.AspectNet.Abstractions.Attributes;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, Inherited =  false)]
public abstract class AbstractAspectNetAttribute : Attribute, IAspectNetAttribute
{
    public int Priority { get; set; } = int.MaxValue;

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
