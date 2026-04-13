using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ClassWithMultipleAspectNetAttributeDecoratedAsyncMethodsTests
{
    private readonly ClassWithMultipleAspectNetAttributeDecoratedMethods sut = new();
    
    [Test]
    public async Task Should_Record_Activity_For_Public_Async_Method_For_All_Aspects()
    {
        //given
        var value = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithMultipleAspectNetAttributeDecoratedMethods), nameof(ClassWithMultipleAspectNetAttributeDecoratedMethods.PublicMethodAsync), 1);
        
        //when
        var result = await sut.PublicMethodAsync(value);
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(4));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.EqualTo(value));
            Assert.That(result, Is.EqualTo(value));
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters.Count, Is.EqualTo(1));
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithMultipleAspectNetAttributeDecoratedMethods>());
        }
    }
    
        [Test]
    public void Should_Record_Activity_For_Public_Async_Method_With_Async_Exception_For_All_Aspects()
    {
        //given
        var value = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithMultipleAspectNetAttributeDecoratedMethods), nameof(ClassWithMultipleAspectNetAttributeDecoratedMethods.PublicAsyncMethodWithAsyncException), 1);
        
        //when
        Assert.ThrowsAsync<Exception>(() => sut.PublicAsyncMethodWithAsyncException(value));
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(6));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Not.Null);
            Assert.That(activity.Context.Parameters.Count, Is.EqualTo(1));
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithMultipleAspectNetAttributeDecoratedMethods>());
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Public_Async_Method_With_Sync_Exception_For_All_Aspects()
    {
        //given
        var value = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithMultipleAspectNetAttributeDecoratedMethods), nameof(ClassWithMultipleAspectNetAttributeDecoratedMethods.PublicAsyncMethodWithSyncException), 1);
        
        //when
        Assert.ThrowsAsync<Exception>(() => sut.PublicAsyncMethodWithSyncException(value));
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(6));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Not.Null);
            Assert.That(activity.Context.Parameters.Count, Is.EqualTo(1));
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithMultipleAspectNetAttributeDecoratedMethods>());
        }
    }
}
