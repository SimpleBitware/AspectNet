using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

public class OverwriteBaseClassMembersClass<T> : TestClassBase<T>
{
    [ExtendedRecordActivity(Priority = 3)]
    public override void VoidMethod()
    {
        Console.WriteLine("EmptyMethod called");
        if (DateTime.Now == DateTime.Parse("2000-01-01"))
            throw new Exception();
    }

    [ModifyState]
    public sealed override async Task<T> AsyncTaskMethod(T? parameter, CancellationToken cancellationToken)
    {
        return await base.AsyncTaskMethod(parameter, cancellationToken);
    }
}
