using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class ModuleDefinitionExtensions
{
    public static MethodReference FindMethod(this ModuleDefinition module, TypeReference typeReference, string name, int paramCount)
    {
        var type = typeReference.Resolve();

        while (type != null)
        {
            var methodDefinition = type.Methods
                .FirstOrDefault(m => m.Name == name && m.Parameters.Count == paramCount);

            if (methodDefinition != null)
                return module.ImportReference(methodDefinition);

            type = type.BaseType?.Resolve();
        }

        throw new InvalidOperationException($"Method {name} not found on {typeReference.FullName}.");
    }
}