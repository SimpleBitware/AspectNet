using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.End2End.Helpers;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ModifyStateTests
{
    [Test]
    public void Should_Modify_Return_Value()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassForTestingModifyState), MemberNameHelper.PropertyGetterName(nameof(ClassForTestingModifyState.Value)));
        
        //when
        var value = ClassForTestingModifyState.Value;
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                (string)a.Context.ReturnValue! == value &&
                a.Context.Exception is null &&
                a.Context.MemberName == MemberNameHelper.PropertyGetterName(nameof(ClassForTestingModifyState.Value)) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance is null
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
    
    [Test]
    public void Should_Modify_Exception()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassForTestingModifyState), nameof(ClassForTestingModifyState.MethodWithException));
        
        //when
        var value = ClassForTestingModifyState.MethodWithException();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                (string)a.Context.ReturnValue! == value &&
                a.Context.Exception is null &&
                a.Context.MemberName == nameof(ClassForTestingModifyState.MethodWithException) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance is null
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnException)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
    
    [Test]
    public async Task Should_Modify_Exception_In_Async_Methods()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassForTestingModifyState), nameof(ClassForTestingModifyState.AsyncMethodWithSyncException));
        
        //when
        await ClassForTestingModifyState.AsyncMethodWithSyncException();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is null &&
                a.Context.MemberName == nameof(ClassForTestingModifyState.AsyncMethodWithSyncException) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance is null
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnException)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
}
