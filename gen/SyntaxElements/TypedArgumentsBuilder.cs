using Cosm.Net.Generators.Util;
using Google.Protobuf.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.SyntaxElements;
public class TypedArgument
{
    public INamedTypeSymbol Type { get; }
    public string VariableName { get; }

    public bool HasExplicitDefaultValue { get; }
    public object? DefaultValue { get; }

    public TypedArgument(INamedTypeSymbol type, string argument, bool hasExplicitDefaultValue, object? defaultvalue)
    {
        Type = type;
        VariableName = argument;
        HasExplicitDefaultValue = hasExplicitDefaultValue;
        DefaultValue = defaultvalue;
    }
}
public class TypedArgumentsBuilder
{
    private readonly List<TypedArgument> _arguments;

    public TypedArgumentsBuilder()
    {
        _arguments = new List<TypedArgument>();
    }

    public TypedArgumentsBuilder AddArgument(INamedTypeSymbol type, string variableName, 
        bool hasExplicityDefaultValue = false, object? defaultValue = null)
    {
        _arguments.Add(new TypedArgument(type, variableName, hasExplicityDefaultValue, defaultValue));
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();

        for(int i = 0; i < _arguments.Count; i++)
        {
            var argument = _arguments[i];

            sb.Append(ArgumentToString(argument.Type, argument.VariableName, argument.HasExplicitDefaultValue, argument.DefaultValue));

            if (i + 1 < _arguments.Count)
            {
                sb.Append(", ");
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
}
