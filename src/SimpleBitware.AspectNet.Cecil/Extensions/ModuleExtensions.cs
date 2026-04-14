using Mono.Cecil;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

internal static class ModuleExtensions
{
    public static MethodReference FindAndImport(this ModuleDefinition targetModule, TypeDefinition typeDef, string name, int paramCount)
    {
        var method = typeDef.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == paramCount);

        return (method == null && typeDef.BaseType != null)
            ? targetModule.FindAndImport(typeDef.BaseType.Resolve(), name, paramCount)
            : targetModule.ImportReference(method);
    }
}
