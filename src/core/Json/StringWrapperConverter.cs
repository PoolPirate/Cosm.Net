using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cosm.Net.Json;
public class StringWrapperConverter : JsonConverter<StringWrapper>
{
    public override StringWrapper? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new StringWrapper()
        {
            Value = reader.GetString()
        };

    public override void Write(Utf8JsonWriter writer, StringWrapper value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}