using Cosm.Net.Generators.CosmWasm.Models;
using NJsonSchema;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public class SchemaTypeGenerator
{
    private EnumerationTypeGenerator _enumerationGenerator = null!;
    private JsonObjectTypeGenerator _jsonObjectTypeGenerator = null!;

    public void Initialize(EnumerationTypeGenerator enumerationTypeGenerator, JsonObjectTypeGenerator jsonObjectTypeGenerator)
    {
        _enumerationGenerator =  enumerationTypeGenerator;
        _jsonObjectTypeGenerator = jsonObjectTypeGenerator;
    }

    public GeneratedTypeHandle GetOrGenerateSchemaType(JsonSchema schema, JsonSchema definitionSource)
    {
        definitionSource ??= schema;
        schema = GetInnerSchema(schema);

        if(IsBasicEnumerationType(schema))
        {
            return _enumerationGenerator.GenerateEnumerationType(schema, definitionSource);
        }
        if(IsDataEnumerationType(schema))
        {
            return _enumerationGenerator.GenerateAbstractSelectorType(schema, definitionSource);
        }
        if(IsNullableInnerTyper(schema))
        {
            var innerType = GetOrGenerateSchemaType(schema.AnyOf.Single(x => x.Type != JsonObjectType.Null), definitionSource);
            return innerType.ToNullable();
        }
        if(IsJsonObjectType(schema))
        {
            return _jsonObjectTypeGenerator.GenerateJsonObjectType(schema, definitionSource);
        }

        //
        throw new NotSupportedException("No known way to handle schema type");
    }

    /// <summary>
    /// Skip schema layers that don't affect generated code
    /// </summary>
    /// <returns></returns>
    public static JsonSchema GetInnerSchema(JsonSchema schema)
    {
        while(TryGetNextInnerSchema(schema, out schema))
        {
        }
        return schema;
    }

    private static bool TryGetNextInnerSchema(JsonSchema schema, out JsonSchema innerSchema)
    {
        if(schema.HasReference && schema.AllOf.Count == 0)
        {
            innerSchema = schema.Reference!;
            return true;
        }
        if(schema.AllOf.Count == 1)
        {
            innerSchema = schema.AllOf.Single();
            return true;
        }

        innerSchema = schema;
        return false;
    }

    /// <summary>
    /// If the schema represents a simple enum type. Simple enum types are made of only strings
    /// </summary>
    /// <returns></returns>
    private static bool IsBasicEnumerationType(JsonSchema schema)
        => schema.OneOf.Count > 0
        && schema.OneOf.All(x => x.IsEnumeration)
        && schema.Type == JsonObjectType.None
        && schema.ActualProperties.Count == 0;

    /// <summary>
    /// If the schema represents an enum type that also includes data. Check the rust enum feature for more clarity on what this means.
    /// </summary>
    /// <returns></returns>
    private static bool IsDataEnumerationType(JsonSchema schema)
        => schema.OneOf.Count > 0 && !schema.OneOf.All(x => x.IsEnumeration);

    /// <summary>
    /// A schema that has 2 AnyOf entries, one of which is null and one being an inner type
    /// </summary>
    /// <returns></returns>
    private static bool IsNullableInnerTyper(JsonSchema schema)
        => schema.AnyOf.Count == 2 && schema.AnyOf.Count(x => x.Type == JsonObjectType.Null) == 1;

    private static bool IsJsonObjectType(JsonSchema schema)
        => schema.AnyOf.Count == 0 && schema.AllOf.Count == 0 && !schema.HasReference;
}
