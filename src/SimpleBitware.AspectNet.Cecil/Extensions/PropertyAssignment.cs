using Mono.Cecil;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

public class PropertyAssignment
{
    public required MethodReference Setter { get; init; }
    public required object Value { get; init; }
}
