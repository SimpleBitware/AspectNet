using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleBitware.AspectNet.Tests.LibraryBase.Serialization;

public sealed class TypeJsonConverter : JsonConverter<Type?>
{
    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        var typeName = reader.GetString();
        
        return string.IsNullOrEmpty(typeName) 
            ? null 
            : Type.GetType(typeName, throwOnError: false);
    }

    public override void Write(Utf8JsonWriter writer, Type? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStringValue(value.AssemblyQualifiedName);
    }
}
