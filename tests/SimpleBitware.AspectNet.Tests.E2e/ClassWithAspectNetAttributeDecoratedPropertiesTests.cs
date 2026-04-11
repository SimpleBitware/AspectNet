using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.E2e.Helpers;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.E2e;

public class ClassWithAspectNetAttributeDecoratedPropertiesTests
{
    private readonly ClassWithAspectNetAttributeDecoratedMembers sut = new();
    
    [Test]
    public void Should_Record_Activity_For_Get_Public_Property()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertyGetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueWithPropertyAttribute)));
        
        //when
        var value = sut.PublicValueWithPropertyAttribute;
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
            Assert.That(context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Set_Public_Property()
    {
        //given
        var propertyValue = DateTime.Now.Ticks;
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertySetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueWithPropertyAttribute)), 1);
        
        //when
        sut.PublicValueWithPropertyAttribute = propertyValue;
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
            Assert.That(context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
        }
    }
    
    [Test]
    public void Should_NOT_Record_Activity_For_Get_Public_Property_With_Setter_Only_Attribute()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertyGetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueWithSetterPropertyAttribute)));
        
        //when
        var value = sut.PublicValueWithSetterPropertyAttribute;

        //then
        Assert.That(RecordActivityAttribute.Activities.ContainsKey(activityKey), Is.False);
    }
    
    [Test]
    public void Should_Record_Activity_For_Set_Public_Property_With_Setter_Only_Attribute()
    {
        //given
        var propertyValue = DateTime.Now.Ticks;
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertySetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueWithSetterPropertyAttribute)), 1);
        
        //when
        sut.PublicValueWithSetterPropertyAttribute = propertyValue;
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
            Assert.That(context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Get_Public_Property_With_Getter_Only_Attribute()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertyGetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueWithGetterPropertyAttribute)));
        
        //when
        var value = sut.PublicValueWithGetterPropertyAttribute;
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
            Assert.That(context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
        }
    }
    
    [Test]
    public void Should_NOT_Record_Activity_For_Set_Public_Property_With_Getter_Only_Attribute()
    {
        //given
        var propertyValue = DateTime.Now.Ticks;
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertySetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueWithGetterPropertyAttribute)));
        
        //when
        sut.PublicValueWithGetterPropertyAttribute = propertyValue;

        //then
        Assert.That(RecordActivityAttribute.Activities.ContainsKey(activityKey), Is.False);
    }
    
    [Test]
    public void Should_Record_Activity_For_Get_Public_Static_Property()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertyGetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicStaticValueWithPropertyAttribute)));
        
        //when
        var value = ClassWithAspectNetAttributeDecoratedMembers.PublicStaticValueWithPropertyAttribute;
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
        var propertyValue = DateTime.Now.Ticks;
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertySetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicStaticValueWithPropertyAttribute)), 1);
        
        //when
        ClassWithAspectNetAttributeDecoratedMembers.PublicStaticValueWithPropertyAttribute = propertyValue;
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
