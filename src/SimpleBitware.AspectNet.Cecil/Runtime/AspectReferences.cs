using Mono.Cecil;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Cecil.Extensions;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

public class AspectReferences(ModuleDefinition targetModule, TypeDefinition baseAspectNetAttributeTypeDefinition)
{
    public MethodReference OnEntry { get; } = targetModule.Cache().ImportAndCache(baseAspectNetAttributeTypeDefinition, nameof(IAspectNetAttribute.OnEntry), 1);
    public MethodReference OnSuccess { get; } = targetModule.Cache().ImportAndCache(baseAspectNetAttributeTypeDefinition, nameof(IAspectNetAttribute.OnSuccess), 1);
    public MethodReference OnException { get; } = targetModule.Cache().ImportAndCache(baseAspectNetAttributeTypeDefinition, nameof(IAspectNetAttribute.OnException), 1);
    public MethodReference OnExit { get; } = targetModule.Cache().ImportAndCache(baseAspectNetAttributeTypeDefinition, nameof(IAspectNetAttribute.OnExit), 1);
}
