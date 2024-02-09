using System.Text.Json;
using TupleAsJsonArray;

namespace Cosm.Net.Json;
public static class CosmWasmJsonUtils
{
    public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters =
        {
            new TupleConverterFactory()
        }
    };

    public static void Test()
    {

    }
}
