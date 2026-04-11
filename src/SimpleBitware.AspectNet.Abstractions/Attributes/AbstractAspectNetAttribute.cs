namespace SimpleBitware.AspectNet.Abstractions.Attributes;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, Inherited =  false, AllowMultiple = true)]
public abstract class AbstractAspectNetAttribute : Attribute, IAspectNetAttribute
{
    public int Priority { get; set; } = int.MaxValue;

    public virtual void OnEntry(AspectNetAttributeContext context)
    {
    }
    
    public virtual void OnSuccess(AspectNetAttributeContext context)
    {
    }

    public virtual void OnExit(AspectNetAttributeContext context)
    {
    }

    public virtual void OnException(AspectNetAttributeContext context)
    {
    }
}
