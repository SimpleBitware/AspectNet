using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

public class ClassWithAspectNetAttributeDecoratedConstructorsTests
{
    [Test]
    public void Should_Record_Activity_For_Public_Constructor()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedConstructors), Constants.InstanceConstructorMethodName);
        
        //when
        var instance = new ClassWithAspectNetAttributeDecoratedConstructors();
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
            Assert.That(context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedConstructors>());
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Public_Constructor_With_Parameters()
    {
        //given
        const int no = 2;
        const int numberOfParameters = 1;
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedConstructors), Constants.InstanceConstructorMethodName, numberOfParameters);
        
        //when
        var instance = new ClassWithAspectNetAttributeDecoratedConstructors(no);
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
            Assert.That(context.Parameters.Count, Is.EqualTo(numberOfParameters));
            Assert.That(context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedConstructors>());
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Static_Constructor()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedStaticConstructor), Constants.StaticConstructorMethodName);
        
        //when
        var value = ClassWithAspectNetAttributeDecoratedStaticConstructor.Value;
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
