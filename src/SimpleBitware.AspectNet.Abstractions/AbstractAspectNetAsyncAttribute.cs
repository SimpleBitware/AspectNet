namespace SimpleBitware.AspectNet.Abstractions;

public abstract class AbstractAspectNetAsyncAttribute : Attribute
{
    public virtual Task OnEntry(AspectNetContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public virtual Task OnExit(AspectNetReturnContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public virtual Task OnException(AspectNetExceptionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
