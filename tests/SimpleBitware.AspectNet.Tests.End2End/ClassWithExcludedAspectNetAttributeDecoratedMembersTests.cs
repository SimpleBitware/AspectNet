using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ClassWithExcludedAspectNetAttributeDecoratedMembersTests
{
    private readonly ClassWithExcludedAspectNetAttributeDecoratedMembers sut = new();
    
    [Test]
    public void Should_NOT_Record_Activity_For_Constructor_When_Excluded()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithExcludedAspectNetAttributeDecoratedMembers), Constants.InstanceConstructorMethodName);

        //then
        Assert.That(ActivitiesStorage.Activities.ContainsKey(activityKey), Is.False);
    }
    
    [Test]
    public void Should_NOT_Record_Activity_For_Public_Method_When_Excluded()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithExcludedAspectNetAttributeDecoratedMembers), nameof(ClassWithExcludedAspectNetAttributeDecoratedMembers.PublicMethod));
        
        //when
        sut.PublicMethod();

        //then
        Assert.That(ActivitiesStorage.Activities.ContainsKey(activityKey), Is.False);
    }
    
    [Test]
    public void Should_NOT_Record_Activity_For_Public_Property_When_Excluded()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithExcludedAspectNetAttributeDecoratedMembers), nameof(ClassWithExcludedAspectNetAttributeDecoratedMembers.PublicValue));
        
        //when
        var value = sut.PublicValue;

        //then
        Assert.That(ActivitiesStorage.Activities.ContainsKey(activityKey), Is.False);
    }
}
