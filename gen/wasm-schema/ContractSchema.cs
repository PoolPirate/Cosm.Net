using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.Common.Util;
using Microsoft.CodeAnalysis;
using NJsonSchema;
using System.Data;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Cosm.Net.Generators.CosmWasm;
public class ContractSchema
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

    private readonly Dictionary<string, int> _typeNames = [];
    private readonly Dictionary<SyntaxId, ITypeBuilder> _sourceComponentsDict = [];
    private readonly List<ITypeBuilder> _sourceComponents = [];

    public async Task<string> GenerateCSharpCodeFileAsync(string targetInterface, string targetNamespace)
    {
        var responseSchemas = new Dictionary<string, JsonSchema>();

        foreach(var responseNode in Responses)
        {
            responseSchemas.Add(responseNode.Key, await JsonSchema.FromJsonAsync(responseNode.Value!.ToJsonString()));
        }

        string contractClassName = targetInterface;
        if(contractClassName.StartsWith("I"))
        {
            contractClassName = contractClassName.Substring(1);
        }
        else
        {
            contractClassName += "Implementation";
        }

        var contractClassBuilder = new ClassBuilder(contractClassName)
            .WithVisibility(ClassVisibility.Internal)
            .WithIsPartial(true)
            .AddField(new FieldBuilder("global::Cosm.Net.Adapters.IWasmAdapater", "_wasm"))
            .AddField(new FieldBuilder("global::System.String", "_contractAddress"))
            .AddBaseType("global::Cosm.Net.Wasm.Models.IContract", true)
            .AddFunctions(GenerateQueryFunctions(await JsonSchema.FromJsonAsync(Query.ToJsonString()), responseSchemas))
            .AddFunctions(GenerateMsgFunctions(await JsonSchema.FromJsonAsync(Execute.ToJsonString())));

        var componentsSb = new StringBuilder();

        foreach(var component in _sourceComponents)
        {
            componentsSb.AppendLine(component.Build());
        }

        return
            $$"""
            #nullable enable
            namespace {{targetNamespace}};
            {{contractClassBuilder.Build(generateFieldConstructor: true, generateInterface: true, interfaceName: targetInterface)}}

            {{componentsSb}}
            """;
    }

    private string TryAddSourceComponent(string typeName, ITypeBuilder builder)
    {
        if(!_sourceComponentsDict.TryGetValue(builder.GetContentId(), out var typeBuilder))
        {
            return ForceAddSourceComponent(typeName, builder);
        }
        else
        {
            //
            return typeBuilder.GetSyntaxId() != builder.GetSyntaxId() 
                ? ForceAddSourceComponent(typeName, builder) 
                : typeName;
        }
    }

    private string ForceAddSourceComponent(string typeName, ITypeBuilder builder)
    {
        _typeNames.TryGetValue(typeName, out int usages);
        _typeNames[typeName] = usages + 1;

        if(usages != 0)
        {
            typeName = $"{typeName}{usages}";
            switch(builder)
            {
                case ClassBuilder c:
                    c.WithName(typeName);
                    break;
                case EnumerationBuilder b:
                    b.WithName(typeName);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        _sourceComponentsDict[builder.GetContentId()] = builder;
        _sourceComponents.Add(builder);
        return typeName;
    }

    private void ForceRemoveSourceComponent(ITypeBuilder builder)
    {
        _sourceComponentsDict.Remove(builder.GetContentId());
        _sourceComponents.Remove(builder);
    }

    private IEnumerable<FunctionBuilder> GenerateMsgFunctions(JsonSchema txMsgSchema)
    {
        foreach(var txSchema in txMsgSchema.OneOf)
        {
            if(txSchema.Properties.Count != 1)
            {
                throw new NotSupportedException();
            }

            var argumentsSchema = txSchema.Properties.Single().Value;
            var msgName = argumentsSchema.Name;
            
            var functions = new List<FunctionBuilder>
            {
                new FunctionBuilder($"{NameUtils.ToValidFunctionName(msgName)}")
                .WithVisibility(FunctionVisibility.Public)
                .WithReturnTypeRaw($"global::Cosm.Net.Tx.Msg.ITxMessage")
                .WithSummaryComment(txSchema.Description)
                .AddArgument("global::System.Collections.Generic.IEnumerable<global::Cosm.Net.Models.Coin>", "funds")
                .AddStatement(new ConstructorCallBuilder("global::System.Text.Json.Nodes.JsonObject")
                    .ToVariableAssignment("innerJsonRequest"))
                .AddStatement(new ConstructorCallBuilder("global::System.Text.Json.Nodes.JsonObject")
                    .AddArgument($"""
                    [
                        new global::System.Collections.Generic.KeyValuePair<
                            global::System.String, global::System.Text.Json.Nodes.JsonNode?>(
                            "{msgName}", innerJsonRequest
                        )!
                    ]
                    """)
                    .ToVariableAssignment("jsonRequest"))

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

            foreach(var property in sortedProperties)
            {
                var argName = property.Key;
                var argSchema = property.Value;

                var paramTypes = GetOrGenerateSplittingSchemaType(argSchema, txMsgSchema).ToArray();

                if(paramTypes.Length == 0)
                {
                    throw new NotSupportedException();
                }
                else if(paramTypes.Length == 1)
                {
                    var paramType = paramTypes[0];
                    foreach(var function in functions)
                    {
                        function
                            .AddArgument(paramType.Name, argName,
                                paramType.HasDefaultValue, paramType.ExplicitDefaultValue)
                            .AddStatement(new MethodCallBuilder("innerJsonRequest", "Add")
                                    .AddArgument($"\"{argName}\"")
                                    .AddArgument($"global::System.Text.Json.JsonSerializer.SerializeToNode({argName})")
                                    .Build());
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
                        functions[i]
                            .AddArgument(paramTypes[paramIndex].Name, argName,
                                paramTypes[paramIndex].HasDefaultValue, paramTypes[paramIndex].ExplicitDefaultValue)
                            .AddStatement(new MethodCallBuilder("innerJsonRequest", "Add")
                                .AddArgument($"\"{argName}\"")
                                .AddArgument($"global::System.Text.Json.JsonSerializer.SerializeToNode((object?) {argName})")
                                .Build());
                    }
                }
            }

            foreach(var function in functions)
            {
                yield return function
                    .AddArgument("global::System.String?", "txSender", true, "null")
                    .AddStatement("var encodedRequest = global::System.Text.Encoding.UTF8.GetBytes(jsonRequest.ToJsonString())")
                    .AddStatement("var response = " + new MethodCallBuilder("_wasm", "EncodeContractCall")
                        .AddArgument("_contractAddress")
                        .AddArgument("global::Google.Protobuf.ByteString.CopyFrom(encodedRequest)")
                        .AddArgument("funds")
                        .AddArgument("txSender")
                        .Build())
                    .AddStatement("return response ?? throw new global::System.Text.Json.JsonException(\"Parsing contract response failed\")");
            }
        }
    }

    private IEnumerable<FunctionBuilder> GenerateQueryFunctions(JsonSchema queryMsgSchema,
        IReadOnlyDictionary<string, JsonSchema> responseSchemas)
    {
        foreach(var querySchema in queryMsgSchema.OneOf)
        {
            if(querySchema.Properties.Count != 1)
            {
                throw new NotSupportedException();
            }

            var argumentsSchema = querySchema.Properties.Single().Value;
            var queryName = argumentsSchema.Name;

            if(!responseSchemas.TryGetValue(queryName, out var responseSchema))
            {
                throw new NotSupportedException();
            }

            var responseType = GetOrGenerateMergingSchemaType(responseSchema);

            var functions = new List<FunctionBuilder>
            {
                new FunctionBuilder($"{NameUtils.ToValidFunctionName(queryName)}Async")
                .WithVisibility(FunctionVisibility.Public)
                .WithReturnTypeRaw($"Task<{responseType.Name}>")
                .WithIsAsync()
                .WithSummaryComment(querySchema.Description)
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
                    .ToVariableAssignment("jsonRequest"))

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

            foreach(var property in sortedProperties)
            {
                var argName = property.Key;
                var argSchema = property.Value;

                var paramTypes = GetOrGenerateSplittingSchemaType(argSchema, queryMsgSchema).ToArray();

                if(paramTypes.Length == 0)
                {
                    throw new NotSupportedException();
                }
                else if(paramTypes.Length == 1)
                {
                    var paramType = paramTypes[0];
                    foreach(var function in functions)
                    {
                        function
                            .AddArgument(paramType.Name, argName,
                                paramType.HasDefaultValue, paramType.ExplicitDefaultValue)
                            .AddStatement(new MethodCallBuilder("innerJsonRequest", "Add")
                                .AddArgument($"\"{argName}\"")
                                .AddArgument($"global::System.Text.Json.JsonSerializer.SerializeToNode((object?) {argName})")
                                .Build());
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
                        functions[i]
                            .AddArgument(paramTypes[paramIndex].Name, argName,
                                paramTypes[paramIndex].HasDefaultValue, paramTypes[paramIndex].ExplicitDefaultValue)
                            .AddStatement(new MethodCallBuilder("innerJsonRequest", "Add")
                                .AddArgument($"\"{argName}\"")
                                .AddArgument($"global::System.Text.Json.JsonSerializer.SerializeToNode({argName})")
                                .Build());
                    }
                }
            }

            foreach(var function in functions)
            {
                yield return function
                    .AddStatement("var encodedRequest = global::System.Text.Encoding.UTF8.GetBytes(jsonRequest.ToJsonString())")
                    .AddStatement("var encodedResponse = await _wasm.SmartContractStateAsync(_contractAddress, global::Google.Protobuf.ByteString.CopyFrom(encodedRequest))")
                    .AddStatement("var jsonResponse = global::System.Text.Encoding.UTF8.GetString(encodedResponse.Span)")
                    .AddStatement($"return global::System.Text.Json.JsonSerializer.Deserialize<{responseType.Name}>(jsonResponse) ?? throw new global::System.Text.Json.JsonException(\"Parsing contract response failed\")");
            }
        }
    }

    private IEnumerable<GeneratedType> GetOrGenerateSplittingSchemaType(JsonSchema schema,
        JsonSchema? definitionSource = null)
    {
        definitionSource ??= schema;

        if(schema.OneOf.Count != 0 && schema.OneOf.All(x => x.IsEnumeration))
        {
            yield return GetOrGenerateEnumerationType(schema, definitionSource);
            yield break;
        }
        if(schema.OneOf.Count != 0)
        {
            yield return GetOrGenerateAbstractSelectorClass(schema, definitionSource);
            yield break;
        }
        if(schema.AnyOf.Count == 2 && schema.AnyOf.Count(x => x.Type != JsonObjectType.Null) == 1)
        {
            foreach(var innerType in GetOrGenerateSplittingSchemaType(
                schema.AnyOf.Single(x => x.Type != JsonObjectType.Null), definitionSource))
            {
                yield return new GeneratedType($"{innerType.Name}?", true);
            }

            yield break;
        }
        if(schema.HasReference && schema.AllOf.Count == 0)
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
                        yield return new GeneratedType($"{innerType.Name}[]", false);
                    }
                    break;
                case JsonObjectType.Object:
                    if(schema.Properties.Count == 0)
                    {
                        yield return new GeneratedType("object", true, explicitDefaultValue: "new object()");
                        break;
                    }
                    foreach(var innerType in GetOrGenerateSplittingObjectType(schema, definitionSource))
                    {
                        yield return innerType;
                    }
                    break;
                case JsonObjectType.Boolean:
                    yield return new GeneratedType("bool", schema.Default is not null, explicitDefaultValue: schema.Default?.ToString()?.ToLower()!);
                    break;
                case JsonObjectType.Boolean | JsonObjectType.Null:
                    yield return new GeneratedType("bool?", true);
                    break;
                case JsonObjectType.Integer:
                    yield return new GeneratedType("int", schema.Default is not null, explicitDefaultValue: schema.Default?.ToString()!);
                    break;
                case JsonObjectType.Integer | JsonObjectType.Null:
                    yield return new GeneratedType("int?", true);
                    break;
                case JsonObjectType.Number:
                    yield return new GeneratedType("double", schema.Default is not null, explicitDefaultValue: schema.Default?.ToString()!);
                    break;
                case JsonObjectType.Number | JsonObjectType.Null:
                    yield return new GeneratedType("double?", true);
                    break;
                case JsonObjectType.String:
                    yield return new GeneratedType("string", schema.Default is not null, explicitDefaultValue: schema.Default?.ToString()!);
                    break;
                case JsonObjectType.String | JsonObjectType.Null:
                    yield return new GeneratedType("string?", true);
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

    private IEnumerable<GeneratedType> GetOrGenerateSplittingObjectType(JsonSchema objectSchema, JsonSchema definitionsSource)
    {
        if(objectSchema.Type != JsonObjectType.Object || objectSchema.ActualProperties.Count == 0)
        {
            throw new InvalidOperationException();
        }

        var definitionName = definitionsSource.Definitions
            .FirstOrDefault(x => x.Value == objectSchema).Key;

        string typeName = objectSchema.Title ?? definitionName
            ?? (objectSchema.RequiredProperties.Count == 1 && objectSchema.Properties.Count == 1 //Nested message
                ? $"{NameUtils.ToValidClassName(objectSchema.RequiredProperties.Single())}" +
                    (objectSchema.ParentSchema.Description.Contains("returns") ? "Query" : "Msg")
                : null)
            ?? (objectSchema is JsonSchemaProperty p ? NameUtils.ToValidClassName(p.Name) : null)
            ?? "Request";

        var classBuilder = new ClassBuilder(typeName);

        foreach(var property in objectSchema.ActualProperties)
        {
            var schemaTypes = GetOrGenerateSplittingSchemaType(property.Value, definitionsSource);

            var schemaType = schemaTypes.Count() == 1
                ? schemaTypes.Single()
                : throw new NotSupportedException();

            classBuilder.AddProperty(
                new PropertyBuilder(
                    schemaType.Name,
                    NameUtils.ToValidPropertyName(property.Key))
                .WithSetterVisibility(SetterVisibility.Init)
                .WithIsRequired(!schemaType.HasDefaultValue)
                .WithJsonPropertyName(property.Key)
                .WithSummaryComment(property.Value.Description).WithDefaultValue(schemaType.HasDefaultValue
                    ? schemaType.Name == "string" && double.TryParse(schemaType.ExplicitDefaultValue, out _)
                        ? $"\"{schemaType.ExplicitDefaultValue}\""
                        : schemaType.ExplicitDefaultValue
                    : null)
            );
        }

        TryAddSourceComponent(typeName, classBuilder);

        yield return objectSchema.Default is not null
            ? throw new NotSupportedException()
            : new GeneratedType(typeName, false, classBuilder.GetSyntaxId());
    }

    private GeneratedType GetOrGenerateAbstractSelectorClass(JsonSchema parentSchema,
        JsonSchema definitionsSource)
    {
        var definitionName = definitionsSource.Definitions
            .FirstOrDefault(x => x.Value == parentSchema).Key;

        string baseClassName = NameUtils.ToValidClassName(definitionName);
        var baseClassBuilder = new ClassBuilder(baseClassName)
            .WithIsAbstract(true);

        foreach(var oneOfSchema in parentSchema.OneOf)
        {
            var type = GetOrGenerateSplittingObjectType(oneOfSchema, definitionsSource).Single();

            if(_sourceComponents.Find(x => x.GetSyntaxId() == type.GeneratedSyntaxId) is not ClassBuilder implementationClassBuilder)
            {
                throw new NotSupportedException();
            }

            ForceRemoveSourceComponent(implementationClassBuilder);

            string implementationClassName = $"{type.Name}Internal";
            implementationClassBuilder
                .WithName(implementationClassName)
                .WithVisibility(ClassVisibility.Internal)
                .AddBaseType(baseClassName, false);

            implementationClassName = TryAddSourceComponent(implementationClassName, implementationClassBuilder);

            var innerCtorCall = new ConstructorCallBuilder(implementationClassName);
            var innerFunc = new FunctionBuilder(type.Name)
                    .WithReturnTypeRaw(baseClassName)
                    .WithIsStatic();

            foreach(var property in implementationClassBuilder.GetProperties())
            {
                string argName = NameUtils.ToValidVariableName(property.Name);

                if (property.Type == "object")
                {
                    innerCtorCall.AddInitializer(property.Name, "new object()");
                }
                else
                {
                    innerFunc.AddArgument(property.Type, argName);
                    innerCtorCall.AddInitializer(property.Name, argName);
                }
            }

            innerFunc.AddStatement($"return {innerCtorCall.ToInlineCall()}");

            baseClassBuilder
                .AddFunction(innerFunc);
        }

        return new GeneratedType(TryAddSourceComponent(baseClassName, baseClassBuilder),
            false, baseClassBuilder.GetSyntaxId());
    }

    private GeneratedType GetOrGenerateEnumerationType(JsonSchema parentSchema, JsonSchema definitionsSource)
    {
        if(parentSchema.Type != JsonObjectType.None || parentSchema.ActualProperties.Count != 0 || parentSchema.OneOf.Count == 0)
        {
            throw new InvalidOperationException();
        }

        var definitionName = definitionsSource.Definitions
            .FirstOrDefault(x => x.Value == parentSchema).Key;

        string typeName = parentSchema.Title ?? definitionName ?? throw new NotSupportedException();

        var enumerationBuilder = new EnumerationBuilder(typeName)
            .WithSummaryComment(parentSchema.Description)
            .WithJsonConverter($"global::Cosm.Net.Json.SnakeCaseJsonStringEnumConverter<{typeName}>");

        foreach(var oneOf in parentSchema.OneOf)
        {
            string enumerationValue = oneOf.Enumeration.Single().ToString()
                    ?? throw new NotSupportedException();

            enumerationBuilder.AddValue(
                NameUtils.ToValidPropertyName(enumerationValue),
                oneOf.Description);
        }

        return parentSchema.Default is not null
            ? throw new NotSupportedException()
            : new GeneratedType(TryAddSourceComponent(typeName, enumerationBuilder), false, enumerationBuilder.GetSyntaxId());
    }
    private GeneratedType GetOrGenerateMergingSchemaType(JsonSchema schema,
    JsonSchema? definitionSource = null)
    {
        definitionSource ??= schema;

        if(schema.OneOf.Count != 0 && schema.OneOf.All(x => x.IsEnumeration))
        {
            return GetOrGenerateEnumerationType(schema, definitionSource);
        }
        if(schema.OneOf.Count != 0)
        {
            return GetOrGenerateMergedType(schema, definitionSource);
        }
        if(schema.AnyOf.Count == 2 && schema.AnyOf.Count(x => x.Type != JsonObjectType.Null) == 1)
        {
            return new GeneratedType(
                $"{GetOrGenerateMergingSchemaType(schema.AnyOf.Single(x => x.Type != JsonObjectType.Null), definitionSource).Name}?",
                true
            );
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
                    if(schema.Default is not null)
                    {
                        throw new NotSupportedException();
                    }
                    return new GeneratedType($"{GetOrGenerateMergingSchemaType(schema.Item, definitionSource).Name}[]", false);
                case JsonObjectType.Object:
                    if(schema.Properties.Count == 0)
                    {
                        return new GeneratedType("object", true, explicitDefaultValue: "new object()");
                    }
                    return GetOrGenerateMergingObjectType(schema, definitionSource);
                case JsonObjectType.Boolean:
                    return new GeneratedType("bool", schema.Default is not null, explicitDefaultValue: schema.Default?.ToString()?.ToLower()!);
                case JsonObjectType.Boolean | JsonObjectType.Null:
                    return new GeneratedType("bool?", true);
                case JsonObjectType.Integer:
                    return new GeneratedType("int", schema.Default is not null, explicitDefaultValue: schema.Default?.ToString()!);
                case JsonObjectType.Integer | JsonObjectType.Null:
                    return new GeneratedType("int?", true);
                case JsonObjectType.Number:
                    return new GeneratedType("double", schema.Default is not null, explicitDefaultValue: schema.Default?.ToString()!);
                case JsonObjectType.Number | JsonObjectType.Null:
                    return new GeneratedType("double?", true);
                case JsonObjectType.String:
                    return new GeneratedType("string", schema.Default is not null, explicitDefaultValue: schema.Default?.ToString()!);
                case JsonObjectType.String | JsonObjectType.Null:
                    return new GeneratedType("string?", true);

                case JsonObjectType.File:
                case JsonObjectType.None:
                case JsonObjectType.Null:
                default:
                    throw new NotSupportedException();
            }
        }

        throw new NotSupportedException();
    }

    private GeneratedType GetOrGenerateMergingObjectType(JsonSchema objectSchema, JsonSchema definitionsSource)
    {
        if(objectSchema.Type != JsonObjectType.Object || objectSchema.ActualProperties.Count == 0)
        {
            throw new InvalidOperationException();
        }

        string typeName = objectSchema.Title
            ?? (objectSchema is JsonSchemaProperty p ? NameUtils.ToValidPropertyName(p.Name) : null)
            ?? definitionsSource.Definitions.FirstOrDefault(x => x.Value == objectSchema).Key
            ?? "Response";

        var classBuilder = new ClassBuilder(typeName);

        foreach(var property in objectSchema.ActualProperties)
        {
            var schemaType = GetOrGenerateMergingSchemaType(property.Value, definitionsSource);

            classBuilder.AddProperty(
                new PropertyBuilder(
                    schemaType.Name,
                    NameUtils.ToValidPropertyName(property.Key))
                .WithSetterVisibility(SetterVisibility.Init)
                .WithIsRequired(!schemaType.HasDefaultValue)
                .WithJsonPropertyName(property.Key)
                .WithSummaryComment(property.Value.Description)
                .WithDefaultValue(schemaType.HasDefaultValue 
                    ? schemaType.Name == "string" && double.TryParse(schemaType.ExplicitDefaultValue, out _)
                        ? $"\"{schemaType.ExplicitDefaultValue}\""
                        : schemaType.ExplicitDefaultValue
                    : null)
            );
        }

        return objectSchema.Default is not null
            ? throw new NotSupportedException()
            : new GeneratedType(TryAddSourceComponent(typeName, classBuilder), false, classBuilder.GetSyntaxId());
    }

    private GeneratedType GetOrGenerateMergedType(JsonSchema parentSchema, JsonSchema definitionsSource)
    {
        var typeName = definitionsSource.Definitions
            .FirstOrDefault(x => x.Value == parentSchema)
            .Key ?? "Response";

        var classBuilder = new ClassBuilder(typeName);

        foreach(var schema in parentSchema.OneOf)
        {
            if(schema.Properties.Count > 1)
            {
                throw new NotSupportedException();
            }

            var type = GetOrGenerateMergingSchemaType(
                schema.Properties.Count == 1 ? schema.Properties.Single().Value : schema,
                definitionsSource);

            if(IsPrimitiveType(type))
            {
                type = type.Name == "string"
                    ? new GeneratedType("global::Cosm.Net.Json.StringWrapper", false)
                    : throw new NotSupportedException();
            }

            string propertyName = schema.Properties.Count == 1
                ? schema.Properties.Single().Key
                : "Value";

            classBuilder.AddProperty(
                new PropertyBuilder(
                    $"{type.Name.TrimEnd('?')}?",
                    NameUtils.ToValidPropertyName(propertyName)
                )
                .WithJsonPropertyName(propertyName)
                .WithSummaryComment(schema.Description)
                .WithSetterVisibility(SetterVisibility.Init)
                .WithIsRequired(false)
            );
        }

        return parentSchema.Default is not null
            ? throw new NotSupportedException()
            : new GeneratedType(TryAddSourceComponent(typeName, classBuilder), false, classBuilder.GetSyntaxId());
    }

    private static bool IsPrimitiveType(GeneratedType type)
        => type.Name switch
        {
            "string" => true,
            "int" => true,
            "double" => true,
            _ => false
        };
}
