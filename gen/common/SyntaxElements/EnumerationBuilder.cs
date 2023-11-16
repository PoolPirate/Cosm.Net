using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public class EnumerationBuilder : ISyntaxBuilder
{
    private readonly List<string> _enumerationValues;
    private readonly string _name;

    public EnumerationBuilder(string name)
    {
        _enumerationValues = [];
        _name = name;
    }

    public EnumerationBuilder AddValue(string value)
    {
        _enumerationValues.Add(value);
        return this;
    }

    public string Build()
    {
        var valueSb = new StringBuilder();

        foreach(var value in _enumerationValues)
        {
            valueSb.AppendLine($"{value},");
        }

        return
            $$"""
            public enum {{_name}} {
            {{valueSb}}
            }
            """;
    }
}
