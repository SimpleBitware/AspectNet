using Mono.Cecil;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Cecil.Extensions;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

public class AspectReferences
{
    public AspectReferences(ModuleCache moduleCache)
    {
        var baseAspectNetAttributeTypeReference = moduleCache.ImportReference(typeof(AbstractAspectNetAttribute));
        OnEntry = moduleCache.ImportReference(baseAspectNetAttributeTypeReference, nameof(IAspectNetAttribute.OnEntry), 1);
        OnSuccess = moduleCache.ImportReference(baseAspectNetAttributeTypeReference, nameof(IAspectNetAttribute.OnSuccess), 1);
        OnException = moduleCache.ImportReference(baseAspectNetAttributeTypeReference, nameof(IAspectNetAttribute.OnException), 1);
        OnExit = moduleCache.ImportReference(baseAspectNetAttributeTypeReference, nameof(IAspectNetAttribute.OnExit), 1);
    }

    public MethodReference OnEntry { get; }
    public MethodReference OnSuccess { get; }
    public MethodReference OnException { get; }
    public MethodReference OnExit { get; }
}
