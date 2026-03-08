namespace SimpleBitware.AspectNet.Abstractions;

public interface IAspectNetAttribute
{
    public void OnEntry(AspectNetContext context);
    public void OnExit(AspectNetReturnContext context);
    public void OnException(AspectNetExceptionContext context);
}
