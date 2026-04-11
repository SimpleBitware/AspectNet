using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Unit.Helpers;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Unit;

public class ClassWithPropertyAttributesTests
{
    private readonly ClassWithMemberAttributes classWithMemberAttributes = new();
    
    [Test]
    public void Should_Record_Activity_For_Get_Public_Property()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMemberAttributes), MemberNameHelper.PropertyGetterName(nameof(ClassWithMemberAttributes.PublicValueWithPropertyAttribute)));
        
        //when
        var value = classWithMemberAttributes.PublicValueWithPropertyAttribute;
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
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Set_Public_Property()
    {
        //given
        var propertyValue = DateTime.Now.Ticks;
        var activityKey = new ActivityKey(typeof(ClassWithMemberAttributes), MemberNameHelper.PropertySetterName(nameof(ClassWithMemberAttributes.PublicValueWithPropertyAttribute)));
        
        //when
        classWithMemberAttributes.PublicValueWithPropertyAttribute = propertyValue;
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
        }
    }
    
    [Test]
    public void Should_NOT_Record_Activity_For_Get_Public_Property_With_Setter_Only_Attribute()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMemberAttributes), MemberNameHelper.PropertyGetterName(nameof(ClassWithMemberAttributes.PublicValueWithSetterPropertyAttribute)));
        
        //when
        var value = classWithMemberAttributes.PublicValueWithSetterPropertyAttribute;

        //then
        Assert.That(RecordActivityAttribute.Activities.ContainsKey(activityKey), Is.False);
    }
    
    [Test]
    public void Should_Record_Activity_For_Set_Public_Property_With_Setter_Only_Attribute()
    {
        //given
        var propertyValue = DateTime.Now.Ticks;
        var activityKey = new ActivityKey(typeof(ClassWithMemberAttributes), MemberNameHelper.PropertySetterName(nameof(ClassWithMemberAttributes.PublicValueWithSetterPropertyAttribute)));
        
        //when
        classWithMemberAttributes.PublicValueWithSetterPropertyAttribute = propertyValue;
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
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Get_Public_Property_With_Getter_Only_Attribute()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMemberAttributes), MemberNameHelper.PropertyGetterName(nameof(ClassWithMemberAttributes.PublicValueWithGetterPropertyAttribute)));
        
        //when
        var value = classWithMemberAttributes.PublicValueWithGetterPropertyAttribute;
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
        }
    }
    
    [Test]
    public void Should_NOT_Record_Activity_For_Set_Public_Property_With_Getter_Only_Attribute()
    {
        //given
        var propertyValue = DateTime.Now.Ticks;
        var activityKey = new ActivityKey(typeof(ClassWithMemberAttributes), MemberNameHelper.PropertySetterName(nameof(ClassWithMemberAttributes.PublicValueWithGetterPropertyAttribute)));
        
        //when
        classWithMemberAttributes.PublicValueWithGetterPropertyAttribute = propertyValue;

        //then
        Assert.That(RecordActivityAttribute.Activities.ContainsKey(activityKey), Is.False);
    }
    
    [Test]
    public void Should_Record_Activity_For_Get_Public_Static_Property()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithMemberAttributes), MemberNameHelper.PropertyGetterName(nameof(ClassWithMemberAttributes.PublicStaticValueWithPropertyAttribute)));
        
        //when
        var value = ClassWithMemberAttributes.PublicStaticValueWithPropertyAttribute;
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
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Set_Public_Static_Property()
    {
        //given
        var propertyValue = DateTime.Now.Ticks;
        var activityKey = new ActivityKey(typeof(ClassWithMemberAttributes), MemberNameHelper.PropertySetterName(nameof(ClassWithMemberAttributes.PublicStaticValueWithPropertyAttribute)));
        
        //when
        ClassWithMemberAttributes.PublicStaticValueWithPropertyAttribute = propertyValue;
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
        }
    }
}
