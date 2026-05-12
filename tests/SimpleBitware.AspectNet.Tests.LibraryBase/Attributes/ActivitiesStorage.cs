using System.Collections.Concurrent;

namespace SimpleBitware.AspectNet.Tests.LibraryBase.Attributes;

public static class ActivitiesStorage
{
    public static readonly ConcurrentDictionary<ActivityKey, List<Activity>> Activities = [];
}
