using Cosm.Net.Generators.Common.Util;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;

public class TypedArgumentsBuilder : ISyntaxBuilder
{
    private readonly List<string> _arguments;

    public TypedArgumentsBuilder()
    {
        _arguments = [];
    }

    public TypedArgumentsBuilder AddArgument(INamedTypeSymbol type, string variableName,
        bool hasExplicityDefaultValue = false, object? defaultValue = null)
    {
        _arguments.Add(ArgumentToString(type, variableName, hasExplicityDefaultValue, defaultValue));
        return this;
    }

    public TypedArgumentsBuilder AddArgument(string type, string variableName,
    bool hasExplicityDefaultValue = false, object? defaultValue = null)
    {
        _arguments.Add(ArgumentToString(type, variableName, hasExplicityDefaultValue, defaultValue));
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();

        for(int i = 0; i < _arguments.Count; i++)
        {
            string argument = _arguments[i];

            _ = sb.Append(argument);

            if(i + 1 < _arguments.Count)
            {
                _ = sb.Append(", ");
            }
        }

        return sb.ToString();
    }

    private string ArgumentToString(INamedTypeSymbol type, string variableName, bool hasExplicityDefaultValue, object? defaultValue)
    {
        string defaultValueSuffix = $"{(hasExplicityDefaultValue ? $" = {defaultValue ?? "default"}" : "")}";

        switch(type.Name)
        {
            case "RepeatedField": //nameof(RepeatedField<>)
                var typeArg = type.TypeArguments[0];
                return $"global::System.Collections.Generic.IEnumerable<{NameUtils.FullyQualifiedTypeName(typeArg)}> {variableName}";
            default:
                return $"{NameUtils.FullyQualifiedTypeName(type)} {variableName}{defaultValueSuffix}";
        }
    }

    private string ArgumentToString(string type, string variableName, bool hasExplicityDefaultValue, object? defaultValue)
    {
        string defaultValueSuffix = $"{(hasExplicityDefaultValue ? $" = {defaultValue ?? "default"}" : "")}";
        return $"{type} {variableName}{defaultValueSuffix}";
    }

    public TypedArgumentsBuilder Clone()
    {
        var clone = new TypedArgumentsBuilder();

        clone._arguments.AddRange(_arguments);

        return clone;
    }

    public SyntaxId GetSyntaxId()
    {
        int innerHashCode = _arguments.Count;

        foreach(int val in _arguments.Select(x => HashCode.Combine(x)))
        {
            innerHashCode = unchecked((innerHashCode * 314159) + val);
        }

        return new SyntaxId(innerHashCode);
    }
}
