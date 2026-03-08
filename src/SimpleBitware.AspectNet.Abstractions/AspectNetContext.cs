namespace SimpleBitware.AspectNet.Abstractions;

public sealed class AspectNetContext
{
    public string ClassName { get; set; }
    public string MemberName { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];
    public object? ReturnValue { get; set; }
}
