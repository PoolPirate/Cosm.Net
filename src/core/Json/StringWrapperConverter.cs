using System.Text.Json.Serialization;
using System.Text.Json;

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