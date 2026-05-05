using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.End2End.Extensions;
using SimpleBitware.AspectNet.Tests.End2End.Helpers;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving.Extensions;
using SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

namespace SimpleBitware.AspectNet.Tests.End2End.TestClasses;

public class ExtendTestClassTests
{
    private readonly ExtendTestClass<string> testClass = new();
    
    [Test]
    public void Should_Weave_Constructor_With_Additional_Class_Aspect()
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
            new(typeof(RecordActivityAttribute), 7, context)
        ];
        ExpectedAspectAttribute[] inheritedAspectAttributes =
        [
            new(typeof(RecordActivityAttribute), 5, context),
        ];        
        var expectedActivities = expectedAspectAttributes.GetExpectedActivitiesForConstructor(inheritedAspectAttributes).ToArray();

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
    public void Should_Weave_The_Inherited_Method()
    {
        //given
        var classType = testClass.GetType();
        const string methodName = nameof(testClass.MethodWithReturn);
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
            new(typeof(RecordActivityAttribute), 7, context)
        ];
        var expectedActivities = expectedAspectAttributes.GetActivities().ToArray();

        //when
        _ = testClass.MethodWithReturn();
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
    public void Should_NOT_Weave_Excluded_ExcludedMethod()
    {
        //given
        const string methodName = nameof(testClass.ExcludedMethod);
        var activityKey = new ActivityKey(testClass.GetType(), methodName);

        //when
        testClass.ExcludedMethod();
        var hasActivities = ActivitiesStorage.Activities.TryGetValue(activityKey, out _);

        //then
        Assert.That(hasActivities, Is.False);
    }
    
    [Test]
    public void Should_Weave_Inherited_AsyncValueTaskMethod()
    {
        //given
        var inputParameter = default(string);
        var classType = testClass.GetType();
        const string methodName = nameof(testClass.AsyncValueTaskMethod);
        var context = new AspectNetAttributeContext
        {
            Instance = testClass,
            ClassType = classType,
            MemberName = methodName,
            Parameters = classType.GetMethodParameters(methodName),
            Exception = new ArgumentException()
        };
        var activityKey = context.GetActivityKey();
        ExpectedAspectAttribute[] expectedAspectAttributes =
        [
            new(typeof(RecordActivityAttribute), 7, context),
            new(typeof(RecordActivityAttribute), 10, context)
        ];
        var expectedActivities = expectedAspectAttributes.GetActivities().ToArray();

        //when
        Assert.ThrowsAsync<ArgumentException>(() => testClass.AsyncValueTaskMethod(inputParameter, CancellationToken.None).AsTask());
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
