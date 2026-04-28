using System.Text.Json;

namespace SimpleBitware.AspectNet.Tests.Weaving.Extensions;

public static class ObjectExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new();

    static ObjectExtensions()
    {
        JsonSerializerOptions.Converters.Add(new TypeJsonConverter());
    }
    
    public static T DeepCopy<T>(this T input)
    {
        var json = JsonSerializer.Serialize(input, JsonSerializerOptions);
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions) ?? throw new InvalidOperationException();
    }
}
