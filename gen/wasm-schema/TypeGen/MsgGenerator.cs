using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.Common.Util;
using NJsonSchema;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public class MsgGenerator
{
    private readonly SchemaTypeGenerator _schemaTypeGenerator;

    public MsgGenerator(SchemaTypeGenerator schemaTypeGenerator)
    {
        _schemaTypeGenerator = schemaTypeGenerator;
    }

    public FunctionBuilder GenerateMsgFunction(JsonSchema querySchema, JsonSchema definitionsSource,
        IReadOnlyDictionary<string, JsonSchema> responseSchemas)
    {
        if(querySchema.Properties.Count != 1)
        {
            throw new NotSupportedException("Top level msg schema must have only one property");
        }

        var propSchema = querySchema.Properties.Single().Value;
        string msgName = propSchema.Name;

        var argumentsSchema = SchemaTypeGenerator.GetInnerSchema(propSchema);

        var function = new FunctionBuilder($"{NameUtils.ToValidFunctionName(msgName)}")
                .WithVisibility(FunctionVisibility.Public)
                .WithReturnTypeRaw($"global::Cosm.Net.Tx.Msg.IWasmTxMessage")
                .AddStatement(new ConstructorCallBuilder("global::System.Text.Json.Nodes.JsonObject")
                    .ToVariableAssignment("innerJsonRequest"))
                .AddStatement(new ConstructorCallBuilder("global::System.Text.Json.Nodes.JsonObject")
                    .AddArgument($"""
                    [
                        new global::System.Collections.Generic.KeyValuePair<
                            global::System.String, global::System.Text.Json.Nodes.JsonNode?>(
                            "{msgName}", innerJsonRequest
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
                .AddStatement(
                $$"""
                if ({{argName}} != {{paramType.DefaultValue}}) {{{new MethodCallBuilder("innerJsonRequest", "Add")
                        .AddArgument($"\"{argName}\"")
                        .AddArgument($"global::System.Text.Json.JsonSerializer.SerializeToNode({argName}, global::Cosm.Net.Encoding.Json.CosmWasmJsonUtils.SerializerOptions)")
                        .Build()}};
                }
                """, false);
        }

        return function
            .AddArgument("global::System.Collections.Generic.IEnumerable<global::Cosm.Net.Models.Coin>?", "funds", true, "null")
            .AddArgument("global::System.String?", "txSender", true, "null")
            .AddStatement("funds = funds ?? global::System.Array.Empty<global::Cosm.Net.Models.Coin>()")
            .AddStatement("return " + new MethodCallBuilder("_wasm", "EncodeContractCall")
                .AddArgument("this")
                .AddArgument("jsonRequest")
                .AddArgument("funds")
                .AddArgument("txSender")
                .Build());
    }
}
