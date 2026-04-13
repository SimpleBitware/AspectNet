using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ClassWithMultipleAspectNetAttributeDecoratedMethodsTests
{
    private readonly ClassWithMultipleAspectNetAttributeDecoratedMethods sut = new();
    
    [Test]
    public void Should_Record_Activity_For_Public_Method_For_All_Aspects()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMultipleAspectNetAttributeDecoratedMethods), nameof(ClassWithMultipleAspectNetAttributeDecoratedMethods.PublicMethod));
        
        //when
        sut.PublicMethod();
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
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithMultipleAspectNetAttributeDecoratedMethods>());
        }
    }
}
