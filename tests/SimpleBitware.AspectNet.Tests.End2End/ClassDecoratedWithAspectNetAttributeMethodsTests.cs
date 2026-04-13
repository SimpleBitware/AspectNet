using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ClassDecoratedWithAspectNetAttributeMethodsTests
{
    private readonly ClassDecoratedWithAspectNetAttributeMethods sut = new();
    
    [Test]
    public void Should_Record_Activity_For_Public_Method()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods), nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicMethod));
        
        //when
        sut.PublicMethod();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters, Is.Empty);
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassDecoratedWithAspectNetAttributeMethods>());
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Public_Method_When_Throws_Exception()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods), nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicMethodException));
        
        //when
        Assert.Throws<Exception>(() => sut.PublicMethodException());
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Not.Null);
            Assert.That(activity.Context.Parameters, Is.Empty);
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassDecoratedWithAspectNetAttributeMethods>());
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Public_Static_Method()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods), nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicStaticMethod));
        
        //when
        ClassDecoratedWithAspectNetAttributeMethods.PublicStaticMethod();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters, Is.Empty);
            Assert.That(activity.Context.Instance, Is.Null);
        }
    }
}
