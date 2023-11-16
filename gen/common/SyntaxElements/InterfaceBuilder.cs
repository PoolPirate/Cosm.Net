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
    private readonly List<string> _baseInterfaces;
    private readonly string _name;
    private InterfaceVisibility _visibility = InterfaceVisibility.Public;
    private bool _isPartial = false;

    public InterfaceBuilder(string name)
    {
        _functions = [];
        _baseInterfaces = [];
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

    public InterfaceBuilder AddBaseInterface(string name)
    {
        _baseInterfaces.Add(name);
        return this;
    }

    public InterfaceBuilder AddBaseTypes(IEnumerable<BaseType> baseTypes)
    {
        foreach(var  baseType in baseTypes)
        {
            if (!baseType.IsInterface)
            {
                continue;
            }

            AddBaseInterface(baseType.Name);
        }

        return this;
    }

    public InterfaceBuilder WithVisibility(InterfaceVisibility visibility) 
    {
        _visibility = visibility;
        return this;
    }

    public InterfaceBuilder WithIsPartial(bool isPartial)
    {
        _isPartial = isPartial;
        return this;
    }

    public string Build()
    {
        var functionsSb = new StringBuilder();
        var baseInterfacesSb = new StringBuilder();

        foreach (var function in _functions)
        {
            functionsSb.AppendLine(function.BuildInterfaceDefinition());
        }
        for(int i = 0; i < _baseInterfaces.Count; i++)
        {
            if (i == 0)
            {
                baseInterfacesSb.Append(" : ");
            }

            baseInterfacesSb.Append(_baseInterfaces[i]);

            if (i < _baseInterfaces.Count - 1)
            {
                baseInterfacesSb.Append(", ");
            }
        }

        return
            $$"""
            {{_visibility.ToString().ToLower()}}{{(_isPartial ? " partial" : "")}} interface {{_name}} {{baseInterfacesSb}} 
            {
                {{functionsSb}}
            }
            """;
    }
}
