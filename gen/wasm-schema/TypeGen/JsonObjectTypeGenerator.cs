using Cosm.Net.Generators.CosmWasm.Models;
using NJsonSchema;
using System.Reflection;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public class JsonObjectTypeGenerator
{
    private ObjectTypeGenerator _objectTypeGenerator = null!;
    private SchemaTypeGenerator _schemaTypeGenerator = null!;
    private TupleTypeGenerator _tupleTypeGenerator = null!;
    
    public void Initialize(ObjectTypeGenerator objectTypeGenerator, SchemaTypeGenerator schemaTypeGenerator, TupleTypeGenerator tupleTypeGenerator)
    {
        _objectTypeGenerator = objectTypeGenerator;
        _schemaTypeGenerator = schemaTypeGenerator;
        _tupleTypeGenerator = tupleTypeGenerator;
    }

    public GeneratedTypeHandle GenerateJsonObjectType(JsonSchema schema, JsonSchema definitionsSource)
        => schema.Type switch
        {
            JsonObjectType.Array => GenerateArrayType(schema, definitionsSource),
            JsonObjectType.Array | JsonObjectType.Null => GenerateArrayType(schema, definitionsSource).ToNullable(),
            JsonObjectType.Object => GenerateObjectType(schema, definitionsSource),
            JsonObjectType.Boolean => GenerateParsableType<bool>(schema),
            JsonObjectType.Boolean | JsonObjectType.Null => GenerateParsableType<bool>(schema).ToNullable(),
            JsonObjectType.Integer => GenerateIntegerType(schema),
            JsonObjectType.Integer | JsonObjectType.Null => GenerateIntegerType(schema),
            JsonObjectType.Number => GenerateParsableType<double>(schema),
            JsonObjectType.Number | JsonObjectType.Null => GenerateParsableType<double>(schema).ToNullable(),
            JsonObjectType.String => GenerateParsableType<string>(schema),
            JsonObjectType.String | JsonObjectType.Null => GenerateParsableType<string>(schema).ToNullable(),
            _ => throw new NotSupportedException($"Unsupported JsonObjectType {schema.Type}"),
        };

    private GeneratedTypeHandle GenerateObjectType(JsonSchema schema, JsonSchema definitionsSource)
        => schema.Properties.Count == 0
            ? new GeneratedTypeHandle(
                "object",
                "new object()"
            )
            : _objectTypeGenerator.GenerateObjectType(schema, definitionsSource);

    private GeneratedTypeHandle GenerateArrayType(JsonSchema schema, JsonSchema definitionsSource)
    {

        if(schema.Item is null && (schema.Items is null || schema.Items.Count == 0))
        {
            throw new NotSupportedException("Unsupported schema, JsonObjectType Array, no Item type(s) set");
        }
        if(schema.Default is not null)
        {
            throw new NotSupportedException("Array types don't support a default value");
        }
        if(schema.Item is not null)
        {
            return _schemaTypeGenerator.GetOrGenerateSchemaType(schema.Item, definitionsSource).ToArray();
        }

        var baseReference = schema.Items.FirstOrDefault(x => x.Reference is not null)?.Reference;

        if(baseReference is null || schema.Items.Any(x => !x.HasReference) || schema.Items.Any(x => x.Reference != baseReference))
        {
            return _tupleTypeGenerator.GenerateTupleType(schema, definitionsSource);
        }
        //
        return _schemaTypeGenerator
            .GetOrGenerateSchemaType(baseReference, definitionsSource)
            .ToArray();
    }

    private static GeneratedTypeHandle GenerateIntegerType(JsonSchema schema)
    {
        var type = schema.Format switch
        {
            "uint64" => GenerateParsableType<ulong>(schema),
            "int64" => GenerateParsableType<long>(schema),
            "uint32" => GenerateParsableType<uint>(schema),
            "int32" => GenerateParsableType<int>(schema),
            "uint16" => GenerateParsableType<ushort>(schema),
            "int16" => GenerateParsableType<short>(schema),
            "uint8" => GenerateParsableType<byte>(schema),
            _ => throw new NotSupportedException($"Unsupported Integer format {schema.Format}")
        };

        return schema.Type.HasFlag(JsonObjectType.Null)
            ? type.ToNullable()
            : type;
    }

    private static bool ReflectionTryParse<TType>(string? s, out TType result)
    {
        if(typeof(TType) == typeof(string))
        {
            if(s is null)
            {
                result = default!;
                return false;
            }

            result = (TType) (object) s;
            return true;
        }

        var tryParseMethod = typeof(TType).GetMethod("TryParse",
            BindingFlags.Static | BindingFlags.Public, null,
            [
                typeof(string),
                typeof(TType).MakeByRefType()
            ], null) ?? throw new Exception();

        TType output = default!;
        bool success = (bool) tryParseMethod.Invoke(null, [s, output])!;
        result = output;
        return success;
    }

    private static GeneratedTypeHandle GenerateParsableType<TType>(JsonSchema schema)
    {
        TType? defaultValue = default;
        if(schema.Default is not null && !ReflectionTryParse<TType>(schema.Default.ToString(), out defaultValue))
        {
            throw new NotSupportedException($"Unsupported {nameof(TType)} default value: {schema.Default}");
        }
        //

        string? defaultValueStr = defaultValue?.ToString()?.ToLower();

        if(defaultValueStr is not null &&
            typeof(TType) == typeof(string)
            && (!defaultValueStr.StartsWith("\"") || !defaultValueStr.EndsWith("\"")))
        {
            defaultValueStr = $"\"{defaultValueStr}\"";
        }

        return new GeneratedTypeHandle(
            typeof(TType).FullName ?? throw new InvalidOperationException("Failed to get Type FullName"),
            defaultValueStr);
    }
}
