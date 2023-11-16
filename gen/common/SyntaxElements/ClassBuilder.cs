using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;

public enum ClassVisibility
{
    Public,
    Internal
}
public class ClassBuilder : ISyntaxBuilder
{
    private readonly List<FunctionBuilder> _functions;
    private readonly List<PropertyBuilder> _properties;
    private readonly string _name;
    private ClassVisibility _visibility = ClassVisibility.Public;

    public ClassBuilder(string name)
    {
        _functions = [];
        _properties = [];
        _name = name;
    }

    public ClassBuilder AddFunction(FunctionBuilder function)
    {
        _functions.Add(function);
        return this;
    }

    public ClassBuilder AddProperty(PropertyBuilder property)
    {
        _properties.Add(property);
        return this;
    }

    public ClassBuilder AddFunctions(IEnumerable<FunctionBuilder> functions)
    {
        _functions.AddRange(functions);
        return this;
    }

    public ClassBuilder WithVisibility(ClassVisibility visibility)
    {
        _visibility = visibility;
        return this;
    }

    public string Build(bool generateInterface = false)
    {
        var functionsSb = new StringBuilder();

        foreach(var function in _functions)
        {
            functionsSb.AppendLine(function.BuildMethodCode());
        }

        foreach(var property in _properties)
        {
            functionsSb.AppendLine(property.Build());
        }   

        return 
            $$"""
            {{(generateInterface 
                ? new InterfaceBuilder($"I{_name}")
                    .AddFunctions(_functions)
                    .Build()
                : ""
            )}}
            {{_visibility.ToString().ToLower()}} class {{_name}} 
                {{(generateInterface ? $": I{_name}" : "")}}
            {
            {{functionsSb}}
            }
            """;
    }

    public string Build() 
        => Build(false);
}
