using Mono.Cecil;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Extensions.Cecil;

namespace SimpleBitware.AspectNet.Runtime.Cecil;

public class AspectReferences
{
    public MethodReference OnEntry { get; }
    public MethodReference OnSuccess { get; }
    public MethodReference OnException { get; }
    public MethodReference OnExit { get; }

    public AspectReferences(ModuleDefinition targetModule, TypeDefinition baseAspectNetAttributeTypeDefinition)
    {
        OnEntry = targetModule.FindAndImport(baseAspectNetAttributeTypeDefinition, nameof(IAspectNetAttribute.OnEntry), 1);
        OnSuccess = targetModule.FindAndImport(baseAspectNetAttributeTypeDefinition, nameof(IAspectNetAttribute.OnSuccess), 1);
        OnException = targetModule.FindAndImport(baseAspectNetAttributeTypeDefinition, nameof(IAspectNetAttribute.OnException), 1);
        OnExit = targetModule.FindAndImport(baseAspectNetAttributeTypeDefinition, nameof(IAspectNetAttribute.OnExit), 1);
    }
}
