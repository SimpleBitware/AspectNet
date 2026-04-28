using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.Attributes;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, Inherited =  false, AllowMultiple = true)]
public class ModifyStateAttribute : RecordActivityAttribute
{
    public override void OnException(AspectNetAttributeContext context)
    {
        context.Exception = null;
        base.OnException(context);
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
