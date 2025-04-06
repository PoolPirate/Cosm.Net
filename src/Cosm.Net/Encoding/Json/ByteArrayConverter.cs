using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cosm.Net.Encoding.Json;
public class ByteArrayConverter : JsonConverter<byte[]?>
{
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new NotSupportedException("Json Value could not be parsed into byte array");
        }

        short[] sByteArray = JsonSerializer.Deserialize<short[]>(ref reader) ?? Array.Empty<short>();
        byte[] value = new byte[sByteArray.Length];
        for(int i = 0; i < sByteArray.Length; i++)
        {
            value[i] = (byte) sByteArray[i];
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();

        foreach(var val in value)
        {
            writer.WriteNumberValue(val);
        }

        writer.WriteEndArray();
    }
}
