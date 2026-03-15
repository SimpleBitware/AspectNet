using Mono.Cecil;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Extensions.Cecil;

namespace SimpleBitware.AspectNet.Runtime.Cecil;

public class AspectReferences
{
    public MethodReference OnEntry { get; }
    public MethodReference OnException { get; }
    public MethodReference OnExit { get; }

    public AspectReferences(ModuleDefinition targetModule, TypeDefinition baseAspectNetAttributeTypeDefinition)
    {
        // We pass the targetModule down to the search helper
        OnEntry = targetModule.FindAndImport(baseAspectNetAttributeTypeDefinition, nameof(AbstractAspectNetAttribute.OnEntry), 1);
        OnException = targetModule.FindAndImport(baseAspectNetAttributeTypeDefinition, nameof(AbstractAspectNetAttribute.OnException), 1);
        OnExit = targetModule.FindAndImport(baseAspectNetAttributeTypeDefinition, nameof(AbstractAspectNetAttribute.OnExit), 1);
    }
}
