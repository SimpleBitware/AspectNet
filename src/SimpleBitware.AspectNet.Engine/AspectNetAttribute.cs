namespace SimpleBitware.AspectNet.Engine;

public abstract class AspectNetAttribute : Attribute
{
    public int Order { get; init; } = 0;
    
    protected virtual void OnEntry(params object[] args)
    {
    }

    protected virtual void OnExit(params object[] args)
    {
    }

    protected virtual void OnException(Exception exception, params object[] args)
    {
    }
}
