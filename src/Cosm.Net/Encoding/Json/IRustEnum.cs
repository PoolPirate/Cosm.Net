using System.Text.Json;

namespace Cosm.Net.Encoding.Json;
public interface IRustEnum<T>
{
    public abstract static void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options);
    public abstract static T ReadFromDocument(JsonDocument document);

    public static string GetEnumKey(JsonDocument document)
    => document.RootElement.ValueKind switch
    {
        JsonValueKind.String => document.RootElement.GetString()!,
        JsonValueKind.Object => document.RootElement.EnumerateObject().Single().Name,
        _ => throw new NotSupportedException()
    };
}
