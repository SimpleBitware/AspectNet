using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.End2End.Helpers;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End;

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
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.EqualTo(value));
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters, Is.Empty);
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Set_Public_Property()
    {
        //given
        var propertyValue = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertySetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueWithPropertyAttribute)), 1);
        
        //when
        sut.PublicValueWithPropertyAttribute = propertyValue;
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters[Constants.SetterParameterName], Is.EqualTo(propertyValue));
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
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
        Assert.That(ActivitiesStorage.Activities.ContainsKey(activityKey), Is.False);
    }
    
    [Test]
    public void Should_Record_Activity_For_Set_Public_Property_With_Setter_Only_Attribute()
    {
        //given
        var propertyValue = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertySetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueWithSetterPropertyAttribute)), 1);
        
        //when
        sut.PublicValueWithSetterPropertyAttribute = propertyValue;
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters[Constants.SetterParameterName], Is.EqualTo(propertyValue));
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Get_Public_Property_With_Getter_Only_Attribute()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertyGetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueWithGetterPropertyAttribute)));
        
        //when
        var value = sut.PublicValueWithGetterPropertyAttribute;
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.EqualTo(value));
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters, Is.Empty);
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
        }
    }
    
    [Test]
    public void Should_NOT_Record_Activity_For_Set_Public_Property_With_Getter_Only_Attribute()
    {
        //given
        var propertyValue = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertySetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueWithGetterPropertyAttribute)));
        
        //when
        sut.PublicValueWithGetterPropertyAttribute = propertyValue;

        //then
        Assert.That(ActivitiesStorage.Activities.ContainsKey(activityKey), Is.False);
    }
    
    [Test]
    public void Should_Record_Activity_For_Get_Public_Static_Property()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertyGetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicStaticValueWithPropertyAttribute)));
        
        //when
        var value = ClassWithAspectNetAttributeDecoratedMembers.PublicStaticValueWithPropertyAttribute;
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.EqualTo(value));
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters, Is.Empty);
            Assert.That(activity.Context.Instance, Is.Null);
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Set_Public_Static_Property()
    {
        //given
        var propertyValue = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), MemberNameHelper.PropertySetterName(nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicStaticValueWithPropertyAttribute)), 1);
        
        //when
        ClassWithAspectNetAttributeDecoratedMembers.PublicStaticValueWithPropertyAttribute = propertyValue;
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters[Constants.SetterParameterName], Is.EqualTo(propertyValue));
            Assert.That(activity.Context.Instance, Is.Null);
        }
    }
}
