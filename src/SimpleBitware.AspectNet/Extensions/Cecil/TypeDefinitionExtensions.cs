using System.Collections.Immutable;
using Mono.Cecil;
using SimpleBitware.AspectNet.Runtime.Cecil;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class TypeDefinitionExtensions
{
    public static IReadOnlyDictionary<MethodDefinition, CustomAttribute[]> GetModuleTypesWithAspectNetDerivedAttributes(
        this IEnumerable<TypeDefinition> moduleTypes,
        TypeDefinition baseAspectNetAttribute,
        Type[] filterAttributes)
    {
        var filterAttributeFullNames = filterAttributes.Select(t => t.FullName).ToArray();
        return moduleTypes
            .SelectMany(x => x.GetTypeAspectNetDerivedAttributes(baseAspectNetAttribute, filterAttributeFullNames))
            .ToImmutableDictionary();
    }

    /// <summary>
    /// Gets type's methods/constructors/properties decorated with aspect derived attributes.
    /// Attributes hierarchy resolution (Method > Property > Class)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="baseAspectNetAttribute"></param>
    /// <param name="filterAttributeFullNames"></param>
    /// <returns></returns>
    private static IReadOnlyDictionary<MethodDefinition, CustomAttribute[]> GetTypeAspectNetDerivedAttributes(
        this TypeDefinition type, 
        TypeDefinition baseAspectNetAttribute, 
        string[] filterAttributeFullNames)
    {
        var classAspectNetAttributes = type.CustomAttributes
            .Where(a => a.AttributeType.Resolve()?.InheritsFrom(baseAspectNetAttribute) ?? false)
            .ToList();
        var typeProperties = type.Properties.ToArray();

        return type.Methods
            .Where(x => x.HasBody && !x.CustomAttributes.Any(a => filterAttributeFullNames.Any(f => f == a.AttributeType.FullName)))
            .Select(x => new KeyValuePair<MethodDefinition, CustomAttribute[]>(
                x,
                x.GetMethodAspectNetDerivedAttributes(baseAspectNetAttribute)
                    // Merge property aspects into method aspects (Method wins on duplicates)
                    .Union(x.GetPropertyAspectNetDerivedAttributes(typeProperties, baseAspectNetAttribute, filterAttributeFullNames))
                    // Merge class aspects into method and property aspects (Method and property wins on duplicates)
                    .Union(classAspectNetAttributes, new AttributeTypeComparer())
                    .ToArray())
            )
            .ToImmutableDictionary();
    }

    public static bool InheritsFrom(this TypeDefinition? type, TypeDefinition baseType)
    {
        var currentType = type;
        while (currentType != null)
        {
            if (currentType.FullName == baseType.FullName)
                return true;

            currentType = currentType.BaseType?.Resolve();
        }

        return false;
    }
    
    public static bool DerivesFrom(this TypeDefinition? type, string targetBaseTypeName)
    {
        while (type != null)
        {
            // Check current base type
            if (type.BaseType != null)
            {
                if (type.BaseType.Name == targetBaseTypeName || type.BaseType.FullName == targetBaseTypeName)
                {
                    return true;
                }

                // Move up the inheritance chain
                try
                {
                    type = type.BaseType.Resolve();
                }
                catch
                {
                    // Assembly resolution failed (e.g., missing a reference)
                    return false;
                }
            }
            else
            {
                // Reached 'object' or a type with no base
                break;
            }
        }
        return false;
    }
}
