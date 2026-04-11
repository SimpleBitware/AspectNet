namespace SimpleBitware.AspectNet.Abstractions.Attributes;

public interface IAspectNetAttribute
{
    int Priority { get; set; }
    
    void OnEntry(AspectNetAttributeContext context);
    void OnSuccess(AspectNetAttributeContext context);
    void OnExit(AspectNetAttributeContext context);
    void OnException(AspectNetAttributeContext context);
}
