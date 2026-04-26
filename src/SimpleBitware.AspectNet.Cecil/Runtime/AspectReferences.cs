using Mono.Cecil;
using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

/// <summary>
/// Provides method references for aspect lifecycle methods.
/// </summary>
/// <remarks>
/// This class imports and caches method references for the four main aspect lifecycle methods
/// (OnEntry, OnSuccess, OnException, OnExit) from the AbstractAspectNetAttribute base class.
/// </remarks>
public class AspectReferences
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AspectReferences"/> class.
    /// </summary>
    /// <param name="moduleCache">The module cache used for importing method references.</param>
    public AspectReferences(ModuleCache moduleCache)
    {
        var baseAspectNetAttributeTypeReference = moduleCache.Resolve(moduleCache.ImportReference(typeof(AbstractAspectNetAttribute)));
        OnEntry = moduleCache.ImportReference(baseAspectNetAttributeTypeReference, nameof(IAspectNetAttribute.OnEntry), 1);
        OnSuccess = moduleCache.ImportReference(baseAspectNetAttributeTypeReference, nameof(IAspectNetAttribute.OnSuccess), 1);
        OnException = moduleCache.ImportReference(baseAspectNetAttributeTypeReference, nameof(IAspectNetAttribute.OnException), 1);
        OnExit = moduleCache.ImportReference(baseAspectNetAttributeTypeReference, nameof(IAspectNetAttribute.OnExit), 1);
    }

    /// <summary>
    /// Gets the method reference for the OnEntry aspect lifecycle method.
    /// </summary>
    public MethodReference OnEntry { get; }
    
    /// <summary>
    /// Gets the method reference for the OnSuccess aspect lifecycle method.
    /// </summary>
    public MethodReference OnSuccess { get; }
    
    /// <summary>
    /// Gets the method reference for the OnException aspect lifecycle method.
    /// </summary>
    public MethodReference OnException { get; }
    
    /// <summary>
    /// Gets the method reference for the OnExit aspect lifecycle method.
    /// </summary>
    public MethodReference OnExit { get; }
}
