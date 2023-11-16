using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;

public enum InterfaceVisibility
{
    Public,
    Internal
}
public class InterfaceBuilder
{
    private readonly List<FunctionBuilder> _functions;
    private readonly string _name;
    private InterfaceVisibility _visibility = InterfaceVisibility.Public;

    public InterfaceBuilder(string name)
    {
        _functions = [];
        _name = name;
    }

    public InterfaceBuilder AddFunction(FunctionBuilder function)
    {
        _functions.Add(function);
        return this;
    }

    public InterfaceBuilder AddFunctions(IEnumerable<FunctionBuilder> functions)
    {
        _functions.AddRange(functions);
        return this;
    }

    public InterfaceBuilder WithVisibility(InterfaceVisibility visibility) 
    {
        _visibility = visibility;
        return this;
    }

    public string Build()
    {
        var functionsSb = new StringBuilder();

        foreach (var function in _functions)
        {
            functionsSb.AppendLine(function.BuildInterfaceDefinition());
        }

        return
            $$"""
            {{_visibility.ToString().ToLower()}} interface {{_name}} {
                {{functionsSb}}
            }
            """;
    }
}
