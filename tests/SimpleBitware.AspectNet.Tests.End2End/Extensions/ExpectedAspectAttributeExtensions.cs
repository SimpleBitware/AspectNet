using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.End2End.Helpers;
using SimpleBitware.AspectNet.Tests.Weaving.Extensions;

namespace SimpleBitware.AspectNet.Tests.End2End.Extensions;

public static class ExpectedAspectAttributeExtensions
{
    public static IEnumerable<ExpectedActivity> GetActivities(this ExpectedAspectAttribute[] expectedAspectAttributes)
    {
        var orderedAspects = expectedAspectAttributes
            .Select((attribute, index) => (Attribute: attribute, Index: index))
            .OrderBy(x => x.Attribute.Priority)
            .ThenBy(x => x.Index)
            .Select(x => x.Attribute)
            .ToArray();

        foreach (var aspect in orderedAspects)
        {
            var context = aspect.Context.PartialDeepCopy();
            context.Exception = null;
            yield return new ExpectedActivity()
            {
                AspectType = aspect.Type,
                AspectPriority = aspect.Priority,
                AspectMethodName = nameof(IAspectNetAttribute.OnEntry),
                Context = context
            };
        }
        
        foreach (var aspect in orderedAspects.Reverse())
        {
            yield return new ExpectedActivity()
            {
                AspectType = aspect.Type,
                AspectPriority = aspect.Priority,
                AspectMethodName = aspect.Context.Exception == null ? nameof(IAspectNetAttribute.OnSuccess) : nameof(IAspectNetAttribute.OnException),
                Context = aspect.Context.PartialDeepCopy(),
            };
            
            yield return new ExpectedActivity()
            {
                AspectType = aspect.Type,
                AspectPriority = aspect.Priority,
                AspectMethodName = nameof(IAspectNetAttribute.OnExit),
                Context = aspect.Context.PartialDeepCopy()
            };
        }
    }

    public static IEnumerable<ExpectedActivity> GetExpectedActivitiesForConstructor(this ExpectedAspectAttribute[] constructorAspectAttributes, ExpectedAspectAttribute[] inheritedAspectAttributes)
    {
        var constructorActivities = constructorAspectAttributes.GetActivities();
        var inheritedActivities = inheritedAspectAttributes.GetActivities();
        return inheritedActivities.Concat(constructorActivities);
    }
}
