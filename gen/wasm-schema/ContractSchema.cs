using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.Common.Util;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cosm.Net.Generators.CosmWasm;
public class ContractSchema
{
    [JsonPropertyName("contract_name")]
    public string ContractName { get; init; } = null!;
    [JsonPropertyName("contract_version")]
    public string ContractVersion { get; init; } = null!;
    [JsonPropertyName("idl_version")]
    public string IdlVersion { get; init; } = null!;

    [JsonPropertyName("instantiate")]
    public JsonObject Instantiate {  get; init; } = null!;
    [JsonPropertyName("execute")]
    public JsonObject Execute { get; init; } = null!;
    [JsonPropertyName("query")]
    public JsonObject Query { get; init; } = null!;
    [JsonPropertyName("migrate")]
    public JsonObject Migrate { get; init; } = null!;
    [JsonPropertyName("responses")]
    public JsonObject Responses { get; init; } = null!;

    private readonly Dictionary<string, ISyntaxBuilder> SourceComponents = [];
    private int RequestCounter = 0;
    private int ResponseCounter = 0;

    public async Task<string> GenerateCSharpCodeFileAsync()
    {
        var responseSchemas = new Dictionary<string, JsonSchema>();

        foreach(var responseNode in Responses)
        {
            responseSchemas.Add(responseNode.Key, await JsonSchema.FromJsonAsync(responseNode.Value!.ToJsonString()));
        }

        var contractClassBuilder = new ClassBuilder(NameUtils.ToValidClassName(ContractName))
            .WithVisibility(ClassVisibility.Internal)
            .AddFunctions(GenerateQueryFunctions(await JsonSchema.FromJsonAsync(Query.ToJsonString()), responseSchemas));

        var componentsSb = new StringBuilder();

        foreach(var component in SourceComponents)
        {
            componentsSb.AppendLine(component.Value.Build());
        }

        return
            $$"""
            namespace Cosmwasm.Contract.{{NameUtils.ToValidNamespaceName(ContractName)}};
            {{contractClassBuilder.Build(generateInterface: true)}}

            {{componentsSb}}
            """;
    }

    private IEnumerable<FunctionBuilder> GenerateQueryFunctions(JsonSchema queryMsgSchema, 
        IReadOnlyDictionary<string, JsonSchema> responseSchemas)
    {
        foreach(var querySchema in queryMsgSchema.OneOf)
        {
            if (querySchema.Properties.Count != 1)
            {
                throw new NotSupportedException();
            }

            var argumentsSchema = querySchema.Properties.Single().Value;
            var queryName = argumentsSchema.Name;
            
            if (!responseSchemas.TryGetValue(queryName, out var responseSchema))
            {
                throw new NotSupportedException();
            }

            string responseType = GetOrGenerateMergingSchemaType(responseSchema);

            var functions = new List<FunctionBuilder>
            {
                new FunctionBuilder(NameUtils.ToValidFunctionName(queryName))
                .WithVisibility(FunctionVisibility.Public)
                .WithReturnTypeRaw($"Task<{responseType}>")
                .WithSummaryComment(querySchema.Description)
                .AddStatement($"return Task.FromResult<{responseType}>(default!)")
            };

            var requiredProps = argumentsSchema.RequiredProperties.ToList();

            var sortedProperties = argumentsSchema.Properties
                .OrderBy(x =>
                {
                    var index = requiredProps.IndexOf(x.Key);
                    if(index == -1)
                    {
                        index = int.MaxValue;
                    }
                    return index;
                });

            foreach(var (argName, argSchema) in sortedProperties)
            {
                var paramTypes = GetOrGenerateSplittingSchemaType(argSchema, queryMsgSchema).ToArray();

                if (paramTypes.Length == 0)
                {
                    throw new NotSupportedException();
                }
                else if (paramTypes.Length == 1)
                {
                    var paramType = paramTypes[0];
                    foreach(var function in functions)
                    {
                        function.AddArgument(paramType, argName, paramType.EndsWith('?'));
                    }
                }
                else
                {
                    foreach(var function in functions.ToArray())
                    {
                        for(int i = 1; i < paramTypes.Length; i++)
                        {
                            functions.Add(function.Clone());
                        }
                    }

                    int paramIndex = 0;
                    for(int i = 0; i < functions.Count; i++)
                    {
                        paramIndex = (paramIndex + 1) % paramTypes.Length;
                        functions[i].AddArgument(paramTypes[paramIndex], argName, paramTypes[paramIndex].EndsWith('?'));
                    }
                }
            }

            foreach(var function in functions)
            {
                yield return function;
            }
        }
    }

