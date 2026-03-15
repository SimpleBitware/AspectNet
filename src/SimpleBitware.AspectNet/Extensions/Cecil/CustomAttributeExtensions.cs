using Mono.Cecil;
using SimpleBitware.AspectNet.Abstractions;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class CustomAttributeExtensions
{
    private const int DefaultPriority = 0;
    
    public static int GetPriorityValue(this CustomAttribute attribute)
    {
        var priorityProperty = attribute.Properties
            .FirstOrDefault(p => p.Name == nameof(AbstractAspectNetAttribute.Priority));

        return priorityProperty is { Name: not null, Argument.Value: int value } 
            ? value 
            : DefaultPriority;
    }
}
