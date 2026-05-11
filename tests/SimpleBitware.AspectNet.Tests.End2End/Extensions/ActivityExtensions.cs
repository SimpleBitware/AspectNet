using SimpleBitware.AspectNet.Tests.End2End.Helpers;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End.Extensions;

public static class ActivityExtensions
{
    private static ExpectedActivity ToExpectedActivity(this Activity activity)
    {
        return new ExpectedActivity()
        {
            AspectType = activity.Aspect.GetType(),
            AspectPriority = activity.Aspect.Priority,
            AspectMethodName = activity.AspectMethodName,
            Context = activity.Context
        };
    }

    public static ExpectedActivity[] ToExpectedActivities(this IEnumerable<Activity> activities)
    {
        return activities.Select(activity => activity.ToExpectedActivity()).ToArray();
    }
}
