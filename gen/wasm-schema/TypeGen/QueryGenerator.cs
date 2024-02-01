using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.Common.Util;
using NJsonSchema;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public static class QueryGenerator
{
    public static FunctionBuilder GenerateQueryFunction(JsonSchema querySchema, JsonSchema definitionsSource,
        IReadOnlyDictionary<string, JsonSchema> responseSchemas)
    {
        if(querySchema.Properties.Count != 1)
        {
            throw new NotSupportedException();
        }

        //ToDo: Consider skipping layers till there's more than 1 property

        var argumentsSchema = querySchema.Properties.Single().Value;
        string queryName = argumentsSchema.Name;

        if(!responseSchemas.TryGetValue(queryName, out var responseSchema))
        {
            throw new NotSupportedException();
        }

        var responseType = SchemaTypeGenerator.GetOrGenerateSchemaType(responseSchema, responseSchema);

        var function = new FunctionBuilder($"{NameUtils.ToValidFunctionName(queryName)}Async")
                .WithVisibility(FunctionVisibility.Public)
                .WithReturnTypeRaw($"Task<{responseType.Name}>")
                .WithIsAsync()
                .AddStatement(new ConstructorCallBuilder("global::System.Text.Json.Nodes.JsonObject")
                    .ToVariableAssignment("innerJsonRequest"))
                .AddStatement(new ConstructorCallBuilder("global::System.Text.Json.Nodes.JsonObject")
                    .AddArgument($"""
                    [
                        new global::System.Collections.Generic.KeyValuePair<
                            global::System.String, global::System.Text.Json.Nodes.JsonNode?>(
                            "{queryName}", innerJsonRequest
                        )
                    ]
                    """)
                    .ToVariableAssignment("jsonRequest"));

        if(querySchema.Description is not null)
        {
            function.WithSummaryComment(querySchema.Description);
        }

        var requiredProps = argumentsSchema.RequiredProperties.ToList();
        var sortedProperties = argumentsSchema.Properties
            .OrderBy(x =>
            {
                int index = requiredProps.IndexOf(x.Key);
                if(index == -1)
                {
                    index = Int32.MaxValue;
                }
                return index;
            });

        foreach(var property in sortedProperties)
        {
            string argName = property.Key;
            var argSchema = property.Value;

            var paramType = SchemaTypeGenerator.GetOrGenerateSchemaType(argSchema, definitionsSource);

            function
            .AddArgument(paramType.Name, argName,
                paramType.DefaultValue is not null, paramType.DefaultValue, argSchema.Description)
            .AddStatement(new MethodCallBuilder("innerJsonRequest", "Add")
                .AddArgument($"\"{argName}\"")
                .AddArgument($"global::System.Text.Json.JsonSerializer.SerializeToNode((object?) {argName})")
                .Build());
        }

        return function
            .AddStatement("var encodedRequest = global::System.Text.Encoding.UTF8.GetBytes(jsonRequest.ToJsonString())")
            .AddStatement("var encodedResponse = await _wasm.SmartContractStateAsync(this, global::Google.Protobuf.ByteString.CopyFrom(encodedRequest))")
            .AddStatement("var jsonResponse = global::System.Text.Encoding.UTF8.GetString(encodedResponse.Span)")
            .AddStatement($"return global::System.Text.Json.JsonSerializer.Deserialize<{responseType.Name}>(jsonResponse) ?? throw new global::System.Text.Json.JsonException(\"Parsing contract response failed\")");
    }
}
