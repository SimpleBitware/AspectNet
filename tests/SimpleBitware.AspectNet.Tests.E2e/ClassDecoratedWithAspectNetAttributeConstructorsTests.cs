using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.E2e;

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
        var activity = RecordActivityAttribute.Activities[activityKey];

        //then
        Assert.That(activity, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activity[0], Is.InstanceOf<AspectNetAttributeContext>());
            Assert.That(activity[1], Is.InstanceOf<AspectNetAttributeContext>());
            Assert.That(activity[2], Is.InstanceOf<AspectNetAttributeContext>());
            
            var context = (AspectNetAttributeContext)activity[2];
            Assert.That(context, Is.Not.Null);
            Assert.That(context.ReturnValue, Is.Null);
            Assert.That(context.Exception, Is.Not.Null);
            Assert.That(context.Parameters.Count, Is.EqualTo(1));
            Assert.That(context.Parameters.FirstOrDefault().Value, Is.EqualTo(no));
            Assert.That(context.Instance, Is.InstanceOf<ClassDecoratedWithAspectNetAttributeMethods>());
        }
    }
}
