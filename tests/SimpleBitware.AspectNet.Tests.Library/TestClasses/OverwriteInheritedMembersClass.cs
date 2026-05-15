using SimpleBitware.AspectNet.Tests.LibraryBase.Attributes;
using SimpleBitware.AspectNet.Tests.LibraryBase.TestClasses;
using SimpleBitware.AspectNet.Tests.Library.Attributes;

namespace SimpleBitware.AspectNet.Tests.Library.TestClasses;

public class OverwriteInheritedMembersClass<T> : TestClassBase<T>
{
    [ExtendedRecordActivity(Priority = 3)]
    public override void VoidMethod()
    {
        Console.WriteLine("EmptyMethod called");
        if (DateTime.Now == DateTime.Parse("2000-01-01"))
            throw new Exception();
    }

    [HideException]
    public sealed override async Task<T> AsyncTaskMethod(T? parameter, CancellationToken cancellationToken)
    {
        return await base.AsyncTaskMethod(parameter, cancellationToken);
    }
}
