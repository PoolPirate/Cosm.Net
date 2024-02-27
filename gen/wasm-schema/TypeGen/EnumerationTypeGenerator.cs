using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.Common.Util;
using Cosm.Net.Generators.CosmWasm.Models;
using NJsonSchema;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public class EnumerationTypeGenerator
{
    private GeneratedTypeAggregator _typeAggregator = null!;
    private ObjectTypeGenerator _objectTypeGenerator = null!;

    public void Initialize(GeneratedTypeAggregator typeAggregator, ObjectTypeGenerator objectTypeGenerator)
    {
        _typeAggregator = typeAggregator;
        _objectTypeGenerator = objectTypeGenerator;
    }

    public GeneratedTypeHandle GenerateEnumerationType(JsonSchema schema, JsonSchema definitionsSource)
    {
        string typeName = GenerateTypeName(schema, definitionsSource);

        var enumerationBuilder = new EnumerationBuilder(typeName)
            .WithJsonConverter($"global::Cosm.Net.Json.SnakeCaseJsonStringEnumConverter<{typeName}>");

        if(schema.Description is not null)
        {
            enumerationBuilder.WithSummaryComment(schema.Description);
        }

        foreach(var enumerationSchema in schema.OneOf)
        {
            if(enumerationSchema.Enumeration.Count != 1)
            {
                throw new NotSupportedException("Enumeration must only contain one value per entry");
            }

            string enumerationValue = enumerationSchema.Enumeration.Single().ToString()
                ?? throw new NotSupportedException("Enumeration Values cannot be null");

            enumerationBuilder.AddValue(
                    NameUtils.ToValidPropertyName(enumerationValue),
                    enumerationSchema.Description);
        }

        return _typeAggregator.GenerateTypeHandle(enumerationBuilder);
    }

    private enum RustEnumType
    {
        String,
        PrimitiveObject,
        ComplexObject
    }

    public GeneratedTypeHandle GenerateAbstractSelectorType(JsonSchema schema, JsonSchema definitionsSource)
    {
        string typeName = GenerateTypeName(schema, definitionsSource);

        var baseClassBuilder = new ClassBuilder(typeName)
            .WithIsAbstract()
            .AddBaseType($"global::Cosm.Net.Json.IRustEnum<{typeName}>", true)
            .WithJsonConverterType($"global::Cosm.Net.Json.RustEnumConverter<{typeName}>");

        var baseWriteFunction = new FunctionBuilder($"global::Cosm.Net.Json.IRustEnum<{typeName}>.Write")
            .WithIsStatic()
            .WithVisibility(FunctionVisibility.Omit)
            .AddArgument("global::System.Text.Json.Utf8JsonWriter", "writer")
            .AddArgument(typeName, "value")
            .AddArgument("global::System.Text.Json.JsonSerializerOptions", "options");
        var baseReadFunction = new FunctionBuilder($"global::Cosm.Net.Json.IRustEnum<{typeName}>.ReadFromDocument")
            .WithIsStatic()
            .WithVisibility(FunctionVisibility.Omit)
            .AddArgument("global::System.Text.Json.JsonDocument", "document")
            .WithReturnTypeRaw(typeName);

        var writeCases = new List<string>();
        var readCases = new List<string>();

        foreach(var enumerationSchema in schema.OneOf)
        {
            var enumType = DetectRustEnumType(enumerationSchema);
            var enumValueName = enumType == RustEnumType.String
                ? enumerationSchema.Enumeration.Count != 1
                    ? throw new NotSupportedException("EnumerationType must only contain one value per entry")
                    : enumerationSchema.Enumeration.Single().ToString()
                : enumerationSchema.ActualProperties.Count != 1
                    ? throw new NotSupportedException("EnumerationType must have only one ActualProperty per entry")
                    : enumerationSchema.ActualProperties.Single().Key;
            string derivedTypeName = $"{NameUtils.ToValidClassName(enumValueName)}{typeName}";

            var derivedTypeBuilder = new ClassBuilder(derivedTypeName)
                .AddBaseType(typeName, false);

            if(enumerationSchema.Description is not null)
            {
                derivedTypeBuilder.WithSummaryComment(enumerationSchema.Description);
            }

            switch(enumType)
            {
                case RustEnumType.String:
                    writeCases.Add(
                        $"""
                        case {derivedTypeName}:
                            writer.WriteStringValue("{enumValueName}");
                        """);
                    readCases.Add(
                        $"""
                        "{enumValueName}" => new {derivedTypeName}()
                        """);
                    break;
                case RustEnumType.PrimitiveObject:
                    _objectTypeGenerator.GenerateObjectTypeContent(derivedTypeBuilder, enumerationSchema, definitionsSource);
                    writeCases.Add(
                        $"""
                        case {derivedTypeName}:
                            writer.WriteStartObject();
                            writer.WritePropertyName("{enumValueName}");
                            var converter{derivedTypeName} = (global::System.Text.Json.Serialization.JsonConverter<{derivedTypeName}>) global::Cosm.Net.Json.CosmWasmJsonUtils.SerializerOptions.GetConverter(typeof({derivedTypeName}));
                            converter{derivedTypeName}.Write(writer, ({derivedTypeName}) (object) value, options);
                            writer.WriteEndObject();
                        """);
                    readCases.Add(
                        $"""
                        "{enumValueName}" => global::System.Text.Json.JsonSerializer.Deserialize<{derivedTypeName}>(document.RootElement.ToString(), global::Cosm.Net.Json.CosmWasmJsonUtils.SerializerOptions)!
                        """);
                    break;
                case RustEnumType.ComplexObject:
                    _objectTypeGenerator.GenerateObjectTypeContent(derivedTypeBuilder, enumerationSchema.ActualProperties.Single().Value, definitionsSource);
                    writeCases.Add(
                        $"""
                        case {derivedTypeName}:
                            writer.WriteStartObject();
                            writer.WritePropertyName("{enumValueName}");
                            var converter{derivedTypeName} = (global::System.Text.Json.Serialization.JsonConverter<{derivedTypeName}>) global::Cosm.Net.Json.CosmWasmJsonUtils.SerializerOptions.GetConverter(typeof({derivedTypeName}));
                            converter{derivedTypeName}.Write(writer, ({derivedTypeName}) (object) value, options);
                            writer.WriteEndObject();
                        """);
                    readCases.Add(
                        $"""
                        "{enumValueName}" => global::System.Text.Json.JsonSerializer.Deserialize<{derivedTypeName}>(
                            document.RootElement.GetProperty("{enumValueName}").ToString(), global::Cosm.Net.Json.CosmWasmJsonUtils.SerializerOptions)!
                        """);
                    break;
                default:
                    break;
                    throw new NotSupportedException($"Unsupport RustEnumType {enumType}");
            }

            baseClassBuilder.AddInnerType(derivedTypeBuilder);
        }

        baseWriteFunction.AddStatement(
            $$"""
            switch(value) {
            {{string.Join("break;\n", writeCases)}} {{(writeCases.Count > 0 ? "break;\n" : "")}}
            default:
                throw new global::System.Text.Json.JsonException("Failed parsing rust enum type {{typeName}}");
            }
            """, false);
        baseReadFunction.AddStatement(
            $$"""
            return global::Cosm.Net.Json.IRustEnum<{{typeName}}>.GetEnumKey(document) switch {
            {{string.Join(",\n", readCases)}}{{(readCases.Count > 0 ? ",\n" : "")}}
            _ => throw new global::System.Text.Json.JsonException("Failed parsing rust enum type {{typeName}}")
            }
            """);

        baseClassBuilder.AddFunction(baseWriteFunction);
        baseClassBuilder.AddFunction(baseReadFunction);

        if(schema.Description is not null)
        {
            baseClassBuilder.WithSummaryComment(schema.Description);
        }

        return _typeAggregator.GenerateTypeHandle(baseClassBuilder);
    }

    private static RustEnumType DetectRustEnumType(JsonSchema schema)
    {
        if(schema.Type == JsonObjectType.String)
        {
            return RustEnumType.String;
        }
        if(schema.ActualProperties.Count != 1)
        {
            throw new NotSupportedException("Unsupported Rust Enum with more than 1 property");
        }
        //
        return schema.ActualProperties.Single().Value.Type == JsonObjectType.Object
            ? RustEnumType.ComplexObject
            : RustEnumType.PrimitiveObject;
    }

    private static string GenerateTypeName(JsonSchema schema, JsonSchema definitionsSource)
    {
        string definitionName = definitionsSource.Definitions
                    .FirstOrDefault(x => x.Value == schema).Key;

        string typeName = schema.Title
            ?? definitionName
            ?? throw new NotSupportedException("No suitable name for enumeration type found");

        return typeName;
    }
}
