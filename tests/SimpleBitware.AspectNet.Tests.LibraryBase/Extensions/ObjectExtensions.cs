using System.Text.Json;
using SimpleBitware.AspectNet.Tests.LibraryBase.Serialization;

namespace SimpleBitware.AspectNet.Tests.LibraryBase.Extensions;

public static class ObjectExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new();

    static ObjectExtensions()
    {
        JsonSerializerOptions.Converters.Add(new TypeJsonConverter());
        JsonSerializerOptions.Converters.Add(new CancellationTokenJsonConverter());
    }
    
    public static T DeepCopy<T>(this T input)
    {
        var json = JsonSerializer.Serialize(input, JsonSerializerOptions);
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions) ?? throw new InvalidOperationException();
    }

    public static T? NewInstance<T>(this T input)
    {
        if (input == null) return default;
        var type = input.GetType();
        return (T)Activator.CreateInstance(type)!;
    }
}
