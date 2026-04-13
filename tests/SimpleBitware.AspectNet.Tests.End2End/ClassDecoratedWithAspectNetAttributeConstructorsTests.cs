using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ClassDecoratedWithAspectNetAttributeConstructorsTests
{
    [Test]
    public void Should_Record_Activity_For_Public_Constructor_When_Throws_Exception()
    {
        //given
        const int no = 3;
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods), Constants.InstanceConstructorMethodName, 1);
        
        //when
        Assert.Throws<Exception>(() => new ClassDecoratedWithAspectNetAttributeMethods(no));
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
            Assert.That(activity.Context.Parameters.FirstOrDefault().Value, Is.EqualTo(no));
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassDecoratedWithAspectNetAttributeMethods>());
        }
    }
}
