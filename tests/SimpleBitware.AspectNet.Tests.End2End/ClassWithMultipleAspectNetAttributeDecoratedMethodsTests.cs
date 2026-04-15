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
        Assert.That(activities, Has.Count.EqualTo(6));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is null &&
                a.Context.MemberName == nameof(ClassWithMultipleAspectNetAttributeDecoratedMethods.PublicMethod) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance?.GetType() == typeof(ClassWithMultipleAspectNetAttributeDecoratedMethods)
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[0].Aspect, Is.InstanceOf<RecordActivityAttribute>());
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].Aspect, Is.InstanceOf<NewRecordActivityAttribute>());
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].Aspect, Is.InstanceOf<NewRecordActivityAttribute>());
            Assert.That(activities[3].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
            Assert.That(activities[3].Aspect, Is.InstanceOf<NewRecordActivityAttribute>());
            Assert.That(activities[4].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[4].Aspect, Is.InstanceOf<RecordActivityAttribute>());
            Assert.That(activities[5].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
            Assert.That(activities[5].Aspect, Is.InstanceOf<RecordActivityAttribute>());
        }
    }
}
