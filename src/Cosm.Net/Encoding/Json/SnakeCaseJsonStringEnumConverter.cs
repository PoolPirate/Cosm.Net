using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cosm.Net.Encoding.Json;
public class SnakeCaseJsonStringEnumConverter<TEnum> : JsonStringEnumConverter<TEnum>
            where TEnum : struct, Enum
{
    public SnakeCaseJsonStringEnumConverter()
        : base(namingPolicy: JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false)
    {
    }
}