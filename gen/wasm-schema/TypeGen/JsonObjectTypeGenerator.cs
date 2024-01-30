using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.Common.Util;
using Cosm.Net.Generators.CosmWasm.Models;
using NJsonSchema;
using System.Reflection;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public static class JsonObjectTypeGenerator
{
    public static GeneratedTypeHandle GenerateJsonObjectType(JsonSchema schema, JsonSchema definitionsSource) 
        => schema.Type switch
        {
            JsonObjectType.Array => GenerateArrayType(schema, definitionsSource),
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

    private static GeneratedTypeHandle GenerateObjectType(JsonSchema schema, JsonSchema definitionsSource) 
        => schema.Properties.Count == 0
            ? new GeneratedTypeHandle(
                "object",
                "new object()"
            )
            : ObjectTypeGenerator.GenerateObjectType(schema, definitionsSource);

    private static GeneratedTypeHandle GenerateArrayType(JsonSchema schema, JsonSchema definitionsSource)
    {

        if(schema.Item is null && (schema.Items is null || schema.Items.Count == 0))
        {
            throw new NotSupportedException("Unsupported schema, JsonObjectType Array, no Item type(s) set");
        }
        if(schema.Item is not null)
        {
            return SchemaTypeGenerator.GetOrGenerateSchemaType(schema.Item, definitionsSource).ToArray();
        }
        if(schema.Items.Any(x => !x.HasReference))
        {
            throw new NotSupportedException("Unsupported schema, JsonObjectType Array, multiple item types, at least one without Reference");
        }

        var baseReference = schema.Items.First().Reference!;

        if(schema.Items.Any(x => x.Reference != baseReference))
        {
            throw new NotSupportedException("Unsupported schema, JsonObjectType Array, multiple entries of different types");
        }
        //
        return SchemaTypeGenerator
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

        if (defaultValueStr is not null && 
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
