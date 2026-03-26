using SimpleBitware.AspectNet.Abstractions.Context;

namespace SimpleBitware.AspectNet.Abstractions.Attributes;

public interface IAspectNetAttribute
{
    int Priority { get; set; }
    
    void OnEntry(AspectNetAttributeContext entryContext);
    void OnSuccess(AspectNetAttributeContext entryContext);
    void OnExit(AspectNetAttributeContext context);
    void OnException(AspectNetAttributeContext context);
}
