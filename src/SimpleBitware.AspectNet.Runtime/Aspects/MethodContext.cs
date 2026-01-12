namespace SimpleBitware.AspectNet.Runtime.Aspects;

public sealed class MethodContext
{
    public string MethodId { get; init; } = string.Empty;
    public object? Target { get; init; }
    public object?[] Arguments { get; init; } = [];
    public object? ReturnValue { get; set; }
}