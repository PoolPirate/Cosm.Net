using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.Common.Util;
using Cosm.Net.Generators.CosmWasm.Models;
using NJsonSchema;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public class ObjectTypeGenerator
{
    private GeneratedTypeAggregator _typeAggregator = null!;
    private SchemaTypeGenerator _schemaTypeGenerator = null!;

    public void Initialize(GeneratedTypeAggregator typeAggregator, SchemaTypeGenerator schemaTypeGenerator)
    {
        _typeAggregator = typeAggregator;
        _schemaTypeGenerator = schemaTypeGenerator;
    }

    public GeneratedTypeHandle GenerateObjectType(JsonSchema schema, JsonSchema definitionsSource, string? nameOverride = null)
    {
        if(schema.ActualProperties.Count == 0)
        {
            throw new NotSupportedException("Generating object type without properties not supported");
        }
        if(schema.Default is not null)
        {
            throw new NotSupportedException("Default not supported for object types");
        }

        string typeName = GenerateTypeName(schema, definitionsSource, nameOverride);
        var classBuilder = new ClassBuilder(typeName);

        return _typeAggregator.GenerateTypeHandle(
            GenerateObjectTypeContent(classBuilder, schema, definitionsSource));
    }

    public ClassBuilder GenerateObjectTypeContent(ClassBuilder classBuilder, JsonSchema schema, JsonSchema definitionsSource)
    {
        foreach(var property in schema.ActualProperties)
        {
            var propertyType = _schemaTypeGenerator.GetOrGenerateSchemaType(property.Value, definitionsSource);
            var propertyBuilder = new PropertyBuilder(
                    propertyType.Name,
                    NameUtils.ToValidPropertyName(property.Key))
                .WithSetterVisibility(SetterVisibility.Init)
                .WithIsRequired(propertyType.DefaultValue is null)
                .WithJsonPropertyName(property.Key);

            if(property.Value.Description is not null)
            {
                propertyBuilder.WithSummaryComment(property.Value.Description);
            }
            if(propertyType.DefaultValue is not null)
            {
                propertyBuilder.WithDefaultValue(propertyType.DefaultValue);
            }

            classBuilder.AddProperty(propertyBuilder);
        }

        if(schema.Description is not null)
        {
            classBuilder.WithSummaryComment(schema.Description);
        }

        return classBuilder;
    }

    private static string GenerateTypeName(JsonSchema schema, JsonSchema definitionsSource, string? nameOverride = null)
    {
        string definitionName = definitionsSource.Definitions
            .FirstOrDefault(x => x.Value == schema).Key;

        string typeName = definitionName 
            ?? (schema.Title.IndexOf(' ') != -1
                ? null
                : schema.Title)
            ?? (schema.RequiredProperties.Count == 1 && schema.Properties.Count == 1 //Nested message
                ? $"{NameUtils.ToValidClassName(schema.RequiredProperties.Single())}" +
                    (schema.ParentSchema is not null && schema.ParentSchema.Description is not null && schema.ParentSchema.Description.Contains("returns")
                        ? "Query"
                        : "Msg")
                : null)
            ?? (schema is JsonSchemaProperty p ? NameUtils.ToValidClassName(p.Name) : null)
            ?? nameOverride
            ?? "Request";

        return NameUtils.ToValidClassName(typeName);
    }
}
