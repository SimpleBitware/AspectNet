using Mono.Cecil;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class ModuleExtensions
{
    // public static MethodReference FindMethod(this ModuleDefinition module, TypeDefinition typeDefinition, string name, int paramCount)
    // {
    //     var currentTypeDefinition = typeDefinition;
    //
    //     while (currentTypeDefinition != null)
    //     {
    //         var methodDefinition = currentTypeDefinition.Methods
    //             .FirstOrDefault(m => m.Name == name && m.Parameters.Count == paramCount);
    //
    //         if (methodDefinition != null)
    //             return module.ImportReference(methodDefinition);
    //
    //         currentTypeDefinition = currentTypeDefinition.BaseType?.Resolve();
    //     }
    //
    //     throw new InvalidOperationException($"Method {name} with {paramCount} parameter(s) not found for {typeDefinition.FullName} type.");
    // }

    public static MethodReference FindAndImport(this ModuleDefinition targetModule, TypeDefinition typeDef, string name, int paramCount)
    {
        var method = typeDef.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == paramCount);

        return (method == null && typeDef.BaseType != null)
            ? targetModule.FindAndImport(typeDef.BaseType.Resolve(), name, paramCount)
            : targetModule.ImportReference(method);
    }
}
