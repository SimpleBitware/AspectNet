namespace SimpleBitware.AspectNet.Abstractions.Attributes;

/// <summary>
/// Base class for all AspectNet aspect attributes.
/// Provides virtual methods that can be overridden to implement aspect behavior at different stages of method execution.
/// Can be applied to classes, methods, properties, and constructors.
/// </summary>
/// <remarks>
/// Aspect attributes must have a public default constructor. Only the public default constructor is used by AspectNet.
/// To configure the aspect when applied to a class or member, use public properties.
/// </remarks>
/// <example>
/// `Priority` property.
/// </example>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, AllowMultiple = true)]
public abstract class AbstractAspectNetAttribute : Attribute, IAspectNetAttribute
{
    /// <summary>
    /// Gets the ServiceProvider registered with UseAspectNet ServiceProvider extension.
    /// </summary>
    protected readonly IServiceProvider? ServiceProvider = AspectNetDependencyInjection.ServiceProvider;
    
    /// <summary>
    /// Gets or sets the execution priority of this aspect.
    /// Lower values execute first; equal values execute in declaration order.
    /// Defaults to <see cref="int.MaxValue"/>.
    /// </summary>
    public int Priority { get; set; } = int.MaxValue;

    /// <summary>
    /// Called before the decorated method executes. Override to implement pre-execution logic.
    /// </summary>
    /// <param name="context">The context containing information about the method execution.</param>
    public virtual void OnEntry(AspectNetAttributeContext context)
    {
    }
    
    /// <summary>
    /// Called after the decorated method completes successfully without throwing an exception.
    /// Override to implement post-success logic.
    /// </summary>
    /// <param name="context">The context containing information about the method execution.</param>
    public virtual void OnSuccess(AspectNetAttributeContext context)
    {
    }

    /// <summary>
    /// Called after the decorated method completes, regardless of success or exception.
    /// Override to implement cleanup or finalization logic.
    /// </summary>
    /// <param name="context">The context containing information about the method execution.</param>
    public virtual void OnExit(AspectNetAttributeContext context)
    {
    }

    /// <summary>
    /// Called when the decorated method throws an exception.
    /// Override to implement exception handling logic.
    /// </summary>
    /// <param name="context">The context containing information about the method execution and the thrown exception.</param>
    public virtual void OnException(AspectNetAttributeContext context)
    {
    }
}
