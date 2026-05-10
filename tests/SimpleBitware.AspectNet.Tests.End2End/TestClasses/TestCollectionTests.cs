using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.End2End.Extensions;
using SimpleBitware.AspectNet.Tests.End2End.Helpers;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving.Extensions;
using SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

namespace SimpleBitware.AspectNet.Tests.End2End.TestClasses;

public class TestCollectionTests
{
    private readonly TestCollection<int> testClass = new();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        testClass.Add(55);
    }
    
    [Test]
    public void Should_Weave_Indexer_Property_Getter()
    {
        //given
        var classType = testClass.GetType();
        var methodName = MemberNameHelper.IndexerGetterName;
        var context = new AspectNetAttributeContext
        {
            Instance = testClass,
            ClassType = classType,
            MemberName = methodName,
            Parameters = classType.GetMethodParameters(methodName),
            Exception = new ArgumentOutOfRangeException()
        };
        var activityKey = context.GetActivityKey();
        ExpectedAspectAttribute[] expectedAspectAttributes =
        [
            new(typeof(RecordActivityAttribute), int.MaxValue, context),
            new(typeof(ExtendedRecordActivityAttribute), int.MaxValue, context)
        ];
        var expectedActivities = expectedAspectAttributes.GetActivities().ToArray();

        //when
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = testClass[100]);
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
    public void Should_Weave_Indexer_Property_Setter()
    {
        //given
        var classType = testClass.GetType();
        var methodName = MemberNameHelper.IndexerSetterName;
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
            new(typeof(RecordActivityAttribute), int.MaxValue, context),
            new(typeof(ExtendedRecordActivityAttribute), int.MaxValue, context)
        ];
        var expectedActivities = expectedAspectAttributes.GetActivities().ToArray();

        //when
        testClass[0] = 66;
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