    private IEnumerable<string> GetOrGenerateSplittingSchemaType(JsonSchema schema, 
        JsonSchema? definitionSource = null)
    {
        definitionSource ??= schema;

        if (schema.OneOf.Count != 0 && schema.OneOf.All(x => x.IsEnumeration))
        {
            yield return GetOrGenerateEnumerationType(schema, definitionSource);
            yield break;
        }
        if (schema.OneOf.Count != 0)
        {
            foreach(var oneOfSchema in schema.OneOf)
            {
                foreach(var innerType in GetOrGenerateSplittingSchemaType(oneOfSchema, definitionSource))
                {
                    yield return innerType;
                }
            }

            yield break;
        }
        if (schema.AnyOf.Count == 2 && schema.AnyOf.Count(x => x.Type != JsonObjectType.Null) == 1)
        {
            foreach(var innerType in GetOrGenerateSplittingSchemaType(
                schema.AnyOf.Single(x => x.Type != JsonObjectType.Null), definitionSource))
            {
                yield return $"{innerType}?";
            }

            yield break;
        }
        if (schema.HasReference && schema.AllOf.Count == 0)
        {
            foreach(var innerType in GetOrGenerateSplittingSchemaType(schema.Reference, definitionSource))
            {
                yield return innerType;
            }

            yield break;
        }
        if(schema.HasReference && schema.AllOf.Count == 1)
        {
            foreach(var innerType in GetOrGenerateSplittingSchemaType(schema.AllOf.Single(), definitionSource))
            {
                yield return innerType;
            }

            yield break;
        }

        if(schema.AnyOf.Count == 0 && schema.AnyOf.Count == 0 && schema.AllOf.Count == 0 && !schema.HasReference)
        {
            switch(schema.Type)
            {
                case JsonObjectType.Array:
                    foreach(var innerType in GetOrGenerateSplittingSchemaType(schema.Item, definitionSource))
                    {
                        yield return $"{innerType}[]";
                    }
                    break;
                case JsonObjectType.Object:
                    if (schema.Properties.Count == 0)
                    {
                        yield return "object";
                        break;
                    }
                    foreach(var innerType in GetOrGenerateSplittingObjectType(schema, definitionSource))
                    {
                        yield return innerType;
                    }
                    break;
                case JsonObjectType.Boolean:
                    yield return "bool";
                    break;
                case JsonObjectType.Boolean | JsonObjectType.Null:
                    yield return "bool?";
                    break;
                case JsonObjectType.Integer:
                    yield return "int";
                    break;
                case JsonObjectType.Integer | JsonObjectType.Null:
                    yield return "int?";
                    break;
                case JsonObjectType.Number:
                    yield return "double";
                    break;
                case JsonObjectType.Number | JsonObjectType.Null:
                    yield return "double?";
                    break;
                case JsonObjectType.String:
                    yield return "string";
                    break;
                case JsonObjectType.String | JsonObjectType.Null:
                    yield return "string?";
                    break;

                case JsonObjectType.File:
                case JsonObjectType.None:
                case JsonObjectType.Null:
                default:
                    throw new NotSupportedException();
            }

            yield break;
        }

        throw new NotSupportedException();
    }

    private IEnumerable<string> GetOrGenerateSplittingObjectType(JsonSchema objectSchema, JsonSchema definitionsSource)
    {
        if (objectSchema.Type != JsonObjectType.Object || objectSchema.ActualProperties.Count == 0)
        {
            throw new InvalidOperationException();
        }

        var definitionName = definitionsSource.Definitions
            .FirstOrDefault(x => x.Value == objectSchema).Key;

        string typeName = objectSchema.Title ?? definitionName ?? $"Request{RequestCounter++}";

        if (!SourceComponents.ContainsKey(typeName))
        {
            var classBuilder = new ClassBuilder(typeName);

            foreach(var property in objectSchema.ActualProperties)
            {
                var schemaTypes = GetOrGenerateSplittingSchemaType(property.Value, definitionsSource);

                if (schemaTypes.Count() != 1)
                {
                    //ToDo: Create abstract base class with static creator functions
                    //and create internal implementation class for each path
                    throw new NotImplementedException();
                }

                var schemaType = schemaTypes.Single();
                classBuilder.AddProperty(
                    new PropertyBuilder(
                        schemaType,
                        NameUtils.ToValidPropertyName(property.Key))
                    .WithSetterVisibility(SetterVisibility.Init)
                    .WithIsRequired(!schemaType.EndsWith('?'))
                    .WithJsonPropertyName(property.Key)
                    .WithSummaryComment(property.Value.Description)
                );
            }

            SourceComponents.Add(typeName, classBuilder);
        }

        yield return typeName;
    }

