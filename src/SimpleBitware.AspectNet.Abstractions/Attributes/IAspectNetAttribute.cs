using SimpleBitware.AspectNet.Abstractions.Context;

namespace SimpleBitware.AspectNet.Abstractions.Attributes;

public interface IAspectNetAttribute
{
    int Priority { get; set; }
    
    public void OnEntry(AspectNetEntryContext entryContext);
    public void OnExit(AspectNetExitContext context);
    public void OnException(AspectNetExceptionContext context);
}
