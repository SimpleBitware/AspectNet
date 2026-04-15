using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.End2End.Helpers;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ClassDecoratedWithAspectNetAttributePropertiesTests
{
    private readonly ClassDecoratedWithAspectNetAttributeMethods sut = new();

    [Test]
    public void Should_Record_Activity_For_Get_Public_Property()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods),
            MemberNameHelper.PropertyGetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicValue)));

        //when
        var value = sut.PublicValue;
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                (int)a.Context.ReturnValue! == value &&
                a.Context.Exception is null &&
                a.Context.MemberName == MemberNameHelper.PropertyGetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicValue)) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance?.GetType() == typeof(ClassDecoratedWithAspectNetAttributeMethods)
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }

    [Test]
    public void Should_Record_Activity_For_Set_Public_Property()
    {
        //given
        var propertyValue = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods),
            MemberNameHelper.PropertySetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicValue)), 1);

        //when
        sut.PublicValue = propertyValue;
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is null &&
                a.Context.MemberName == MemberNameHelper.PropertySetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicValue)) &&
                (int)a.Context.Parameters[Constants.PropertySetterParameterName] == propertyValue &&
                a.Context.Instance?.GetType() == typeof(ClassDecoratedWithAspectNetAttributeMethods)
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Get_Public_Property_When_Throws_Exception()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods),
            MemberNameHelper.PropertyGetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicValueException)));
        //when
        Assert.Throws<Exception>(() => _ = sut.PublicValueException);
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                (int)a.Context.ReturnValue! == 0 &&
                a.Context.Exception is not null &&
                a.Context.MemberName == MemberNameHelper.PropertyGetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicValueException)) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance?.GetType() == typeof(ClassDecoratedWithAspectNetAttributeMethods)
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnException)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }

    [Test]
    public void Should_Record_Activity_For_Get_Public_Static_Property()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods),
            MemberNameHelper.PropertyGetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicStaticValue)));

        //when
        var value = ClassDecoratedWithAspectNetAttributeMethods.PublicStaticValue;
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                (int)a.Context.ReturnValue! == 0 &&
                a.Context.Exception is null &&
                a.Context.MemberName == MemberNameHelper.PropertyGetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicStaticValue)) &&
                a.Context.Parameters.Count == 0 &&
                a.Context.Instance is null
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }

    [Test]
    public void Should_Record_Activity_For_Set_Public_Static_Property()
    {
        //given
        var propertyValue = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods),
            MemberNameHelper.PropertySetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicStaticValue)), 1);

        //when
        ClassDecoratedWithAspectNetAttributeMethods.PublicStaticValue = propertyValue;
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activities, Has.All.Matches<Activity>(a => 
                a.Context.ReturnValue is null &&
                a.Context.Exception is null &&
                a.Context.MemberName == MemberNameHelper.PropertySetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicStaticValue)) &&
                (int)a.Context.Parameters[Constants.PropertySetterParameterName] == propertyValue &&
                a.Context.Instance is null
            ));
            Assert.That(activities[0].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnEntry)));
            Assert.That(activities[1].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnSuccess)));
            Assert.That(activities[2].AspectMethodName, Is.EqualTo(nameof(IAspectNetAttribute.OnExit)));
        }
    }
}
