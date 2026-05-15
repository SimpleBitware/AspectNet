using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Tests.LibraryBase.Attributes;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, Inherited =  false, AllowMultiple = true)]
public class HideExceptionAttribute : RecordActivityAttribute
{
    public override void OnException(AspectNetAttributeContext context)
    {
        base.OnException(context);
        context.Exception = null;
    }
    
    public override void OnExit(AspectNetAttributeContext context)
    {
        base.OnExit(context);

        var returnValueType = context.ReturnValue?.GetType();
        if (returnValueType == typeof(Task) ||
            returnValueType == typeof(Task<>) ||
            returnValueType == typeof(ValueTask) ||
            returnValueType == typeof(ValueTask<>))
            return;

        context.ReturnValue = null;
    }
}
