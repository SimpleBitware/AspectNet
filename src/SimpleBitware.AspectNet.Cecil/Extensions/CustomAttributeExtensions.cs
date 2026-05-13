using Mono.Cecil;
using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

/// <summary>
/// Provides extension methods for working with custom attributes in Mono.Cecil.
/// </summary>
public static class CustomAttributeExtensions
{
    /// <summary>
    /// The default priority value used when no priority is explicitly specified.
    /// </summary>
    private const int DefaultPriority = int.MaxValue;

    /// <summary>
    /// Gets the priority value from a custom attribute that implements <see cref="IAspectNetAttribute.Priority"/>.
    /// </summary>
    /// <param name="attribute">The custom attribute to extract the priority from.</param>
    /// <returns>The priority value, or <see cref="DefaultPriority"/> if not specified.</returns>
    /// <remarks>
    /// This method searches for a property named "Priority" in the attribute's properties
    /// and returns its integer value. If no priority is found, it returns the default maximum value.
    /// </remarks>
    public static int GetPriorityValue(this CustomAttribute attribute)
    {
        var priorityProperty = attribute.Properties
            .FirstOrDefault(p => p.Name == nameof(IAspectNetAttribute.Priority));

        return priorityProperty is { Name: not null, Argument.Value: int value }
            ? value
            : DefaultPriority;
    }

    /// <summary>
    /// Determines whether a collection of custom attributes contains any attributes with the specified full names.
    /// </summary>
    /// <param name="customAttributes">The collection of custom attributes to search.</param>
    /// <param name="excludeFromWeavingAttributes">The full names of attributes to filter by.</param>
    /// <returns><c>true</c> if any of the specified attributes are found; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method is used to check if certain attributes (like exclusion attributes) are present
    /// before applying aspect weaving logic.
    /// </remarks>
    public static bool ContainsFilterAttributes(this IList<CustomAttribute> customAttributes, TypeReference[] excludeFromWeavingAttributes)
    {
        return customAttributes.Any(x => excludeFromWeavingAttributes.Any(a => a.FullName == x.AttributeType.FullName));
    }

    /// <summary>
    /// Gets all custom attributes that are derived from the specified base aspect attribute type.
    /// </summary>
    /// <param name="customAttributes">The collection of custom attributes to filter.</param>
    /// <param name="baseAspectNetAttribute">The base aspect attribute type to check inheritance against.</param>
    /// <returns>An array of custom attributes that derive from the base aspect attribute.</returns>
    /// <remarks>
    /// This method uses the module cache to determine inheritance relationships and filter
    /// attributes that are aspect-related. It's essential for collecting all applicable aspects
    /// for a given method or type.
    /// </remarks>
    public static CustomAttribute[] GetAspectNetDerivedAttributes(this IList<CustomAttribute> customAttributes, TypeDefinition baseAspectNetAttribute)
    {
        return customAttributes
            .Where(customAttribute => baseAspectNetAttribute.Module.Cache().IsAspect(customAttribute.AttributeType, baseAspectNetAttribute))
            .ToArray();
    }

    public static PropertyAssignment[] GetAttributePropertyAssignments(this CustomAttribute attribute)
    {
        var assignments = new List<PropertyAssignment>();
        var attributeType = attribute.AttributeType.Module.Cache().Resolve(attribute.AttributeType);
    
        if (attributeType == null) return assignments.ToArray();

        foreach (var namedArgument in attribute.Properties)
        {
            // Search the hierarchy for the property definition
            var propertyDef = FindPropertyInHierarchy(attributeType, namedArgument.Name);

            if (propertyDef?.SetMethod != null && propertyDef.SetMethod.IsPublic)
            {
                assignments.Add(new PropertyAssignment
                {
                    Setter = attribute.AttributeType.Module.Cache().ImportReference(propertyDef.SetMethod),
                    Value = namedArgument.Argument.Value
                });
            }
        }

        return assignments.ToArray();
    }

    private static PropertyDefinition FindPropertyInHierarchy(TypeDefinition type, string propertyName)
    {
        var currentType = type;

        while (currentType != null)
        {
            // Check if the property exists on the current level
            var prop = currentType.Properties.FirstOrDefault(p => p.Name == propertyName);
            if (prop != null) return prop;

            // Move up to the base type
            // BaseType is a TypeReference, so we must Resolve() it to get a TypeDefinition
            var baseTypeRef = currentType.BaseType;
            if (baseTypeRef == null || baseTypeRef.FullName == "System.Object") 
                break;

            currentType = baseTypeRef.Resolve();
        }

        return null;
    }
}

public class PropertyAssignment
{
    public required MethodReference Setter { get; set; }
    public required object Value { get; set; }
}
