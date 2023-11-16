using System.Text.Json.Serialization;

namespace Cosm.Net.Json;
[JsonConverter(typeof(StringWrapperConverter))]
public class StringWrapper
{
    public string? Value { get; set; }
}
