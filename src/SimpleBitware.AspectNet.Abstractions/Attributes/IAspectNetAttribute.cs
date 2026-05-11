namespace SimpleBitware.AspectNet.Abstractions.Attributes;

/// <summary>
/// Interface used by the weaver to identify AspectNet attributes.
/// </summary>
public interface IAspectNetAttribute
{
    /// <summary>
    /// Gets or sets the execution priority of this aspect.
    /// Lower values execute first; equal values execute in declaration order.
    /// Defaults to <see cref="int.MaxValue"/>.
    /// </summary>
    int Priority { get; set; }
    
    /// <summary>
    /// Called before the decorated method executes. Override to implement pre-execution logic.
    /// </summary>
    /// <param name="context">The context containing information about the method execution.</param>
    void OnEntry(AspectNetAttributeContext context);
    
    /// <summary>
    /// Called after the decorated method completes successfully without throwing an exception.
    /// Override to implement post-success logic.
    /// </summary>
    /// <param name="context">The context containing information about the method execution.</param>
    void OnSuccess(AspectNetAttributeContext context);
    
    /// <summary>
    /// Called after the decorated method completes, regardless of success or exception.
    /// Override to implement cleanup or finalization logic.
    /// </summary>
    /// <param name="context">The context containing information about the method execution.</param>
    void OnExit(AspectNetAttributeContext context);
    
    /// <summary>
    /// Called when the decorated method throws an exception.
    /// Override to implement exception handling logic.
    /// </summary>
    /// <param name="context">The context containing information about the method execution and the thrown exception.</param>
    void OnException(AspectNetAttributeContext context);
}
