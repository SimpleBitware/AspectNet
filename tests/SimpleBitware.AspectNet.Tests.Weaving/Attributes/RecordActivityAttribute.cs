using System.Collections.Concurrent;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving.Extensions;

namespace SimpleBitware.AspectNet.Tests.Weaving.Attributes;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, Inherited =  false)]
public class RecordActivityAttribute : AbstractAspectNetAttribute
{
    public static readonly ConcurrentDictionary<ActivityKey, List<object>> Activities = [];
    
    public override void OnEntry(AspectNetAttributeContext context)
    {
        var key = context.GetActivityKey();
        
        if(!Activities.ContainsKey(key))
            Activities.TryAdd(key, []);
        
        Activities[key].Add(context);
    }

    public override void OnExit(AspectNetAttributeContext context)
    {
        var key = context.GetActivityKey();
        Activities[key].Add(context);
    }

    public override void OnException(AspectNetAttributeContext context)
    {
        var key = context.GetActivityKey();
        Activities[key].Add(context);
    }
}
