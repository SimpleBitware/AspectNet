using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.E2e;

public class ClassDecoratedWithExcludedAspectNetAttributeTests
{
    private readonly ClassDecoratedWithExcludedAspectNetAttribute sut = new();
    
    [Test]
    public void Should_NOT_Record_Activity_For_Constructor()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithExcludedAspectNetAttributeDecoratedMembers), Constants.InstanceConstructorMethodName);

        //then
        Assert.That(RecordActivityAttribute.Activities.ContainsKey(activityKey), Is.False);
    }
    
    [Test]
    public void Should_NOT_Record_Activity_For_Public_Method()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithExcludedAspectNetAttributeDecoratedMembers), nameof(ClassWithExcludedAspectNetAttributeDecoratedMembers.PublicMethod));
        
        //when
        sut.PublicMethod();

        //then
        Assert.That(RecordActivityAttribute.Activities.ContainsKey(activityKey), Is.False);
    }
    
    [Test]
    public void Should_NOT_Record_Activity_For_Public_Property()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithExcludedAspectNetAttributeDecoratedMembers), nameof(ClassWithExcludedAspectNetAttributeDecoratedMembers.PublicValue));
        
        //when
        var value = sut.PublicValue;

        //then
        Assert.That(RecordActivityAttribute.Activities.ContainsKey(activityKey), Is.False);
    }
}
