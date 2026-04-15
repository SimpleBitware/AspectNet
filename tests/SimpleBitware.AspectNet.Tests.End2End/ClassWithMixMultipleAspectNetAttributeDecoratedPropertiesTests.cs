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
        Assert.That(activities, Has.Count.EqualTo(12));
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
            Assert.That(activities[0].Aspect, Is.InstanceOf<NewRecordActivityAttribute>());
            Assert.That(activities[0].Aspect.Priority, Is.EqualTo(1));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].Aspect, Is.InstanceOf<RecordActivityAttribute>());
            Assert.That(activities[1].Aspect.Priority, Is.EqualTo(2));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[2].Aspect, Is.InstanceOf<NewRecordActivityAttribute>());
            Assert.That(activities[2].Aspect.Priority, Is.EqualTo(3));
            Assert.That(activities[3].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[3].Aspect, Is.InstanceOf<NewRecordActivityAttribute>());
            Assert.That(activities[3].Aspect.Priority, Is.EqualTo(10));
            Assert.That(activities[4].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[4].Aspect, Is.InstanceOf<NewRecordActivityAttribute>());
            Assert.That(activities[4].Aspect.Priority, Is.EqualTo(10));
            Assert.That(activities[5].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
            Assert.That(activities[5].Aspect, Is.InstanceOf<NewRecordActivityAttribute>());
            Assert.That(activities[5].Aspect.Priority, Is.EqualTo(10));
            Assert.That(activities[6].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[6].Aspect, Is.InstanceOf<NewRecordActivityAttribute>());
            Assert.That(activities[6].Aspect.Priority, Is.EqualTo(3));
            Assert.That(activities[7].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
            Assert.That(activities[7].Aspect, Is.InstanceOf<NewRecordActivityAttribute>());
            Assert.That(activities[7].Aspect.Priority, Is.EqualTo(3));
            Assert.That(activities[8].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[8].Aspect, Is.InstanceOf<RecordActivityAttribute>());
            Assert.That(activities[8].Aspect.Priority, Is.EqualTo(2));
            Assert.That(activities[9].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
            Assert.That(activities[9].Aspect, Is.InstanceOf<RecordActivityAttribute>());
            Assert.That(activities[9].Aspect.Priority, Is.EqualTo(2));
            Assert.That(activities[10].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[10].Aspect, Is.InstanceOf<NewRecordActivityAttribute>());
            Assert.That(activities[10].Aspect.Priority, Is.EqualTo(1));
            Assert.That(activities[11].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
            Assert.That(activities[11].Aspect, Is.InstanceOf<NewRecordActivityAttribute>());
            Assert.That(activities[11].Aspect.Priority, Is.EqualTo(1));
        }
    }
}
