namespace SimpleBitware.AspectNet.Abstractions;

public sealed class AspectNetReturnContext : AbstractAspectNetContext
{
    public object? ReturnValue { get; set; }
}
