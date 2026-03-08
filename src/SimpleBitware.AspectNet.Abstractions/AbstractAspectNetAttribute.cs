namespace SimpleBitware.AspectNet.Abstractions;

public abstract class AbstractAspectNetAttribute : Attribute
{
    public virtual void OnEntry(AspectNetContext context) { }
    public virtual void OnExit(AspectNetContext context) { }
    public virtual void OnException(AspectNetContext context, Exception ex) { }
}