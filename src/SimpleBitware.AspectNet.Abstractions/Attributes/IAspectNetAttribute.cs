namespace SimpleBitware.AspectNet.Abstractions.Attributes;

public interface IAspectNetAttribute
{
    /// <summary>
    /// Gets or sets the aspect priority.
    /// Aspects with lower Priority values run first.
    /// Aspects with equal priority runs in order or appearance, with aspects declared first being run first.
    /// </summary>
    int Priority { get; set; }
    
    /// <summary>
    /// Executes before running the decorated method.
    /// </summary>
    /// <param name="context">The context within which the method runs.</param>
    void OnEntry(AspectNetAttributeContext context);
    
    /// <summary>
    /// Executes only when the decorated method runs successfully.
    /// This method is not run if the decorated method throws an exception.
    /// </summary>
    /// <param name="context">The context within which the method runs.</param>
    void OnSuccess(AspectNetAttributeContext context);
    
    /// <summary>
    /// Executes after the decorated method was run.
    /// This method always runs, irrespective if the decorated method throws an exception or not.
    /// </summary>
    /// <param name="context">The context within which the method runs.</param>
    void OnExit(AspectNetAttributeContext context);
    
    /// <summary>
    /// Executes only when the decorated method throws an exception.
    /// </summary>
    /// <param name="context">The context within which the method runs.</param>
    void OnException(AspectNetAttributeContext context);
}
