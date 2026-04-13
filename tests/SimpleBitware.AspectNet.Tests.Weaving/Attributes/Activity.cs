using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.Attributes;

public record Activity
{
    public required AspectNetAttributeContext Context { get; init; }
    public required int Priority { get; init; }
    public required string AspectMethodName { get; init; }
}
