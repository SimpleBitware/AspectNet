namespace SimpleBitware.AspectNet.Extensions;

public static class LinqExtensions
{
    public static void Each<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
            action(item);
    }
}
