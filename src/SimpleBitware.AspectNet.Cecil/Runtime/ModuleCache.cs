using System.Collections.Concurrent;
using System.Reflection;
using Mono.Cecil;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

/// <summary>
/// Provides a caching layer for Mono.Cecil operations to improve performance during aspect weaving.
/// </summary>
/// <remarks>
/// This class caches type definitions, type references, method references, and inheritance relationships.
/// </remarks>
public class ModuleCache(ModuleDefinition module)
{
    private readonly ModuleDefinition module = module ?? throw new ArgumentNullException(nameof(module));
    private readonly ConcurrentDictionary<string, TypeDefinition> typeDefinitions = new();
    private readonly ConcurrentDictionary<string, TypeReference> typeReferences = new();
    private readonly ConcurrentDictionary<string, MethodReference> methodReferences = new();
    private readonly ConcurrentDictionary<MethodBase, MethodReference> methodBaseReferences = new();
    private readonly ConcurrentDictionary<string, bool> aspectCache = new();
    private readonly ConcurrentDictionary<string, bool> inheritanceCache = new();

    /// <summary>
    /// Resolves a type reference to its type definition, with caching.
    /// </summary>
    /// <param name="typeReference">The type reference to resolve.</param>
    /// <returns>The resolved type definition.</returns>
    /// <exception cref="ArgumentException">Thrown when the type definition cannot be resolved.</exception>
    public TypeDefinition Resolve(TypeReference typeReference)
    {
        if (typeDefinitions.TryGetValue(typeReference.FullName, out var cached))
            return cached;

        var resolvedTypeDefinition = typeReference.Resolve();
        if (resolvedTypeDefinition != null)
            typeDefinitions[typeReference.FullName] = resolvedTypeDefinition;
    
        return resolvedTypeDefinition ?? throw new ArgumentException($"TypeDefinition not found for {typeReference.FullName}");
    }

    /// <summary>
    /// Imports a .NET type as a type reference, with caching.
    /// </summary>
    /// <param name="type">The .NET type to import.</param>
    /// <returns>The imported type reference.</returns>
    /// <exception cref="ArgumentException">Thrown when the type reference cannot be imported.</exception>
    public TypeReference ImportReference(Type type)
    {
        if (typeReferences.TryGetValue(type.FullName, out var cached))
            return cached;
        
        var importedTypeReference = module.ImportReference(type);
        if (importedTypeReference != null)
            typeReferences.TryAdd(importedTypeReference.FullName, importedTypeReference);
    
        return importedTypeReference ?? throw new ArgumentException($"TypeReference not found for {type.FullName}");
    }

    /// <summary>
    /// Imports a type reference, with caching.
    /// </summary>
    /// <param name="typeReference">The type reference to import.</param>
    /// <returns>The imported type reference.</returns>
    /// <exception cref="ArgumentException">Thrown when the type reference cannot be imported.</exception>
    public TypeReference ImportReference(TypeReference typeReference)
    {
        if (typeReferences.TryGetValue(typeReference.FullName, out var cached))
            return cached;
        
        var importedTypeReference = module.ImportReference(typeReference);
        if (importedTypeReference != null)
            typeReferences.TryAdd(importedTypeReference.FullName, importedTypeReference);
    
        return importedTypeReference ?? throw new ArgumentException($"TypeReference not found for {typeReference.FullName}");
    }
    
    /// <summary>
    /// Imports a method base as a method reference, with caching.
    /// </summary>
    /// <param name="method">The method base to import, or null.</param>
    /// <returns>The imported method reference, or null if the input was null.</returns>
    /// <exception cref="ArgumentException">Thrown when the method reference cannot be imported.</exception>
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

    /// <summary>
    /// Imports a method reference from a type definition by name and parameter count.
    /// </summary>
    /// <param name="typeReference">The type reference containing the method.</param>
    /// <param name="name">The name of the method.</param>
    /// <param name="paramCount">The number of parameters the method should have.</param>
    /// <returns>The imported method reference.</returns>
    public MethodReference ImportReference(TypeReference typeReference, string name, int paramCount)
    {
        return ImportReference(Resolve(typeReference), name, paramCount);
    }
    
    /// <summary>
    /// Imports a method reference from a type definition by name and parameter count, with caching.
    /// </summary>
    /// <param name="typeDefinition">The type definition containing the method.</param>
    /// <param name="name">The name of the method.</param>
    /// <param name="paramCount">The number of parameters the method should have.</param>
    /// <returns>The imported method reference.</returns>
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
    
    /// <summary>
    /// Determines whether a type reference represents an aspect type, with caching.
    /// </summary>
    /// <param name="typeReference">The type reference to check.</param>
    /// <param name="baseType">The base aspect type to check inheritance against.</param>
    /// <returns><c>true</c> if the type inherits from the base aspect type; otherwise, <c>false</c>.</returns>
    public bool IsAspect(TypeReference typeReference, TypeDefinition baseType)
    {
        var fullName = typeReference.FullName;
        if (aspectCache.TryGetValue(fullName, out bool result))
            return result;

        result = InheritsFrom(Resolve(typeReference), baseType);
        aspectCache.TryAdd(fullName, result);
        
        return result;
    }

    /// <summary>
    /// Determines whether a type definition inherits from a base type, with caching.
    /// </summary>
    /// <param name="type">The type definition to check.</param>
    /// <param name="baseType">The base type to check inheritance against.</param>
    /// <returns><c>true</c> if the type inherits from the base type; otherwise, <c>false</c>.</returns>
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

        inheritanceCache.TryAdd(type.FullName, result);
        return result;
    }
}
