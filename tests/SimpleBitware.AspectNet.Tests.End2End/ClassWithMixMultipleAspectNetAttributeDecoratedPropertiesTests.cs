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
        Assert.That(activities, Has.Count.EqualTo(9));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                (int)a.Context.ReturnValue! == value &&
                a.Context.Exception is null &&
                a.Context.MemberName == MemberNameHelper.PropertyGetterName(nameof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicValue)) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance is null
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[3].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[4].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
            Assert.That(activities[5].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[6].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
            Assert.That(activities[7].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[8].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
}
