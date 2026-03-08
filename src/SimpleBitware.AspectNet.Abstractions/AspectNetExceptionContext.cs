namespace SimpleBitware.AspectNet.Abstractions;

public sealed class AspectNetExceptionContext : AbstractAspectNetContext
{
    public required Exception Exception { get; init; }
}
