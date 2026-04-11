using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.Tests.Weaving.Attributes;

namespace SimpleBitware.Tests.Weaving.Extensions;

public static class AspectNetAttributeContextExtensions
{
    public static ActivityKey GetActivityKey(this AspectNetAttributeContext context) => new ActivityKey(context.ClassType, context.MemberName);
}
