using System.Collections.Immutable;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

/// <summary>
/// Provides extension methods for working with type definitions in Mono.Cecil.
/// </summary>
public static class TypeDefinitionExtensions
{
    public static TypeReference GetRuntimeTypeReference(this TypeDefinition typeDefinition)
    {
        if (!typeDefinition.HasGenericParameters)
            return typeDefinition;

        var genericInstance = new GenericInstanceType(typeDefinition);
        foreach (var parameter in typeDefinition.GenericParameters)
        {
            genericInstance.GenericArguments.Add(parameter);
        }

        return genericInstance;
    }

    /// <summary>
    /// Gets a dictionary mapping methods to their associated aspect attributes from all types in the module.
    /// </summary>
    /// <param name="type">The type definitions to search.</param>
    /// <param name="classAspects"></param>
    /// <param name="baseAspectNetAttribute">The base aspect attribute type to check inheritance against.</param>
    /// <param name="filterAttributes">The attribute types to filter out (e.g., exclusion attributes).</param>
    /// <returns>An immutable dictionary mapping method definitions to arrays of aspect attributes.</returns>
    /// <remarks>
    /// This method aggregates aspect attributes from class-level and method-level declarations,
    /// merging them appropriately and filtering out methods that should be excluded from weaving.
    /// </remarks>
    public static IReadOnlyDictionary<MethodDefinition, CustomAttribute[]> GetMethodsDecoratedWithAspectNetDerivedAttributes(
        this TypeDefinition type,
        CustomAttribute[] classAspects,
        TypeDefinition baseAspectNetAttribute,
        Type[] filterAttributes)
    {
        var filterAttributeFullNames = filterAttributes
            .Select(t => t.FullName)
            .ToArray();

        var methodsAspects = type.Methods.GetMethodLevelAttributes(classAspects, baseAspectNetAttribute, filterAttributeFullNames);
        var propertiesAspects = type.Properties.GetPropertyLevelAttributes(classAspects, baseAspectNetAttribute, filterAttributeFullNames);

        var memberAspects = methodsAspects.Concat(propertiesAspects);
        return memberAspects
            .GroupBy(kvp => kvp.Key)
            .Select(group => new KeyValuePair<MethodDefinition, CustomAttribute[]>(
                group.Key,
                group.SelectMany(x => x.Value)
                    .Distinct(new AttributeInstanceComparer())
                    .ToArray()
            ))
            .Where(kvp => kvp.Value.Length > 0)
            .ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static IMemberDefinition[] GetInheritedMembersToBridge(this TypeDefinition targetType)
    {
        var membersToBridge = new List<IMemberDefinition>();
        var currentBase = targetType.BaseType?.Resolve();

        // Constant for the attribute name to avoid magic strings
        const string ExcludeAttributeName = "AspectNetExcludeAttribute";

        // Set of signatures already defined in the targetType to avoid bridging what's already there
        var existingSignatures = new HashSet<string>(
            targetType.Methods.Select(m => m.FullName.Replace(targetType.FullName, ""))
        );

        while (currentBase != null && currentBase.FullName != "System.Object")
        {
            // 1. Collect Methods
            var inheritedMethods = from method in currentBase.Methods
                where !method.IsPrivate && !method.IsStatic && !method.IsConstructor
                // FILTER: Check if the method has the exclude attribute
                where !method.CustomAttributes.Any(a => a.AttributeType.Name == ExcludeAttributeName)
                let relativeSignature = method.FullName.Replace(currentBase.FullName, "")
                where existingSignatures.Add(relativeSignature)
                select method;

            membersToBridge.AddRange(inheritedMethods.Cast<IMemberDefinition>());

            // 2. Collect Properties
            var inheritedProperties = currentBase.Properties
                .Where(p => (p.GetMethod?.IsPrivate == false) || (p.SetMethod?.IsPrivate == false))
                // FILTER: Check if the property has the exclude attribute
                .Where(p => !p.CustomAttributes.Any(a => a.AttributeType.Name == ExcludeAttributeName))
                .Where(prop => !targetType.Properties.Any(p => p.Name == prop.Name));

            membersToBridge.AddRange(inheritedProperties.Cast<IMemberDefinition>());

            currentBase = currentBase.BaseType?.Resolve();
        }

        return membersToBridge.ToArray();
    }

    public static void MaterializeInheritedBridges(this TypeDefinition targetType, IMemberDefinition[] members)
    {
        if (targetType == null || members == null) return;

        foreach (var member in members)
        {
            if (member is MethodDefinition method)
            {
                MaterializeMethodBridge(targetType, method);
            }
            else if (member is PropertyDefinition prop)
            {
                // Bridge the accessors first
                MethodDefinition getMethod = prop.GetMethod != null ? MaterializeMethodBridge(targetType, prop.GetMethod) : null;
                MethodDefinition setMethod = prop.SetMethod != null ? MaterializeMethodBridge(targetType, prop.SetMethod) : null;

                // Determine the property type from whichever accessor we successfully bridged
                TypeReference propType = getMethod?.ReturnType ?? setMethod?.Parameters.LastOrDefault()?.ParameterType;

                if (propType != null)
                {
                    var newProp = new PropertyDefinition(prop.Name, prop.Attributes, propType)
                    {
                        GetMethod = getMethod,
                        SetMethod = setMethod
                    };
                    targetType.Properties.Add(newProp);
                }
            }
        }
    }

    private static MethodDefinition MaterializeMethodBridge(TypeDefinition targetType, MethodDefinition baseMethod)
    {
        if (targetType?.Module == null || baseMethod == null) return null;

        var module = targetType.Module;
        var baseType = targetType.BaseType;

        // 1. Prepare and Sanitize Attributes
        // We start with the base attributes but MUST remove 'Abstract' 
        // because we are providing a concrete implementation (the bridge).
        MethodAttributes attrs = baseMethod.Attributes;
        attrs &= ~MethodAttributes.Abstract;
        attrs |= MethodAttributes.HideBySig; // Standard for C# methods

        if (baseMethod.IsVirtual)
        {
            attrs &= ~MethodAttributes.NewSlot;
            attrs |= MethodAttributes.ReuseSlot;
        }
        else
        {
            attrs |= MethodAttributes.NewSlot;
            attrs &= ~MethodAttributes.ReuseSlot;
            attrs &= ~MethodAttributes.Virtual;
        }

        // 2. Create the bridge method
        var bridge = new MethodDefinition(baseMethod.Name, attrs, module.TypeSystem.Void);

        // 3. CRITICAL: Ensure the Body is initialized
        // If baseMethod was abstract, Cecil won't create a Body automatically.
        if (bridge.Body == null)
        {
            bridge.Body = new Mono.Cecil.Cil.MethodBody(bridge);
        }

        // Add to targetType immediately to establish context
        targetType.Methods.Add(bridge);

        // 4. Map Generics
        if (baseMethod.HasGenericParameters)
        {
            foreach (var gp in baseMethod.GenericParameters)
                bridge.GenericParameters.Add(new GenericParameter(gp.Name, bridge));
        }

        // 5. Resolve Signature Types
        // (Using the SafeReplace helper from the previous response)
        bridge.ReturnType = SafeReplace(baseMethod.ReturnType, baseType, targetType, bridge, module)
                            ?? module.TypeSystem.Void;

        foreach (var p in baseMethod.Parameters)
        {
            var resolvedType = SafeReplace(p.ParameterType, baseType, targetType, bridge, module);
            bridge.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, resolvedType));
        }

