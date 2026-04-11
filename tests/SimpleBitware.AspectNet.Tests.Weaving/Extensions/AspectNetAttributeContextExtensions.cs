using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.Extensions;

public static class AspectNetAttributeContextExtensions
{
    public static ActivityKey GetActivityKey(this AspectNetAttributeContext context) => new ActivityKey(context.ClassType, context.MemberName, context.Parameters.Count);
}
