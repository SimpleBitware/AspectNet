using System.Collections.Immutable;
using Mono.Cecil;
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
    /// <param name="moduleTypes">The collection of type definitions to search.</param>
    /// <param name="baseAspectNetAttribute">The base aspect attribute type to check inheritance against.</param>
    /// <param name="filterAttributes">The attribute types to filter out (e.g., exclusion attributes).</param>
    /// <returns>An immutable dictionary mapping method definitions to arrays of aspect attributes.</returns>
    /// <remarks>
    /// This method aggregates aspect attributes from class-level and method-level declarations,
    /// merging them appropriately and filtering out methods that should be excluded from weaving.
    /// </remarks>
    public static IReadOnlyDictionary<MethodDefinition, CustomAttribute[]> GetMethodsDecoratedWithAspectNetDerivedAttributes(
        this IEnumerable<TypeDefinition> moduleTypes,
        TypeDefinition baseAspectNetAttribute,
        Type[] filterAttributes)
    {
        var filterAttributeFullNames = filterAttributes
            .Select(t => t.FullName)
            .ToArray();

        return moduleTypes
            .SelectMany(type =>
                {
                    var classAspects = type.CustomAttributes.GetAspectNetDerivedAttributes(baseAspectNetAttribute);
                    return type.Methods.GetMethodLevelAttributes(classAspects, baseAspectNetAttribute, filterAttributeFullNames)
                        .Concat(type.Properties.GetPropertyLevelAttributes(classAspects, baseAspectNetAttribute, filterAttributeFullNames));
                }
            )
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
