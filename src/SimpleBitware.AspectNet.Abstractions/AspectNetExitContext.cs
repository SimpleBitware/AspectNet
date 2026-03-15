namespace SimpleBitware.AspectNet.Abstractions;

public sealed class AspectNetExitContext : AbstractAspectNetContext
{
    public object? ReturnValue { get; init; }
}
