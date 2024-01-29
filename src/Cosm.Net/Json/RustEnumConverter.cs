﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cosm.Net.Json;
public class RustEnumConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : IRustEnum<TEnum>
{
    private readonly static JsonConverter<JsonDocument> _defaultConverter =
        (JsonConverter<JsonDocument>) JsonSerializerOptions.Default.GetConverter(typeof(JsonDocument));

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDocument = _defaultConverter.Read(ref reader, typeToConvert, options)
            ?? throw new JsonException("Failed to parse");

        return TEnum.ReadFromDocument(jsonDocument);
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        => TEnum.Write(writer, value, options);
}
