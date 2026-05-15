using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleBitware.AspectNet.Tests.LibraryBase.Serialization;

public sealed class CancellationTokenJsonConverter : JsonConverter<CancellationToken>
{
    public override CancellationToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return CancellationToken.None;
    }

    public override void Write(Utf8JsonWriter writer, CancellationToken value, JsonSerializerOptions options)
    {
        writer.WriteNullValue();
    }
}
