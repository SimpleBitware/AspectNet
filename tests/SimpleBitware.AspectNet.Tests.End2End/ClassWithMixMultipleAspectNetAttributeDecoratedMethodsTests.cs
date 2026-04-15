using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ClassWithMixMultipleAspectNetAttributeDecoratedMethodsTests
{
    [Test]
    public void Should_Record_Activity_For_Public_Static_Method()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods), nameof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethod));
        
        //when
        ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethod();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(6));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is null &&
                a.Context.MemberName == nameof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethod) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance is null
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[3].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
            Assert.That(activities[4].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[5].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Public_Static_Method_With_Duplicated_Aspects()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods), nameof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethodWithDuplicatedAspects));
        
        //when
        ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethodWithDuplicatedAspects();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(9));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is null &&
                a.Context.MemberName == nameof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethodWithDuplicatedAspects) &&
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
    
    [Test]
    public void Should_Record_Activity_For_Public_Static_Method_With_Same_Aspect_As_Class()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods), nameof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethodWithSameAspectAsClass));
        
        //when
        ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethodWithSameAspectAsClass();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(6));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is null &&
                a.Context.MemberName == nameof(ClassWithMixMultipleAspectNetAttributeDecoratedMethods.PublicStaticMethodWithSameAspectAsClass) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance is null
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[3].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
            Assert.That(activities[4].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[5].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
}
