using NJsonSchema;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Cosm.Net.Generators.CosmWasm.Models;
public class ContractAPISchema
{
    [JsonPropertyName("contract_name")]
    [JsonRequired]
    public string ContractName { get; set; } = null!;
    [JsonPropertyName("contract_version")]
    [JsonRequired]
    public string ContractVersion { get; set; } = null!;
    [JsonPropertyName("idl_version")]
    [JsonRequired]
    public string IdlVersion { get; set; } = null!;

    [JsonPropertyName("instantiate")]
    [JsonRequired]
    public JsonObject Instantiate { get; set; } = null!;
    [JsonPropertyName("execute")]
    [JsonRequired]
    public JsonObject Execute { get; set; } = null!;
    [JsonPropertyName("query")]
    [JsonRequired]
    public JsonObject Query { get; set; } = null!;
    [JsonPropertyName("migrate")]
    [JsonRequired]
    public JsonObject Migrate { get; set; } = null!;
    [JsonPropertyName("responses")]
    [JsonRequired]
    public JsonObject Responses { get; set; } = null!;

    public async Task<IReadOnlyDictionary<string, JsonSchema>> GetResponseSchemasAsync()
    {
        var responseSchemas = new Dictionary<string, JsonSchema>();

        foreach(var responseNode in Responses)
        {
            responseSchemas.Add(responseNode.Key, await JsonSchema.FromJsonAsync(responseNode.Value!.ToJsonString()));
        }

        return responseSchemas;
    }

    public async Task<JsonSchema> GetQuerySchemaAsync() 
        => await JsonSchema.FromJsonAsync(Query.ToJsonString());

    public async Task<JsonSchema> GetExecuteSchemaAsync()
        => await JsonSchema.FromJsonAsync(Execute.ToJsonString());
}
