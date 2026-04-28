using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.End2End.Helpers;

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
            yield return new ExpectedActivity()
            {
                AspectType = aspect.Type,
                AspectPriority = aspect.Priority,
                AspectMethodName = nameof(IAspectNetAttribute.OnEntry),
                Context = aspect.Context
            };
        }
        
        foreach (var aspect in orderedAspects.Reverse())
        {
            yield return new ExpectedActivity()
            {
                AspectType = aspect.Type,
                AspectPriority = aspect.Priority,
                AspectMethodName = aspect.Context.Exception == null ? nameof(IAspectNetAttribute.OnSuccess) : nameof(IAspectNetAttribute.OnException),
                Context = aspect.Context
            };
            
            yield return new ExpectedActivity()
            {
                AspectType = aspect.Type,
                AspectPriority = aspect.Priority,
                AspectMethodName = nameof(IAspectNetAttribute.OnExit),
                Context = aspect.Context
            };
        }
    } 
}
