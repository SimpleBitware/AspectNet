using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Tests.Weaving.Attributes;

namespace SimpleBitware.AspectNet.Tests.Weaving.TestClasses;

public class TestClassBase<T>
{
    [RecordActivity(Priority = 5)]
    protected TestClassBase()
    {
        Console.WriteLine("TestClassBase<T> Constructor called");
    }
    
    protected TestClassBase(T? initialValue)
    {
        Console.WriteLine("TestClassBase<T> Constructor with initialValue called");
        NullablePropertyWithExcludedSet = initialValue;
    }
    
    [RecordActivity(Priority = 10)]
    public required T PropertyWithLogic
    {
        [ModifyState]
        get
        {
            Console.WriteLine("Property getter called");
            return field?.ToString() == "throw"
                ? throw new Exception()
                : field;
        }
        [RecordActivity(Priority = 5)]
        set
        {
            Console.WriteLine("Property setter called");
            if (EqualityComparer<T>.Default.Equals(value, field))
                return;
            field = value;
        }
    }

    public T? NullablePropertyWithExcludedSet
    {
        get;
        [AspectNetExclude]
        set;
    }

    [AspectNetExclude]
    public virtual void ExcludedMethod()
    {
        Console.WriteLine("ExcludedMethod called");
    }

    [RecordActivity(Priority = 10)]
    public virtual void VoidMethod()
    {
        Console.WriteLine("EmptyMethod called");
        if (DateTime.Now == DateTime.Parse("2000-01-01"))
            throw new Exception();
    }
    
    public virtual T? MethodWithReturn()
    {
        Console.WriteLine("MethodWithReturn called");
        return default(T);
    }

    [RecordActivity(Priority = 5)]
    public virtual T MethodWithReturnAndOneParameter(T? parameter)
    {
        Console.WriteLine("MethodWithReturnAndOneParameter called");
        return (parameter == null || EqualityComparer<T>.Default.Equals(parameter, default(T)))
            ? throw new ArgumentException("Parameter cannot be default value", nameof(parameter))
            : parameter;
    }

    [RecordActivity(Priority = 5)]
    public virtual int MethodWithReturnAndParameters(params T[] parameters)
    {
        Console.WriteLine("MethodWithReturnAndParameters called");
        return (parameters.Length == 0)
            ? throw new ArgumentException("No parameters provided", nameof(parameters))
            : parameters.Length;
    }

    [RecordActivity(Priority = 5)]
    public virtual Task<T> TaskMethod(T? parameter, CancellationToken cancellationToken)
    {
        Console.WriteLine("TaskMethod called");
        if (parameter == null || EqualityComparer<T>.Default.Equals(parameter, default(T)))
            throw new ArgumentException("Parameter cannot be default value", nameof(parameter));

        return Task.Delay(1000, cancellationToken)
            .ContinueWith(x =>
            {
                Console.WriteLine("Executing task pipeline");
                return Task.Delay(1, cancellationToken);
            }, cancellationToken)
            .Unwrap()
            .ContinueWith<T>(x => parameter, cancellationToken);
    }
    
    public virtual async Task<T> AsyncTaskMethod(T? parameter, CancellationToken cancellationToken)
    {
        Console.WriteLine("AsyncTaskMethod called");
        await Task.Delay(1, cancellationToken);

        if (parameter == null || EqualityComparer<T>.Default.Equals(parameter, default(T)))
            throw new ArgumentException("Parameter cannot be default value", nameof(parameter));

        var task1 = Task.Delay(1000, cancellationToken);
        var task2 = Task.Delay(10, cancellationToken);
        await Task.WhenAny(task1, task2);

        return parameter;
    }

    public ValueTask<int> ValueTaskMethod(params T[] parameters)
    {
        Console.WriteLine("ValueTaskMethod called");
        if (parameters.Length == 0)
            throw new ArgumentException("No parameters provided", nameof(parameters));

        return ValueTask.FromResult(parameters.Length);
    }
    
    [RecordActivity(Priority = 10)]
    public virtual async ValueTask<T> AsyncValueTaskMethod(T? parameter, CancellationToken cancellationToken)
    {
        Console.WriteLine("AsyncValueTaskMethod called");
        if (parameter == null || EqualityComparer<T>.Default.Equals(parameter, default(T)))
            throw new ArgumentException("Parameter cannot be default value", nameof(parameter));

        return await await Task.Delay(1000, cancellationToken)
            .ContinueWith(_ =>
            {
                Console.WriteLine("Executing task pipeline");
                return Task.Delay(1, cancellationToken)
                    .ContinueWith<T>(_ => parameter, cancellationToken);
            }, cancellationToken);
    }
}
