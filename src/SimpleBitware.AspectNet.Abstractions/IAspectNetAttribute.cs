namespace SimpleBitware.AspectNet.Abstractions;

public interface IAspectNetAttribute
{
    int Priority { get; set; }
    
    public void OnEntry(AspectNetEntryContext entryContext);
    public void OnExit(AspectNetExitContext context);
    public void OnException(AspectNetExceptionContext context);
}
