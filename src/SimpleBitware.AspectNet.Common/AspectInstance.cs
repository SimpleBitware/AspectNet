using Microsoft.CodeAnalysis;
using SimpleBitware.AspectNet.Abstractions;

namespace SimpleBitware.AspectNet.Common;

/// <summary>
/// One concrete aspect instance applied to a member.
/// </summary>
public sealed record AspectInstance(AttributeData Attribute, string InstanceName)
{
    private const string AspectNetAttributeOrderMethodName = nameof(AspectNetAttribute.Order);
    
    /// <summary>
    /// Gets the attribute Order value otherwise returns 0.
    /// </summary>
    /// <returns>Attribute Order value.</returns>
    public int GetOrder()
    {
        var namedTypeSymbol = Attribute.AttributeClass;
        if (namedTypeSymbol is null)
            return 0;

        foreach (var member in namedTypeSymbol.GetMembers(AspectNetAttributeOrderMethodName))
        {
            if (member is not IPropertySymbol propertySymbol)
                continue;

            if (propertySymbol.Type.SpecialType != SpecialType.System_Int32)
                continue;

            foreach (var kvp in Attribute.NamedArguments)
            {
                if (kvp is { Key: AspectNetAttributeOrderMethodName, Value.Value: int v })
                    return v;
            }

            return 0;
        }

        return 0;
    }
}
