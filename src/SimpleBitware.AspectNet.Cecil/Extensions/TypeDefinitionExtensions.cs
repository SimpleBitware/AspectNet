using System.Collections.Immutable;
using Mono.Cecil;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

public static class TypeDefinitionExtensions
{
    public static IReadOnlyDictionary<MethodDefinition, CustomAttribute[]> GetMethodsDecoratedWithAspectNetDerivedAttributes(
        this IEnumerable<TypeDefinition> moduleTypes,
        TypeDefinition baseAspectNetAttribute,
        Type[] filterAttributes)
    {
        var filterAttributeFullNames = filterAttributes.Select(t => t.FullName).ToArray();
    
        return moduleTypes
            .SelectMany(type =>
                type.GetMethodLevelAttributes(baseAspectNetAttribute, filterAttributeFullNames)
                    .Concat(type.GetPropertyLevelAttributes(baseAspectNetAttribute, filterAttributeFullNames))
            )
            .GroupBy(kvp => kvp.Key)
            .Select(group => new KeyValuePair<MethodDefinition, CustomAttribute[]>(
                group.Key,
                group.SelectMany(x => x.Value)
                    // 1. Remove ONLY identical instances if necessary, 
                    // but keep different instances of the same type.
                    .Distinct() 
                    // 2. EXPLICIT SORT BY PRIORITY
                    .OrderByDescending(GetAttributePriority) 
                    .ToArray()
            ))
            .Where(kvp => kvp.Value.Length > 0)
            .ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static int GetAttributePriority(this CustomAttribute attribute)
    {
        // 1. Check Properties (e.g., [Attr(Priority = 3)])
        var priorityProp = attribute.Properties.FirstOrDefault(p => p.Name == "Priority");
        if (priorityProp is { Name: not null, Argument.Value: not null })
            return Convert.ToInt32(priorityProp.Argument.Value);

        // 2. Check Fields (e.g., if Priority was a public field)
        var priorityField = attribute.Fields.FirstOrDefault(f => f.Name == "Priority");
        return priorityField is { Name: not null, Argument.Value: not null } 
            ? Convert.ToInt32(priorityField.Argument.Value) 
            : int.MaxValue; //Since you defined it as int.MaxValue in C#, use that here.
    }

    /// <summary>
    /// Collects attributes applied directly to methods or inherited from the class.
    /// </summary>
    private static IEnumerable<KeyValuePair<MethodDefinition, CustomAttribute[]>> GetMethodLevelAttributes(
        this TypeDefinition type,
        TypeDefinition baseAspectNetAttribute,
        string[] filterAttributeFullNames)
    {
        var classAspects = type.CustomAttributes
            .Where(a => a.AttributeType.Resolve()?.InheritsFrom(baseAspectNetAttribute) == true)
            .ToList();

        return type.Methods
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
    private static IEnumerable<KeyValuePair<MethodDefinition, CustomAttribute[]>> GetPropertyLevelAttributes(
        this TypeDefinition type,
        TypeDefinition baseAspectNetAttribute,
        string[] filterAttributeFullNames)
    {
        var classAspects = type.CustomAttributes
            .Where(a => a.AttributeType.Resolve()?.InheritsFrom(baseAspectNetAttribute) == true)
            .ToList();

        return type.Properties
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
                        .Union(propertyAspects, new AttributeTypeComparer())
                        .Concat(classAspects)
                        .ToArray();

                    return new KeyValuePair<MethodDefinition, CustomAttribute[]>(method, merged);
                });
            })
            .Where(kvp => kvp.Value.Length > 0);
    }

    public static bool InheritsFrom(this TypeDefinition? type, TypeDefinition baseType)
    {
        if (type == null) return false;
        if (type.FullName == baseType.FullName) return true;
        
        // Check Interfaces
        if (type.Interfaces.Any(i => i.InterfaceType.FullName == baseType.FullName || 
                                     i.InterfaceType.Resolve()?.InheritsFrom(baseType) == true))
            return true;

        // Check Base Class
        try 
        {
            return type.BaseType?.Resolve()?.InheritsFrom(baseType) ?? false;
        }
        catch 
        {
            return false; // Handle cases where base type cannot be resolved
        }
    }
}

