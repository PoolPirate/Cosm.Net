using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public class ConstructorBuilder
{
    private readonly List<FieldBuilder> _fields;
    private readonly string _className;

    public ConstructorBuilder(string className) 
    {
        _fields = [];
        _className = className;
    }

    public ConstructorBuilder AddInitializedField(FieldBuilder field)
    {
        _fields.Add(field);
        return this;
    }

    public ConstructorBuilder AddInitializedFields(IEnumerable<FieldBuilder> fields) { 
        foreach(var field in fields)
        {
            AddInitializedField(field);
        }

        return this;
    }

    public string Build()
    {
        var assignmentSb = new StringBuilder();
        var argumentBuilder = new TypedArgumentsBuilder();

        foreach(var field in _fields)
        { 
            if (field.Name.StartsWith("_"))
            {
                assignmentSb.AppendLine($"{field.Name} = {field.Name.Substring(1)};");
                argumentBuilder.AddArgument(field.Type, field.Name.Substring(1));
            } 
            else
            {
                assignmentSb.AppendLine($"{field.Name} = _{field.Name};");
                argumentBuilder.AddArgument(field.Type, $"_{field.Name}");
            }
        }

        return
            $$"""
            public {{_className}}({{argumentBuilder.Build()}}) {
            {{assignmentSb}}
            }
            """;
    }
}
