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
    /// <param name="filterAttributeFullNames">The full names of attributes to filter by.</param>
    /// <returns><c>true</c> if any of the specified attributes are found; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method is used to check if certain attributes (like exclusion attributes) are present
    /// before applying aspect weaving logic.
    /// </remarks>
    public static bool ContainsFilterAttributes(this IList<CustomAttribute> customAttributes, string[] filterAttributeFullNames)
    {
        return customAttributes.Any(a => filterAttributeFullNames.Any(f => f == a.AttributeType.FullName));
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
}
