using System.Collections.Concurrent;
using Mono.Cecil;
using SimpleBitware.AspectNet.Cecil.Extensions;

namespace SimpleBitware.AspectNet.Cecil.Helpers;

public class ModuleCache(ModuleDefinition module)
{
    private readonly ModuleDefinition module = module ?? throw new ArgumentNullException(nameof(module));
    private readonly ConcurrentDictionary<string, MethodReference> methodReferences = new();
    private readonly ConcurrentDictionary<string, TypeDefinition> typeDefinitions = new();
    private readonly ConcurrentDictionary<string, bool> aspectCache = new();
    private readonly ConcurrentDictionary<string, bool> inheritanceCache = new();

    public TypeDefinition ResolveAndCache(TypeReference typeReference)
    {
        if (typeDefinitions.TryGetValue(typeReference.FullName, out var cached))
            return cached;

        var resolvedTypeDefinition = typeReference.Resolve();
        if (resolvedTypeDefinition != null)
            typeDefinitions[typeReference.FullName] = resolvedTypeDefinition;
    
        return resolvedTypeDefinition ?? throw new ArgumentException($"TypeDefinition not found for {typeReference.FullName}");
    }

    public MethodReference ImportAndCache(TypeDefinition typeDefinition, string name, int paramCount)
    {
        var key = $"{typeDefinition.FullName}:{name}({paramCount})";
        if(methodReferences.TryGetValue(key, out var cachedMethodReference))
            return cachedMethodReference;
        
        var method = typeDefinition.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == paramCount);
        
        var methodReference = (method == null && typeDefinition.BaseType != null)
            ? ImportAndCache(ResolveAndCache(typeDefinition.BaseType), name, paramCount)
            : module.ImportReference(method);
        
        methodReferences.TryAdd(key, methodReference);
        return methodReference;
    }
    
    public bool IsAspect(TypeReference typeReference, TypeDefinition baseType)
    {
        var fullName = typeReference.FullName;
        if (aspectCache.TryGetValue(fullName, out bool result))
            return result;

        result = InheritsFrom(ResolveAndCache(typeReference), baseType);
        aspectCache.TryAdd(fullName, result);
        
        return result;
    }
    
    public bool InheritsFrom(TypeDefinition? type, TypeDefinition baseType)
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
        if (type.Interfaces.Any(i => i.InterfaceType.FullName == baseType.FullName || InheritsFrom(ResolveAndCache(i.InterfaceType), baseType)))
            return inheritanceCache.TryAdd(type.FullName, true);

        // 4. Check Base Class (Recursive)
        try
        {
            var resolvedBase = ResolveAndCache(type.BaseType);
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
