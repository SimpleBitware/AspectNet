namespace SimpleBitware.AspectNet.Abstractions.Context;

public abstract class AbstractAspectNetContext
{
    internal AbstractAspectNetContext() { }
    
    public required string ClassName { get; init; }
    public required string MemberName { get; init; }
    public Dictionary<string, object> Parameters { get; init; } = [];
}
