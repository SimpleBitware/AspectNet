using Mono.Cecil;
using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

/// <summary>
/// Provides method references for AspectNetAttributeContext properties.
/// </summary>
/// <remarks>
/// This class imports and caches method references for the getter and setter methods
/// of AspectNetAttributeContext properties to avoid repeated imports during IL weaving.
/// </remarks>
public class AspectContextReferences(ModuleCache moduleCache)
{
    /// <summary>
    /// Gets the method reference for the MemberName property getter.
    /// </summary>
    public MethodReference? NameGetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.MemberName))?.GetMethod);

    /// <summary>
    /// Gets the method reference for the MemberName property setter.
    /// </summary>
    public MethodReference? NameSetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.MemberName))?.SetMethod);
    
    /// <summary>
    /// Gets the method reference for the Instance property getter.
    /// </summary>
    public MethodReference? InstanceGetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Instance))!.GetMethod);

    /// <summary>
    /// Gets the method reference for the Instance property setter.
    /// </summary>
    public MethodReference? InstanceSetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Instance))!.SetMethod);
    
    /// <summary>
    /// Gets the method reference for the Exception property getter.
    /// </summary>
    public MethodReference? ExceptionGetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Exception))!.GetMethod);
    
    /// <summary>
    /// Gets the method reference for the Exception property setter.
    /// </summary>
    public MethodReference? ExceptionSetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Exception))!.SetMethod);

    /// <summary>
    /// Gets the method reference for the ReturnValue property getter.
    /// </summary>
    public MethodReference? ReturnValueGetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.ReturnValue))!.GetMethod);

    /// <summary>
    /// Gets the method reference for the ReturnValue property setter.
    /// </summary>
    public MethodReference? ReturnValueSetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.ReturnValue))!.SetMethod);
}
