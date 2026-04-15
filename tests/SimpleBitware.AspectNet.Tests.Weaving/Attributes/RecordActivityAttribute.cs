using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving.Extensions;

namespace SimpleBitware.AspectNet.Tests.Weaving.Attributes;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, Inherited =  false, AllowMultiple = true)]
public class RecordActivityAttribute : AbstractAspectNetAttribute
{
    public override void OnEntry(AspectNetAttributeContext context)
    {
        var key = context.GetActivityKey();
        
        if(!ActivitiesStorage.Activities.ContainsKey(key))
            ActivitiesStorage.Activities.TryAdd(key, []);

        var activity = new Activity()
        {
            Context = context,
            Priority = this.Priority,
            AspectMethodName = nameof(OnEntry)
        };
        ActivitiesStorage.Activities[key].Add(activity);
    }

    public override void OnSuccess(AspectNetAttributeContext context)
    {
        var key = context.GetActivityKey();
        var activity = new Activity()
        {
            Context = context,
            Priority = this.Priority,
            AspectMethodName = nameof(OnSuccess)
        };
        ActivitiesStorage.Activities[key].Add(activity);
    }
    
    public override void OnExit(AspectNetAttributeContext context)
    {
        var key = context.GetActivityKey();
        var activity = new Activity()
        {
            Context = context,
            Priority = this.Priority,
            AspectMethodName = nameof(OnExit)
        };
        ActivitiesStorage.Activities[key].Add(activity);
    }

    public override void OnException(AspectNetAttributeContext context)
    {
        var key = context.GetActivityKey();
        var activity = new Activity()
        {
            Context = context,
            Priority = this.Priority,
            AspectMethodName = nameof(OnException)
        };
        ActivitiesStorage.Activities[key].Add(activity);
    }
}
