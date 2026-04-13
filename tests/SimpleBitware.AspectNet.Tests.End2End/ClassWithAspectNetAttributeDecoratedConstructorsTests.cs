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
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters, Is.Empty);
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedConstructors>());
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
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters.Count, Is.EqualTo(numberOfParameters));
            Assert.That(activity.Context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedConstructors>());
        }
    }
    
    [Test]
    public void Should_Record_Activity_For_Static_Constructor()
    {
        //given
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedStaticConstructor), Constants.StaticConstructorMethodName);
        
        //when
        var value = ClassWithAspectNetAttributeDecoratedStaticConstructor.Value;
        var activities = ActivitiesStorage.Activities[activityKey];

        //then
        Assert.That(activities, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            var activity = activities.Last();
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Context.ReturnValue, Is.Null);
            Assert.That(activity.Context.Exception, Is.Null);
            Assert.That(activity.Context.Parameters, Is.Empty);
            Assert.That(activity.Context.Instance, Is.Null);
        }
    }
}
