using System.Collections.Concurrent;

namespace SimpleBitware.AspectNet.Tests.Weaving.Attributes;

public static class ActivitiesStorage
{
    public static readonly ConcurrentDictionary<ActivityKey, List<Activity>> Activities = [];
}
