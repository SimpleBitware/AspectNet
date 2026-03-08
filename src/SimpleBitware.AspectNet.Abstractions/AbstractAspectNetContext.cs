namespace SimpleBitware.AspectNet.Abstractions;

public abstract class AbstractAspectNetContext
{
    internal AbstractAspectNetContext() { }
    
    public required string ClassName { get; set; }
    public required string MemberName { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];
}
