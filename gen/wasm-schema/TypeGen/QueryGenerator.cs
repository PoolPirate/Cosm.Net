﻿using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.Common.Util;
using NJsonSchema;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public class QueryGenerator
{
    private readonly SchemaTypeGenerator _schemaTypeGenerator;

    public QueryGenerator(SchemaTypeGenerator schemaTypeGenerator)
    {
        _schemaTypeGenerator = schemaTypeGenerator;
    }

    public FunctionBuilder GenerateQueryFunction(JsonSchema querySchema, JsonSchema definitionsSource,
        IReadOnlyDictionary<string, JsonSchema> responseSchemas)
    {
        if(querySchema.Properties.Count != 1)
        {
            throw new NotSupportedException("Top level query schema must have only one property");
        }

        var propSchema = querySchema.Properties.Single().Value;
        string msgName = propSchema.Name;
        var argumentsSchema = SchemaTypeGenerator.GetInnerSchema(propSchema);

        string queryName = propSchema.Name;

        if(!responseSchemas.TryGetValue(queryName, out var responseSchema))
        {
            throw new NotSupportedException($"No response schema for query {queryName} found in schema");
        }

        var responseType = _schemaTypeGenerator.GetOrGenerateSchemaType(responseSchema, responseSchema, queryName);

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

        var paramTypes = argumentsSchema.Properties
            .Select(prop =>
            {
                string argName = NameUtils.ToValidVariableName(prop.Key);
                var argSchema = prop.Value;

                return new
                {
                    Type = _schemaTypeGenerator.GetOrGenerateSchemaType(argSchema, definitionsSource),
                    ArgName = argName,
                    ArgSchema = argSchema,
                };
            }).ToArray();

        foreach(var param in paramTypes.Where(x => x.Type.DefaultValue is null))
        {
            var argName = param.ArgName;
            var argSchema = param.ArgSchema;
            var paramType = param.Type;

            function
                .AddArgument(paramType.Name, argName,
                    paramType.DefaultValue is not null, paramType.DefaultValue, argSchema.Description)
                .AddStatement(new MethodCallBuilder("innerJsonRequest", "Add")
                    .AddArgument($"\"{argName}\"")
                    .AddArgument($"global::System.Text.Json.JsonSerializer.SerializeToNode({argName}, global::Cosm.Net.Encoding.Json.CosmWasmJsonUtils.SerializerOptions)")
                    .Build());
        }
        foreach(var param in paramTypes.Where(x => x.Type.DefaultValue is not null))
        {
            var argName = param.ArgName;
            var argSchema = param.ArgSchema;
            var paramType = param.Type;

            function
                .AddArgument(paramType.Name, argName,
                    paramType.DefaultValue is not null, paramType.DefaultValue, argSchema.Description)
                .AddStatement(new MethodCallBuilder("innerJsonRequest", "Add")
                    .AddArgument($"\"{argName}\"")
                    .AddArgument($"global::System.Text.Json.JsonSerializer.SerializeToNode({argName}, global::Cosm.Net.Encoding.Json.CosmWasmJsonUtils.SerializerOptions)")
                    .Build());
        }

        return function
            .AddArgument("global::System.Threading.CancellationToken", "cancellationToken", true, "default")
            .AddStatement("var encodedRequest = global::System.Text.Encoding.UTF8.GetBytes(jsonRequest.ToJsonString())")
            .AddStatement("var encodedResponse = await _wasm.SmartContractStateAsync(this, global::Google.Protobuf.ByteString.CopyFrom(encodedRequest), cancellationToken)")
            .AddStatement("var jsonResponse = global::System.Text.Encoding.UTF8.GetString(encodedResponse.Span)")
            .AddStatement($"var decodedResponse = global::System.Text.Json.JsonSerializer.Deserialize<{responseType.Name}>(jsonResponse, global::Cosm.Net.Encoding.Json.CosmWasmJsonUtils.SerializerOptions)")
            .AddStatement(
            """
            if (decodedResponse == default) 
            {
                throw new global::System.Text.Json.JsonException("Parsing contract response failed");
            }
            return decodedResponse
            """);
    }
}
