using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End.Helpers;

public record ExpectedActivity
{
    public required Type AspectType { get; init; }
    public required int AspectPriority { get; init; }
    public required string AspectMethodName { get; init; }
    public required AspectNetAttributeContext Context { get; init; }
}
