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
    public JsonObject? Instantiate { get; set; } = null!;
    [JsonPropertyName("execute")]
    [JsonRequired]
    public JsonObject? Execute { get; set; } = null!;
    [JsonPropertyName("query")]
    [JsonRequired]
    public JsonObject? Query { get; set; } = null!;
    [JsonPropertyName("migrate")]
    [JsonRequired]
    public JsonObject? Migrate { get; set; } = null!;
    [JsonPropertyName("responses")]
    [JsonRequired]
    public JsonObject? Responses { get; set; } = null!;

    public async Task<IReadOnlyDictionary<string, JsonSchema>> GetResponseSchemasAsync()
    {
        var responseSchemas = new Dictionary<string, JsonSchema>();

        foreach(var responseNode in Responses ?? throw new InvalidOperationException("Schema is missing responses property"))
        {
            var responseKey = responseNode.Key;

            if (responseNode.Value is null)
            {
                throw new InvalidOperationException($"Response {responseKey} does not contain a schema");
            }

            try
            {
                responseSchemas.Add(responseKey, await JsonSchema.FromJsonAsync(responseNode.Value.ToJsonString()));
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"ResponseSchema {responseKey} could not be parsed: {ex.Message}", ex);
            }
        }

        return responseSchemas;
    }

    public async Task<JsonSchema> GetQuerySchemaAsync()
    {
        if (Query is null)
        {
            throw new InvalidOperationException("Schema is missing query property");
        }

        try
        {
            return await JsonSchema.FromJsonAsync(Query.ToJsonString());
        }
        catch(Exception)
        {
            throw new InvalidOperationException("query schema could not be parsed to JsonSchema");
        }
    }

    public async Task<JsonSchema> GetExecuteSchemaAsync()
    {
        if(Execute is null)
        {
            throw new InvalidOperationException("Schema is missing execute property");
        }

        try
        {
            return await JsonSchema.FromJsonAsync(Execute.ToJsonString());
        }
        catch(Exception)
        {
            throw new InvalidOperationException("execute schema could not be parsed to JsonSchema");
        }
    }
}
