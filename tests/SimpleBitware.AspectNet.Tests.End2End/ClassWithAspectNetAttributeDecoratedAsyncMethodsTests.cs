using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ClassWithAspectNetAttributeDecoratedAsyncMethodsTests
{
    private readonly ClassWithAspectNetAttributeDecoratedMembers sut = new();
    
    [Test]
    public async Task Should_Record_Activity_For_Public_Async_Method()
    {
        //given
        var value = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicMethodAsync), 1);
        
        //when
        var result = await sut.PublicMethodAsync(value);
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo(value));
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                (int)a.Context.ReturnValue! == value &&
                a.Context.Exception is null &&
                a.Context.MemberName == nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicMethodAsync) &&
                a.Context.Parameters.Count == 1 &&
                a.Context.Instance?.GetType() == typeof(ClassWithAspectNetAttributeDecoratedMembers)
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Public_Async_Method_With_Async_Exception()
    {
        //given
        var value = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicAsyncMethodWithAsyncException), 1);
        
        //when
        Assert.ThrowsAsync<Exception>(() => sut.PublicAsyncMethodWithAsyncException(value));
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is not null &&
                a.Context.MemberName == nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicAsyncMethodWithAsyncException) &&
                a.Context.Parameters.Count == 1 &&
                a.Context.Instance?.GetType() == typeof(ClassWithAspectNetAttributeDecoratedMembers)
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnException)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Public_Async_Method_With_Sync_Exception()
    {
        //given
        var value = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicAsyncMethodWithSyncException), 1);
        
        //when
        Assert.ThrowsAsync<Exception>(() => sut.PublicAsyncMethodWithSyncException(value));
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is not null &&
                a.Context.MemberName == nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicAsyncMethodWithSyncException) &&
                a.Context.Parameters.Count == 1 &&
                a.Context.Instance?.GetType() == typeof(ClassWithAspectNetAttributeDecoratedMembers)
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnException)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
    
    [Test]
    public async Task Should_Record_Activity_For_Public_ValueTask_Method()
    {
        //given
        var value = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueTaskMethod), 1);
        
        //when
        _ = await sut.PublicValueTaskMethod(value);
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                (int)a.Context.ReturnValue! == value &&
                a.Context.Exception is null &&
                a.Context.MemberName == nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueTaskMethod) &&
                a.Context.Parameters.Count == 1 &&
                a.Context.Instance?.GetType() == typeof(ClassWithAspectNetAttributeDecoratedMembers)
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
}
