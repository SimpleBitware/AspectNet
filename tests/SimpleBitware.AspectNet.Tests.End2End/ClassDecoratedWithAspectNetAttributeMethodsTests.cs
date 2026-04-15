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
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is null &&
                a.Context.MemberName == nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicMethod) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance?.GetType() == typeof(ClassDecoratedWithAspectNetAttributeMethods)
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
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
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is not null &&
                a.Context.MemberName == nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicMethodException) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance?.GetType() == typeof(ClassDecoratedWithAspectNetAttributeMethods)
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnException)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
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
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is null &&
                a.Context.MemberName == nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicStaticMethod) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance is null
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
}
