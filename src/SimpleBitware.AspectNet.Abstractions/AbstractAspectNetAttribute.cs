namespace SimpleBitware.AspectNet.Abstractions;

public abstract class AbstractAspectNetAttribute : Attribute, IAspectNetAttribute
{
    public virtual void OnEntry(AspectNetContext context) { }
    public virtual void OnExit(AspectNetReturnContext context) { }
    public virtual void OnException(AspectNetExceptionContext context) { }
}
