using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.End2End.Helpers;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ClassWithMixMultipleAspectNetAttributeDecoratedPropertiesTests
{
    [Test]
    public void Should_Record_Activity_For_Get_Public_Property()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods),
            MemberNameHelper.PropertyGetterName(nameof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicValue)));

        //when
        var value = ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicValue;
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(6));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.EqualTo(value));
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters, Is.Empty);
            Assert.That(activity.Context.Instance, Is.Null);
        }
    }
}