    private string GetOrGenerateEnumerationType(JsonSchema parentSchema,  JsonSchema definitionsSource)
    {
        if(parentSchema.Type != JsonObjectType.None || parentSchema.ActualProperties.Count != 0 || parentSchema.OneOf.Count == 0)
        {
            throw new InvalidOperationException();
        }

        var definitionName = definitionsSource.Definitions
            .FirstOrDefault(x => x.Value == parentSchema).Key;

        string typeName = parentSchema.Title ?? definitionName ?? throw new NotSupportedException();

        if(!SourceComponents.ContainsKey(typeName))
        {
            var enumerationBuilder = new EnumerationBuilder(typeName)
                .WithSummaryComment(parentSchema.Description);

            foreach(var oneOf in parentSchema.OneOf)
            {
                string enumerationValue = oneOf.Enumeration.Single().ToString()
                        ?? throw new NotSupportedException();

                enumerationBuilder.AddValue(
                    NameUtils.ToValidPropertyName(enumerationValue),
                    enumerationValue,
                    oneOf.Description);
            }

            SourceComponents.Add(typeName, enumerationBuilder);
        }

        return typeName;
    }

    private string GetOrGenerateMergingSchemaType(JsonSchema schema,
    JsonSchema? definitionSource = null)
    {
        definitionSource ??= schema;

        if(schema.OneOf.Count != 0 && schema.OneOf.All(x => x.IsEnumeration))
        {
            return GetOrGenerateEnumerationType(schema, definitionSource);
        }
        if(schema.OneOf.Count != 0)
        {
            return GetOrGenerateMergedType(schema.OneOf, definitionSource);
        }
        if(schema.AnyOf.Count == 2 && schema.AnyOf.Count(x => x.Type != JsonObjectType.Null) == 1)
        {
            return $"{GetOrGenerateMergingSchemaType(schema.AnyOf.Single(x => x.Type != JsonObjectType.Null), definitionSource)}?";
        }
        if(schema.HasReference && schema.AllOf.Count == 0)
        {
            return GetOrGenerateMergingSchemaType(schema.Reference, definitionSource);
        }
        if(schema.HasReference && schema.AllOf.Count == 1)
        {
            return GetOrGenerateMergingSchemaType(schema.AllOf.Single(), definitionSource);
        }

        if(schema.AnyOf.Count == 0 && schema.AnyOf.Count == 0 && schema.AllOf.Count == 0 && !schema.HasReference)
        {
            switch(schema.Type)
            {
                case JsonObjectType.Array:
                    return $"{GetOrGenerateMergingSchemaType(schema.Item, definitionSource)}[]";
                case JsonObjectType.Object:
                    if(schema.Properties.Count == 0)
                    {
                        return "object";
                    }
                    return GetOrGenerateMergingObjectType(schema, definitionSource);
                case JsonObjectType.Boolean:
                    return "bool";
                case JsonObjectType.Boolean | JsonObjectType.Null:
                    return "bool?";
                case JsonObjectType.Integer:
                    return "int";
                case JsonObjectType.Integer | JsonObjectType.Null:
                    return "int?";
                case JsonObjectType.Number:
                    return "double";
                case JsonObjectType.Number | JsonObjectType.Null:
                    return "double?";
                case JsonObjectType.String:
                    return "string";
                case JsonObjectType.String | JsonObjectType.Null:
                    return "string?";

                case JsonObjectType.File:
                case JsonObjectType.None:
                case JsonObjectType.Null:
                default:
                    throw new NotSupportedException();
            }
        }

        throw new NotSupportedException();
    }

    private string GetOrGenerateMergingObjectType(JsonSchema objectSchema, JsonSchema definitionsSource)
    {
        if(objectSchema.Type != JsonObjectType.Object || objectSchema.ActualProperties.Count == 0)
        {
            throw new InvalidOperationException();
        }

        string typeName = objectSchema.Title ?? $"Response{ResponseCounter++}" 
            ?? throw new NotSupportedException();

        if(!SourceComponents.ContainsKey(typeName))
        {
            var classBuilder = new ClassBuilder(typeName);

            foreach(var property in objectSchema.ActualProperties)
            {
                var schemaType = GetOrGenerateMergingSchemaType(property.Value, definitionsSource);
                classBuilder.AddProperty(
                    new PropertyBuilder(
                        schemaType,
                        NameUtils.ToValidPropertyName(property.Key))
                    .WithSetterVisibility(SetterVisibility.Init)
                    .WithIsRequired(!schemaType.EndsWith('?'))
                    .WithJsonPropertyName(property.Key)
                    .WithSummaryComment(property.Value.Description)
                );
            }

            SourceComponents.Add(typeName, classBuilder);
        }

        return typeName;
    }

    private string GetOrGenerateMergedType(IEnumerable<JsonSchema> schemas, JsonSchema definitionsSource)
    {
        string typeName = $"Response{ResponseCounter++}";
        var classBuilder = new ClassBuilder(typeName);

        int i = 0;
        foreach(var schema in schemas)
        {
            string type = GetOrGenerateMergingSchemaType(schema, definitionsSource);

            classBuilder.AddProperty(
                new PropertyBuilder(
                    type,
                    $"ParamOption{i++}"
                )
                .WithSetterVisibility(SetterVisibility.Init)
                .WithIsRequired(!type.EndsWith('?'))
            );
        }

        SourceComponents.Add(typeName, classBuilder);
        return typeName;
    }
}
