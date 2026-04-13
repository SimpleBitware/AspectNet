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
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.EqualTo(value));
            Assert.That(result, Is.EqualTo(value));
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters.Count, Is.EqualTo(1));
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
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
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Not.Null);
            Assert.That(activity.Context.Parameters.Count, Is.EqualTo(1));
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
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
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Not.Null);
            Assert.That(activity.Context.Parameters.Count, Is.EqualTo(1));
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
        }
    }
    
    [Test]
    public async Task Should_Record_Activity_For_Public_ValueTask_Method()
    {
        //given
        var value = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueTaskMethod), 1);
        
        //when
        var result = await sut.PublicValueTaskMethod(value);
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.EqualTo(value));
            Assert.That(result, Is.EqualTo(value));
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters.Count, Is.EqualTo(1));
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
        }
    }
}
