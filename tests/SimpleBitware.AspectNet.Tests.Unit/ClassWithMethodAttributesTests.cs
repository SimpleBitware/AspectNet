using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Unit;

public class ClassWithMemberAttributesTests
{
    private readonly ClassWithMemberAttributes classWithMemberAttributes = new();
    
    [Test]
    public void Should_Record_Activity_For_Public_Method()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMemberAttributes), nameof(ClassWithMemberAttributes.PublicMethod));
        
        //when
        classWithMemberAttributes.PublicMethod();
        var activity = RecordActivityAttribute.Activities[activityKey];

        //then
        Assert.That(activity, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activity[0], Is.InstanceOf<AspectNetAttributeContext>());
            Assert.That(activity[1], Is.InstanceOf<AspectNetAttributeContext>());
            
            var context = (AspectNetAttributeContext)activity[1];
            Assert.That(context, Is.Not.Null);
            Assert.That(context.ReturnValue, Is.Null);
            Assert.That(context.Exception, Is.Null);
            Assert.That(context.Parameters, Is.Empty);
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Public_Static_Method()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMemberAttributes), nameof(ClassWithMemberAttributes.PublicStaticMethod));
        
        //when
        ClassWithMemberAttributes.PublicStaticMethod();
        var activity = RecordActivityAttribute.Activities[activityKey];

        //then
        Assert.That(activity, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activity[0], Is.InstanceOf<AspectNetAttributeContext>());
            Assert.That(activity[1], Is.InstanceOf<AspectNetAttributeContext>());
            
            var context = (AspectNetAttributeContext)activity[1];
            Assert.That(context, Is.Not.Null);
            Assert.That(context.ReturnValue, Is.Null);
            Assert.That(context.Exception, Is.Null);
            Assert.That(context.Parameters, Is.Empty);
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Private_Method()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMemberAttributes), "PrivateMethod");
        
        //when
        classWithMemberAttributes.WrapperForPrivateMethod();
        var activity = RecordActivityAttribute.Activities[activityKey];

        //then
        Assert.That(activity, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activity[0], Is.InstanceOf<AspectNetAttributeContext>());
            Assert.That(activity[1], Is.InstanceOf<AspectNetAttributeContext>());
            
            var context = (AspectNetAttributeContext)activity[1];
            Assert.That(context, Is.Not.Null);
            Assert.That(context.ReturnValue, Is.Null);
            Assert.That(context.Exception, Is.Null);
            Assert.That(context.Parameters, Is.Empty);
        }
    }
}
