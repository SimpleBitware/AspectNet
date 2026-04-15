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
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is not null &&
                a.Context.MemberName == Constants.InstanceConstructorMethodName &&
                a.Context.Parameters.Count == 1 &&
                (int)a.Context.Parameters.First().Value == no &&
                a.Context.Instance?.GetType() == typeof(ClassDecoratedWithAspectNetAttributeMethods)
                ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnException)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
}
