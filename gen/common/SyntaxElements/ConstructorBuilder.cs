using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public class ConstructorBuilder : ISyntaxBuilder
{
    private readonly List<FieldBuilder> _fields;
    private readonly List<PropertyBuilder> _properties;
    private readonly string _className;

    public ConstructorBuilder(string className)
    {
        _fields = [];
        _properties = [];
        _className = className;
    }

    public ConstructorBuilder AddInitializedField(FieldBuilder field)
    {
        _fields.Add(field);
        return this;
    }

    public ConstructorBuilder AddInitializedProperty(PropertyBuilder property)
    {
        _properties.Add(property);
        return this;
    }

    public ConstructorBuilder AddInitializedFields(IEnumerable<FieldBuilder> fields)
    {
        foreach(var field in fields)
        {
            _ = AddInitializedField(field);
        }

        return this;
    }

    public ConstructorBuilder AddInitializedProperties(IEnumerable<PropertyBuilder> properties)
    {
        foreach(var property in properties)
        {
            _ = AddInitializedProperty(property);
        }

        return this;
    }

    public string Build()
    {
        var assignmentSb = new StringBuilder();
        var argumentBuilder = new TypedArgumentsBuilder();

        foreach(var field in _fields)
        {
            if(field.Name.StartsWith("_"))
            {
                _ = assignmentSb.AppendLine($"{field.Name} = {field.Name.Substring(1)};");
                _ = argumentBuilder.AddArgument(field.Type, field.Name.Substring(1));
            }
            else
            {
                _ = assignmentSb.AppendLine($"{field.Name} = _{field.Name};");
                _ = argumentBuilder.AddArgument(field.Type, $"_{field.Name}");
            }
        }
        foreach(var property in _properties)
        {
            if(property.Name.StartsWith("_"))
            {
                _ = assignmentSb.AppendLine($"{property.Name} = {property.Name.Substring(1)};");
                _ = argumentBuilder.AddArgument(property.Type, property.Name.Substring(1));
            }
            else
            {
                _ = assignmentSb.AppendLine($"{property.Name} = _{property.Name};");
                _ = argumentBuilder.AddArgument(property.Type, $"_{property.Name}");
            }
        }

        return
            $$"""
            public {{_className}}({{argumentBuilder.Build()}}) {
            {{assignmentSb}}
            }
            """;
    }

    public SyntaxId GetSyntaxId()
    {
        var innerSyntaxId = new SyntaxId(_fields.Count);

        foreach(var syntaxId in _fields.Select(x => x.GetSyntaxId()))
        {
            innerSyntaxId = innerSyntaxId.Combine(syntaxId);
        }

        int hashCode = HashCode.Combine(
            innerSyntaxId,
            _className
        );

        return new SyntaxId(hashCode);
    }
}
