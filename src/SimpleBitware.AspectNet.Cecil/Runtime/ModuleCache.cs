using System.Collections.Concurrent;
using System.Reflection;
using Mono.Cecil;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

public class ModuleCache(ModuleDefinition module)
{
    private readonly ModuleDefinition module = module ?? throw new ArgumentNullException(nameof(module));
    private readonly ConcurrentDictionary<string, TypeDefinition> typeDefinitions = new();
    private readonly ConcurrentDictionary<string, TypeReference> typeReferences = new();
    private readonly ConcurrentDictionary<string, MethodReference> methodReferences = new();
    private readonly ConcurrentDictionary<MethodBase, MethodReference> methodBaseReferences = new();
    private readonly ConcurrentDictionary<string, bool> aspectCache = new();
    private readonly ConcurrentDictionary<string, bool> inheritanceCache = new();

    public TypeDefinition Resolve(TypeReference typeReference)
    {
        if (typeDefinitions.TryGetValue(typeReference.FullName, out var cached))
            return cached;

        var resolvedTypeDefinition = typeReference.Resolve();
        if (resolvedTypeDefinition != null)
            typeDefinitions[typeReference.FullName] = resolvedTypeDefinition;
    
        return resolvedTypeDefinition ?? throw new ArgumentException($"TypeDefinition not found for {typeReference.FullName}");
    }

    public TypeReference ImportReference(Type type)
    {
        if (typeReferences.TryGetValue(type.FullName, out var cached))
            return cached;
        
        var importedTypeReference = module.ImportReference(type);
        if (importedTypeReference != null)
            typeReferences.TryAdd(importedTypeReference.FullName, importedTypeReference);
    
        return importedTypeReference ?? throw new ArgumentException($"TypeReference not found for {type.FullName}");
    }

    public TypeReference ImportReference(TypeReference typeReference)
    {
        if (typeReferences.TryGetValue(typeReference.FullName, out var cached))
            return cached;
        
        var importedTypeReference = module.ImportReference(typeReference);
        if (importedTypeReference != null)
            typeReferences.TryAdd(importedTypeReference.FullName, importedTypeReference);
    
        return importedTypeReference ?? throw new ArgumentException($"TypeReference not found for {typeReference.FullName}");
    }
    
    public MethodReference? ImportReference(MethodBase? method)
    {
        if (method is null)
            return null;
        
        if (methodBaseReferences.TryGetValue(method, out var cached))
            return cached;
        
        var importedMethodReference = module.ImportReference(method);
        if (importedMethodReference != null)
            methodBaseReferences.TryAdd(method, importedMethodReference);
    
        return importedMethodReference ?? throw new ArgumentException($"MethodReference not found for {method.Name} in class {method.DeclaringType?.FullName ?? "unknown"}");
    }

    public MethodReference ImportReference(TypeReference typeReference, string name, int paramCount)
    {
        return ImportReference(Resolve(typeReference), name, paramCount);
    }
    
    public MethodReference ImportReference(TypeDefinition typeDefinition, string name, int paramCount)
    {
        var key = $"{typeDefinition.FullName}:{name}({paramCount})";
        if(methodReferences.TryGetValue(key, out var cachedMethodReference))
            return cachedMethodReference;
        
        var method = typeDefinition.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == paramCount);
        
        var methodReference = (method == null && typeDefinition.BaseType != null)
            ? ImportReference(Resolve(typeDefinition.BaseType), name, paramCount)
            : module.ImportReference(method);
        
        methodReferences.TryAdd(key, methodReference);
        return methodReference;
    }
    
    public bool IsAspect(TypeReference typeReference, TypeDefinition baseType)
    {
        var fullName = typeReference.FullName;
        if (aspectCache.TryGetValue(fullName, out bool result))
            return result;

        result = InheritsFrom(Resolve(typeReference), baseType);
        aspectCache.TryAdd(fullName, result);
        
        return result;
    }

    private bool InheritsFrom(TypeDefinition? type, TypeDefinition baseType)
    {
        if (type == null) 
            return false;
    
        // 1. Quick identity check
        if (type.FullName == baseType.FullName) 
            return true;

        // 2. Cache Lookup
        if (inheritanceCache.TryGetValue(type.FullName, out var result))
            return result;

        // 3. Check Interfaces
        if (type.Interfaces.Any(i => i.InterfaceType.FullName == baseType.FullName || InheritsFrom(Resolve(i.InterfaceType), baseType)))
        {
            inheritanceCache.TryAdd(type.FullName, true);
            return true;
        }

        // 4. Check Base Class (Recursive)
        try
        {
            var resolvedBase = Resolve(type.BaseType);
            result = InheritsFrom(resolvedBase, baseType);
        }
        catch
        {
            result = false; 
        }

        // 5. Store and Return
        inheritanceCache.TryAdd(type.FullName, result);
        return result;
    }
}
