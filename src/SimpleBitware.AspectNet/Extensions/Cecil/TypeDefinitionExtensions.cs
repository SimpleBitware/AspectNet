using System.Collections.Immutable;
using Mono.Cecil;
using SimpleBitware.AspectNet.Runtime.Cecil;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class TypeDefinitionExtensions
{
    public static IReadOnlyDictionary<MethodDefinition, CustomAttribute[]> GetMethodsDecoratedWithAspectNetDerivedAttributes(
        this IEnumerable<TypeDefinition> moduleTypes,
        TypeDefinition baseAspectNetAttribute,
        Type[] filterAttributes)
    {
        var filterAttributeFullNames = filterAttributes.Select(t => t.FullName).ToArray();
        return moduleTypes
            .SelectMany(x =>
                x.GetMethodsDecoratedWithAspectNetDerivedAttributes(baseAspectNetAttribute, filterAttributeFullNames)
                    .Concat(x.GetPropertiesDecoratedWithAspectNetDerivedAttributes(baseAspectNetAttribute, filterAttributeFullNames))
            )
            .ToImmutableDictionary();
    }

    /// <summary>
    /// Gets type's methods/constructors decorated with aspect derived attributes.
    /// Attributes hierarchy resolution (Method > Class)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="baseAspectNetAttribute"></param>
    /// <param name="filterAttributeFullNames"></param>
    /// <returns></returns>
    private static IReadOnlyDictionary<MethodDefinition, CustomAttribute[]> GetMethodsDecoratedWithAspectNetDerivedAttributes(
        this TypeDefinition type,
        TypeDefinition baseAspectNetAttribute,
        string[] filterAttributeFullNames)
    {
        var classAspectNetAttributes = type.CustomAttributes
            .Where(a => a.AttributeType.Resolve().InheritsFrom(baseAspectNetAttribute))
            .ToList();

        return type.Methods
            .Where(x => x.HasBody && !x.CustomAttributes.ContainsFilterAttributes(filterAttributeFullNames))
            .Select(x => new KeyValuePair<MethodDefinition, CustomAttribute[]>(
                x,
                x.GetMethodAspectNetDerivedAttributes(baseAspectNetAttribute)
                    // Merge class aspects into method aspects (Method wins on duplicates)
                    .Union(classAspectNetAttributes, new AttributeTypeComparer())
                    .ToArray())
            )
            .Where(x => x.Value.Any())
            .ToImmutableDictionary();
    }

    /// <summary>
    /// Gets type's properties decorated with aspect derived attributes.
    /// Attributes hierarchy resolution (Property > Class)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="baseAspectNetAttribute"></param>
    /// <param name="filterAttributeFullNames"></param>
    /// <returns></returns>
    private static IReadOnlyDictionary<MethodDefinition, CustomAttribute[]> GetPropertiesDecoratedWithAspectNetDerivedAttributes(
        this TypeDefinition type,
        TypeDefinition baseAspectNetAttribute,
        string[] filterAttributeFullNames)
    {
        var classAspectNetAttributes = type.CustomAttributes
            .Where(a => a.AttributeType.Resolve().InheritsFrom(baseAspectNetAttribute))
            .ToList();

        return type.Properties
            .Where(x => !x.CustomAttributes.ContainsFilterAttributes(filterAttributeFullNames))
            .Select(x =>
            {
                var propertyAspectNetDerivedAttributes = x.CustomAttributes.GetAspectNetDerivedAttributes(baseAspectNetAttribute);
                return new Dictionary<MethodDefinition, CustomAttribute[]>()
                    {
                        [x.GetMethod] = x.GetMethod.CustomAttributes.ContainsFilterAttributes(filterAttributeFullNames)
                            ? []
                            : x.GetMethod.CustomAttributes.GetAspectNetDerivedAttributes(baseAspectNetAttribute)
                                .Union(propertyAspectNetDerivedAttributes, new AttributeTypeComparer())
                                // Merge class aspects into property aspects (Property wins on duplicates)
                                .Union(classAspectNetAttributes, new AttributeTypeComparer())
                                .ToArray(),
                        [x.SetMethod] = x.GetMethod.CustomAttributes.ContainsFilterAttributes(filterAttributeFullNames)
                            ? []
                            : x.SetMethod.CustomAttributes.GetAspectNetDerivedAttributes(baseAspectNetAttribute)
                                .Union(propertyAspectNetDerivedAttributes, new AttributeTypeComparer())
                                // Merge class aspects into property aspects (Property wins on duplicates)
                                .Union(classAspectNetAttributes, new AttributeTypeComparer())
                                .ToArray()
                    }
                    .Select(p => p);
            })
            .SelectMany(x => x)
            .Where(x => x.Value.Any())
            .ToImmutableDictionary();
    }

    public static bool InheritsFrom(this TypeDefinition? type, TypeDefinition baseType)
    {
        if (type == null)
            return false;

        if (type.FullName == baseType.FullName)
            return true;
        
        if(type.Interfaces.Any(x=> x.InterfaceType.FullName == baseType.FullName
                                   || x.InterfaceType.Resolve().InheritsFrom(baseType)))
            return true;

        return type.BaseType?.Resolve().InheritsFrom(baseType) ?? false;
    }
}
