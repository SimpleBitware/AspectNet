using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.E2e.Helpers;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.E2e;

public class ClassDecoratedWithAspectNetAttributePropertiesTests
{
    private readonly ClassDecoratedWithAspectNetAttributeMethods sut = new();

    [Test]
    public void Should_Record_Activity_For_Get_Public_Property()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods),
            MemberNameHelper.PropertyGetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicValue)));

        //when
        var value = sut.PublicValue;
        var activity = RecordActivityAttribute.Activities[activityKey];

        //then
        Assert.That(activity, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activity[0], Is.InstanceOf<AspectNetAttributeContext>());
            Assert.That(activity[1], Is.InstanceOf<AspectNetAttributeContext>());

            var context = (AspectNetAttributeContext)activity[1];
            Assert.That(context, Is.Not.Null);
            Assert.That(context.ReturnValue, Is.EqualTo(value));
            Assert.That(context.Exception, Is.Null);
            Assert.That(context.Parameters, Is.Empty);
            Assert.That(context.Instance, Is.InstanceOf<ClassDecoratedWithAspectNetAttributeMethods>());
        }
    }

    [Test]
    public void Should_Record_Activity_For_Set_Public_Property()
    {
        //given
        var propertyValue = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods),
            MemberNameHelper.PropertySetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicValue)), 1);

        //when
        sut.PublicValue = propertyValue;
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
            Assert.That(context.Parameters[Constants.SetterParameterName], Is.EqualTo(propertyValue));
            Assert.That(context.Instance, Is.InstanceOf<ClassDecoratedWithAspectNetAttributeMethods>());
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Get_Public_Property_When_Throws_Exception()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods),
            MemberNameHelper.PropertyGetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicValueException)));
        var value = 0;
        
        //when
        try
        {
            value = sut.PublicValueException;
        }
        catch
        {
            // ignored
        }

        var activity = RecordActivityAttribute.Activities[activityKey];

        //then
        Assert.That(activity, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activity[0], Is.InstanceOf<AspectNetAttributeContext>());
            Assert.That(activity[1], Is.InstanceOf<AspectNetAttributeContext>());
            Assert.That(activity[2], Is.InstanceOf<AspectNetAttributeContext>());
            
            var context = (AspectNetAttributeContext)activity[1];
            Assert.That(context, Is.Not.Null);
            Assert.That(context.ReturnValue, Is.EqualTo(value));
            Assert.That(context.Exception, Is.Not.Null);
            Assert.That(context.Parameters, Is.Empty);
            Assert.That(context.Instance, Is.InstanceOf<ClassDecoratedWithAspectNetAttributeMethods>());
        }
    }

    [Test]
    public void Should_Record_Activity_For_Get_Public_Static_Property()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods),
            MemberNameHelper.PropertyGetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicStaticValue)));

        //when
        var value = ClassDecoratedWithAspectNetAttributeMethods.PublicStaticValue;
        var activity = RecordActivityAttribute.Activities[activityKey];

        //then
        Assert.That(activity, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activity[0], Is.InstanceOf<AspectNetAttributeContext>());
            Assert.That(activity[1], Is.InstanceOf<AspectNetAttributeContext>());

            var context = (AspectNetAttributeContext)activity[1];
            Assert.That(context, Is.Not.Null);
            Assert.That(context.ReturnValue, Is.EqualTo(value));
            Assert.That(context.Exception, Is.Null);
            Assert.That(context.Parameters, Is.Empty);
            Assert.That(context.Instance, Is.Null);
        }
    }

    [Test]
    public void Should_Record_Activity_For_Set_Public_Static_Property()
    {
        //given
        var propertyValue = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassDecoratedWithAspectNetAttributeMethods),
            MemberNameHelper.PropertySetterName(nameof(ClassDecoratedWithAspectNetAttributeMethods.PublicStaticValue)), 1);

        //when
        ClassDecoratedWithAspectNetAttributeMethods.PublicStaticValue = propertyValue;
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
            Assert.That(context.Parameters[Constants.SetterParameterName], Is.EqualTo(propertyValue));
            Assert.That(context.Instance, Is.Null);
        }
    }
}
