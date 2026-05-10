namespace SimpleBitware.AspectNet.Tests.End2End.Helpers;

public class ActivityComparer : Comparer<ExpectedActivity>
{
    public static ActivityComparer Instance { get; } = new();

    public override int Compare(ExpectedActivity? x, ExpectedActivity? y)
    {
        return (x is not null &&
                y is not null &&
                x.AspectType == y.AspectType &&
                x.AspectPriority == y.AspectPriority &&
                x.AspectMethodName == y.AspectMethodName) &&
               (
                   x.Context.ClassType == y.Context.ClassType &&
                   x.Context.MemberName == y.Context.MemberName &&
                   x.Context.Parameters.Keys.SequenceEqual(y.Context.Parameters.Keys) &&
                   ((x.Context.Instance is null && y.Context.Instance is null) || (x.Context.Instance == y.Context.Instance)) &&
                   ((x.Context.Exception is null && y.Context.Exception is null) || (x.Context.Exception?.GetType() == y.Context.Exception?.GetType())) &&
                   ((x.Context.ReturnValue is null && y.Context.ReturnValue is null) || (x.Context.ReturnValue?.ToString() == y.Context.ReturnValue?.ToString())) &&
                   ((x.Context.Data is null && y.Context.Data is null) || (x.Context.Data == y.Context.Data))
               )
            ? 0
            : 1;
    }
}
