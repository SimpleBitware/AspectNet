using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.Attributes;

public record Activity
{
    public required AspectNetAttributeContext Context { get; init; }
    public required string AspectMethodName { get; init; }
    public required IAspectNetAttribute Aspect { get; init; }
}
