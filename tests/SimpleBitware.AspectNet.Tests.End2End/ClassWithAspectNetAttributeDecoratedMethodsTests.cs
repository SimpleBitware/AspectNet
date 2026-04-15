using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ClassWithAspectNetAttributeDecoratedMethodsTests
{
    private readonly ClassWithAspectNetAttributeDecoratedMembers sut = new();
    
    [Test]
    public void Should_Record_Activity_For_Public_Method()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicMethod));
        
        //when
        sut.PublicMethod();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is null &&
                a.Context.MemberName == nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicMethod) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance?.GetType() == typeof(ClassWithAspectNetAttributeDecoratedMembers)
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Public_Static_Method()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicStaticMethod));
        
        //when
        ClassWithAspectNetAttributeDecoratedMembers.PublicStaticMethod();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is null &&
                a.Context.MemberName == nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicStaticMethod) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance is null
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Private_Method()
    {
        //given
        var methodName = "PrivateMethod";
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), methodName);
        
        //when
        sut.WrapperForPrivateMethod();
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is null &&
                a.Context.MemberName == methodName &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance?.GetType() == typeof(ClassWithAspectNetAttributeDecoratedMembers)
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
}
