using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.E2e;

public class ClassWithAspectNetAttributeDecoratedAsyncMethodsTests
{
    private readonly ClassWithAspectNetAttributeDecoratedMembers sut = new();
    
    [Test]
    public async Task Should_Record_Activity_For_Public_Async_Method()
    {
        //given
        var value = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicMethodAsync), 1);
        
        //when
        var result = await sut.PublicMethodAsync(value);
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
            Assert.That(result, Is.EqualTo(value));
            Assert.That(context.Exception, Is.Null);
            Assert.That(context.Parameters.Count, Is.EqualTo(1));
            Assert.That(context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
        }
    }
    
    [Test]
    public async Task Should_Record_Activity_For_Public_Async_Method_Exception()
    {
        //given
        var value = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicMethodExceptionAsync), 1);
        
        //when
        try
        {
            await sut.PublicMethodExceptionAsync(value);
        }
        catch(Exception ex)
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
            
            var context = (AspectNetAttributeContext)activity[1];
            Assert.That(context, Is.Not.Null);
            Assert.That(context.ReturnValue, Is.Null);
            Assert.That(context.Exception, Is.Not.Null);
            Assert.That(context.Parameters.Count, Is.EqualTo(1));
            Assert.That(context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
        }
    }
    
    [Test]
    public async Task Should_Record_Activity_For_Public_ValueTask_Method()
    {
        //given
        var value = Random.Shared.Next();
        var activityKey = new ActivityKey(typeof(ClassWithAspectNetAttributeDecoratedMembers), nameof(ClassWithAspectNetAttributeDecoratedMembers.PublicValueTaskMethod), 1);
        
        //when
        var result = await sut.PublicValueTaskMethod(value);
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
            Assert.That(result, Is.EqualTo(value));
            Assert.That(context.Exception, Is.Null);
            Assert.That(context.Parameters.Count, Is.EqualTo(1));
            Assert.That(context.Instance, Is.InstanceOf<ClassWithAspectNetAttributeDecoratedMembers>());
        }
    }
}