        // 6. Build the Base Call Reference (Preserving raw !0/!!0 tokens)
        var baseMethodRef = new MethodReference(baseMethod.Name, CloneUnmapped(baseMethod.ReturnType, module), baseType)
        {
            HasThis = baseMethod.HasThis,
            ExplicitThis = baseMethod.ExplicitThis,
            CallingConvention = baseMethod.CallingConvention
        };

        foreach (var p in baseMethod.Parameters)
            baseMethodRef.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, CloneUnmapped(p.ParameterType, module)));

        // 7. Generate IL
        var proc = bridge.Body.GetILProcessor();
        proc.Emit(OpCodes.Ldarg_0); // 'this'
        for (int i = 0; i < bridge.Parameters.Count; i++)
            proc.Emit(OpCodes.Ldarg, i + 1);

        proc.Emit(OpCodes.Call, baseMethodRef);
        proc.Emit(OpCodes.Ret);

        return bridge;
    }

    private static TypeReference SafeReplace(TypeReference type, TypeReference baseType, TypeDefinition targetType, MethodDefinition bridge, ModuleDefinition module)
    {
        if (type == null) return module.TypeSystem.Void;

        if (type.IsGenericParameter)
        {
            var gp = (GenericParameter)type;

            // !!0 - Method Level
            if (gp.Type == GenericParameterType.Method)
            {
                return (bridge.HasGenericParameters && gp.Position < bridge.GenericParameters.Count)
                    ? bridge.GenericParameters[gp.Position]
                    : type;
            }

            // !0 - Type Level
            if (gp.Type == GenericParameterType.Type)
            {
                // Map from Base<T> arguments if the base is a generic instance
                if (baseType is GenericInstanceType git1 && gp.Position < git1.GenericArguments.Count)
                {
                    return module.ImportReference(git1.GenericArguments[gp.Position]);
                }

                // Map to Derived<T> parameters
                if (targetType.HasGenericParameters && gp.Position < targetType.GenericParameters.Count)
                {
                    return targetType.GenericParameters[gp.Position];
                }
            }

            return type;
        }

        // Recursive resolution for complex types
        if (type is ArrayType at)
            return new ArrayType(SafeReplace(at.ElementType, baseType, targetType, bridge, module), at.Rank);

        if (type is ByReferenceType brt)
            return new ByReferenceType(SafeReplace(brt.ElementType, baseType, targetType, bridge, module));

        if (type is GenericInstanceType git)
        {
            var instance = new GenericInstanceType(module.ImportReference(git.ElementType));
            foreach (var arg in git.GenericArguments)
                instance.GenericArguments.Add(SafeReplace(arg, baseType, targetType, bridge, module));
            return instance;
        }

        return module.ImportReference(type);
    }

    private static TypeReference CloneUnmapped(TypeReference type, ModuleDefinition module)
    {
        if (type == null) return null;
        if (type.IsGenericParameter) return type;

        if (type.IsArray) return new ArrayType(CloneUnmapped(type.GetElementType(), module), ((ArrayType)type).Rank);
        if (type.IsByReference) return new ByReferenceType(CloneUnmapped(type.GetElementType(), module));

        if (type.IsGenericInstance)
        {
            var git = (GenericInstanceType)type;
            var newGit = new GenericInstanceType(module.ImportReference(git.ElementType));
            foreach (var arg in git.GenericArguments)
                newGit.GenericArguments.Add(CloneUnmapped(arg, module));
            return newGit;
        }

        return module.ImportReference(type);
    }

    /// <summary>
    /// Collects attributes applied directly to methods or inherited from the class.
    /// </summary>
    /// <param name="methods">The collection of methods to analyze.</param>
    /// <param name="classAspects">The aspect attributes defined at the class level.</param>
    /// <param name="baseAspectNetAttribute">The base aspect attribute type.</param>
    /// <param name="filterAttributeFullNames">The full names of attributes to filter out.</param>
    /// <returns>A collection of method-aspect attribute pairs.</returns>
    /// <remarks>
    /// This method processes each method, collecting both method-level and inherited class-level
    /// aspect attributes, while filtering out methods that have exclusion attributes.
    /// </remarks>
    private static IEnumerable<KeyValuePair<MethodDefinition, CustomAttribute[]>> GetMethodLevelAttributes(
        this Collection<MethodDefinition> methods,
        CustomAttribute[] classAspects,
        TypeDefinition baseAspectNetAttribute,
        string[] filterAttributeFullNames)
    {
        return methods
            .Where(m => m.HasBody && !m.CustomAttributes.ContainsFilterAttributes(filterAttributeFullNames))
            .Select(m =>
            {
                var methodAspects = m.CustomAttributes.GetAspectNetDerivedAttributes(baseAspectNetAttribute);
                var merged = methodAspects
                    .Concat(classAspects)
                    .ToArray();

                return new KeyValuePair<MethodDefinition, CustomAttribute[]>(m, merged);
            })
            .Where(kvp => kvp.Value.Length > 0);
    }

    /// <summary>
    /// Collects attributes applied to properties (and inherited class aspects) and maps them to accessors.
    /// </summary>
    /// <param name="properties">The collection of properties to analyze.</param>
    /// <param name="classAspects">The aspect attributes defined at the class level.</param>
    /// <param name="baseAspectNetAttribute">The base aspect attribute type.</param>
    /// <param name="filterAttributeFullNames">The full names of attributes to filter out.</param>
    /// <returns>A collection of method-aspect attribute pairs for property accessors.</returns>
    /// <remarks>
    /// This method processes property accessors (getters and setters), collecting property-level
    /// and inherited class-level aspect attributes, while respecting exclusion filters.
    /// </remarks>
    private static IEnumerable<KeyValuePair<MethodDefinition, CustomAttribute[]>> GetPropertyLevelAttributes(
        this Collection<PropertyDefinition> properties,
        CustomAttribute[] classAspects,
        TypeDefinition baseAspectNetAttribute,
        string[] filterAttributeFullNames)
    {
        return properties
            .Where(p => !p.CustomAttributes.ContainsFilterAttributes(filterAttributeFullNames))
            .SelectMany(p =>
            {
                var propertyAspects = p.CustomAttributes.GetAspectNetDerivedAttributes(baseAspectNetAttribute);

                var accessors = new List<MethodDefinition>();
                if (p.GetMethod != null) accessors.Add(p.GetMethod);
                if (p.SetMethod != null) accessors.Add(p.SetMethod);

                return accessors.Select(method =>
                {
                    if (method.CustomAttributes.ContainsFilterAttributes(filterAttributeFullNames))
                        return new KeyValuePair<MethodDefinition, CustomAttribute[]>(method, []);

                    var methodAspects = method.CustomAttributes.GetAspectNetDerivedAttributes(baseAspectNetAttribute);

                    var merged = methodAspects
                        .Concat(propertyAspects)
                        .Concat(classAspects)
                        .ToArray();

                    return new KeyValuePair<MethodDefinition, CustomAttribute[]>(method, merged);
                });
            })
            .Where(kvp => kvp.Value.Length > 0);
    }
}
