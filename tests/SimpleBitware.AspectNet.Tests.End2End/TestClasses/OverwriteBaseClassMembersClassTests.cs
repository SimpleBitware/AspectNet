using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.End2End.Extensions;
using SimpleBitware.AspectNet.Tests.End2End.Helpers;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving.Extensions;
using SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

namespace SimpleBitware.AspectNet.Tests.End2End.TestClasses;

public class OverwriteBaseClassMembersClassTests
{
    private readonly OverwriteBaseClassMembersClass<int> testClass = new();
    
    [Test]
    public void Should_Use_The_Inherited_Weaved_Constructor()
    {
        //given
        var classType = testClass.GetType();
        const string methodName = Constants.InstanceConstructorMethodName;
        var context = new AspectNetAttributeContext
        {
            Instance = testClass,
            ClassType = classType,
            MemberName = methodName,
            Parameters = classType.GetMethodParameters(methodName)
        };
        var activityKey = context.GetActivityKey();
        ExpectedAspectAttribute[] expectedAspectAttributes =
        [
            new(typeof(RecordActivityAttribute), 5, context)
        ];
        var expectedActivities = expectedAspectAttributes.GetActivities().ToArray();

        //when
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
    public void Should_Weave_Overwritten_Method()
    {
        //given
        var classType = testClass.GetType();
        const string methodName = nameof(OverwriteBaseClassMembersClass<>.VoidMethod);
        var context = new AspectNetAttributeContext
        {
            Instance = testClass,
            ClassType = classType,
            MemberName = methodName,
            Parameters = classType.GetMethodParameters(methodName)
        };
        var activityKey = context.GetActivityKey();
        ExpectedAspectAttribute[] expectedAspectAttributes =
        [
            new(typeof(ExtendedRecordActivityAttribute), 3, context)
        ];
        var expectedActivities = expectedAspectAttributes.GetActivities().ToArray();

        //when
        testClass.VoidMethod();
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
    public async Task Should_Weave_Overwritten_AsyncMethod()
    {
        //given
        const int inputParameter = 2;
        var classType = testClass.GetType();
        const string methodName = nameof(OverwriteBaseClassMembersClass<>.AsyncTaskMethod);
        var context = new AspectNetAttributeContext
        {
            Instance = testClass,
            ClassType = classType,
            MemberName = methodName,
            Parameters = classType.GetMethodParameters(methodName)
        };
        var activityKey = context.GetActivityKey();
        ExpectedAspectAttribute[] expectedAspectAttributes =
        [
            new(typeof(ModifyStateAttribute), int.MaxValue, context)
        ];
        var expectedActivities = expectedAspectAttributes.GetActivities().ToArray();
        expectedActivities[1].Context.ReturnValue = inputParameter;
        expectedActivities[2].Context.ReturnValue = inputParameter;

        //when
        await testClass.AsyncTaskMethod(inputParameter, CancellationToken.None);
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
}
