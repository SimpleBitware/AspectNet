using Mono.Cecil;
using SimpleBitware.AspectNet.Abstractions;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class CustomAttributeExtensions
{
    private const int DefaultPriority = 0;

    public static int GetPriorityValue(this CustomAttribute attribute)
    {
        var priorityProperty = attribute.Properties
            .FirstOrDefault(p => p.Name == nameof(AbstractObserverAttribute.Priority));

        return priorityProperty is { Name: not null, Argument.Value: int value }
            ? value
            : DefaultPriority;
    }

    public static bool ContainsFilterAttributes(this IList<CustomAttribute> customAttributes, string[] filterAttributeFullNames)
    {
        return customAttributes.Any(a => filterAttributeFullNames.Any(f => f == a.AttributeType.FullName));
    }

    public static CustomAttribute[] GetAspectNetDerivedAttributes(this IList<CustomAttribute> customAttributes, TypeDefinition baseAspectNetAttribute)
    {
        return customAttributes
            .Where(customAttribute => customAttribute.AttributeType.Resolve()?.InheritsFrom(baseAspectNetAttribute) ?? false)
            .ToArray();
    }
}
