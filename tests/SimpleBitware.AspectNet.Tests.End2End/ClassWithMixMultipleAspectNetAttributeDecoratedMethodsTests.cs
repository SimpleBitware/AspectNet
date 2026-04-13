using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ClassWithMixMultipleAspectNetAttributeDecoratedMethodsTests
{
    [Test]
    public void Should_Record_Activity_For_Public_Static_Method_For_All_Aspects()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods), nameof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethod));
        
        //when
        ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethod();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(4));
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
    
    [Test]
    public void Should_Record_Activity_For_Public_Static_Method_With_Duplicated_Aspects_For_All_Aspects()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods), nameof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethodWithDuplicatedAspects));
        
        //when
        ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethodWithDuplicatedAspects();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(6));
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
    
    [Test]
    public void Should_Record_Activity_For_Public_Static_Method_With_Different_Duplicated_AspectsFor_All_Aspects()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods), nameof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethodWithDifferentDuplicatedAspects));
        
        //when
        ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethodWithDifferentDuplicatedAspects();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(4));
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
