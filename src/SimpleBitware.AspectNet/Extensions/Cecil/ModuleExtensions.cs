using Mono.Cecil;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

internal static class ModuleExtensions
{
    public static MethodReference FindAndImport(this ModuleDefinition targetModule, TypeDefinition typeDef, string name, int paramCount)
    {
        var method = typeDef.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == paramCount);

        return (method == null && typeDef.BaseType != null)
            ? targetModule.FindAndImport(typeDef.BaseType.Resolve(), name, paramCount)
            : targetModule.ImportReference(method);
    }
    
    public static MethodReference GetPropertyGetMethodReference<T>(this ModuleDefinition module, string methodName)
    {
        var propertyName = typeof(T).GetProperty(methodName) ?? throw new ArgumentException($"Method {methodName} of type {typeof(T).FullName} not found");
        return module.ImportReference(propertyName.GetMethod);
    }
    
    public static MethodReference GetPropertySetMethodReference<T>(this ModuleDefinition module, string methodName)
    {
        var propertyName = typeof(T).GetProperty(methodName) ?? throw new ArgumentException($"Method {methodName} of type {typeof(T).FullName} not found");
        return module.ImportReference(propertyName.SetMethod);
    }
}
