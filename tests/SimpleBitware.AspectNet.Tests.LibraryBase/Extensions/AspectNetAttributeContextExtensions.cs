using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.LibraryBase.Attributes;

namespace SimpleBitware.AspectNet.Tests.LibraryBase.Extensions;

public static class AspectNetAttributeContextExtensions
{
    public static ActivityKey GetActivityKey(this AspectNetAttributeContext context) => new(context.ClassType, context.MemberName, context.Parameters?.Count ?? 0);
    
    public static AspectNetAttributeContext PartialDeepCopy(this AspectNetAttributeContext context)
    {
        return new AspectNetAttributeContext()
        {
            ClassType = context.ClassType,
            MemberName = context.MemberName,
            Parameters = context.Parameters.DeepCopy(),
            ReturnValue = context.ReturnValue,
            Exception = context.Exception?.NewInstance(),
            Instance = context.Instance,
            Data = context.Data?.DeepCopy()
        };
    }
}
