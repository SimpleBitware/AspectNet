using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.E2e;

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
            Assert.That(context.Instance, Is.InstanceOf<ClassDecoratedWithAspectNetAttributeMethods>());
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Public_Static_Method()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods), nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicStaticMethod));
        
        //when
        ClassDecoratedWithAspectNetAttributeMethods.PublicStaticMethod();
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
            Assert.That(context.Instance, Is.Null);
        }
    }
}
