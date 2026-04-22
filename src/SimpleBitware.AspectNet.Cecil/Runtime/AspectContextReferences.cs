using Mono.Cecil;
using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

public class AspectContextReferences(ModuleCache moduleCache)
{
    public MethodReference? NameGetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.MemberName))?.GetMethod);
    public MethodReference? NameSetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.MemberName))?.SetMethod);
    public MethodReference? InstanceGetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Instance))!.GetMethod);
    public MethodReference? InstanceSetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Instance))!.SetMethod);
    public MethodReference? ExceptionGetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Exception))!.GetMethod);
    public MethodReference? ExceptionSetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Exception))!.SetMethod);
    public MethodReference? ReturnValueGetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.ReturnValue))!.GetMethod);
    public MethodReference? ReturnValueSetMethod { get; } = moduleCache.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.ReturnValue))!.SetMethod);
}
