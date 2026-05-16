using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.End2End.Extensions;
using SimpleBitware.AspectNet.Tests.End2End.Helpers;
using SimpleBitware.AspectNet.Tests.Library.Attributes;
using SimpleBitware.AspectNet.Tests.Library.TestClasses;
using SimpleBitware.AspectNet.Tests.LibraryBase.Attributes;
using SimpleBitware.AspectNet.Tests.LibraryBase.Extensions;

namespace SimpleBitware.AspectNet.Tests.End2End.TestClasses;

public class StaticClassTests
{
    [Test]
    public void Should_Weave_Static_Constructor()
    {
        //given
        var classType = typeof(StaticClass<DateTime>);
        const string methodName = Constants.StaticConstructorMethodName;
        var context = new AspectNetAttributeContext
        {
            Instance = null,
            ClassType = classType,
            MemberName = methodName,
            Parameters = classType.GetMethodParameters(methodName)
        };
        var activityKey = context.GetActivityKey();
        ExpectedAspectAttribute[] expectedAspectAttributes =
        [
            new(typeof(RecordActivityAttribute), 5, context),
            new(typeof(ExtendedRecordActivityAttribute), 7, context)
        ];
        var expectedActivities = expectedAspectAttributes.GetExpectedActivitiesForConstructor([]).ToArray();

        //when
        _ = StaticClass<DateTime>.StaticNullableProperty;
        var hasActivities = ActivitiesStorage.Activities.TryGetValue(activityKey, out var activities);

        //then
        using (Assert.EnterMultipleScope())
        {
            Assert.That(hasActivities, Is.True);
            Assert.That(activities, Is.Not.Null);
            Assert.That(activities, Has.Count.EqualTo(expectedActivities.Length));
            Assert.That(activities!.ToExpectedActivities(), Is.EqualTo(expectedActivities).Using<ExpectedActivity>(ActivityComparer.Instance));
        }
    }

    [Test]
    public void Should_Weave_StaticNullableProperty_Setter()
    {
        //given
        var classType = typeof(StaticClass<string>);
        var methodName = MemberNameHelper.PropertySetterName(nameof(StaticClass<>.StaticNullableProperty));
        var context = new AspectNetAttributeContext
        {
            Instance = null,
            ClassType = classType,
            MemberName = methodName,
            Parameters = classType.GetMethodParameters(methodName)
        };
        var activityKey = context.GetActivityKey();
        ExpectedAspectAttribute[] expectedAspectAttributes =
        [
            new(typeof(RecordActivityAttribute), 10, context),
            new(typeof(ExtendedRecordActivityAttribute), 7, context)
        ];
        var expectedActivities = expectedAspectAttributes.GetActivities().ToArray();

        //when
        StaticClass<string>.StaticNullableProperty = Guid.NewGuid().ToString();
        var hasActivities = ActivitiesStorage.Activities.TryGetValue(activityKey, out var activities);

        //then
        using (Assert.EnterMultipleScope())
        {
            Assert.That(hasActivities, Is.True);
            Assert.That(activities, Is.Not.Null);
            Assert.That(activities, Has.Count.EqualTo(expectedActivities.Length));
            Assert.That(activities!.ToExpectedActivities(), Is.EqualTo(expectedActivities).Using<ExpectedActivity>(ActivityComparer.Instance));
        }
    }
    
    [Test]
    public void Should_NOT_Weave_Excluded_StaticNullableProperty_Getter()
    {
        //given
        var activityKey = new ActivityKey(typeof(StaticClass<int>), MemberNameHelper.PropertyGetterName(nameof(StaticClass<>.StaticNullableProperty)));

        //when
        _ = StaticClass<int>.StaticNullableProperty;
        var hasActivities = ActivitiesStorage.Activities.TryGetValue(activityKey, out _);

        //then
        Assert.That(hasActivities, Is.False);
    }
}
