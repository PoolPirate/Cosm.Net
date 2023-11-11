using Cosm.Net.Generators.Util;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.SyntaxElements;
public enum FunctionVisibility
{
    Public,
    Private,
}

public class FunctionBuilder
{
    private readonly TypedArgumentsBuilder _argumentBuilder;
    private readonly List<string> _statements;

    private readonly string _name;

    private string? _returnType = null;
    private FunctionVisibility _visibility = FunctionVisibility.Public;

    public FunctionBuilder(string name)
    {
         _argumentBuilder = new TypedArgumentsBuilder();
        _statements = new List<string>();
        _name = name;
    }

    public FunctionBuilder WithReturnType(INamedTypeSymbol returnType)
    {
        _returnType = NameUtils.FullyQualifiedTypeName(returnType);
        return this;
    }

    public FunctionBuilder WithReturnTypeRaw(string returnType)
    {
        _returnType = returnType;
        return this;
    }

    public FunctionBuilder WithVisibility(FunctionVisibility visibility)
    {
        _visibility = visibility;
        return this;
    }

    public FunctionBuilder AddStatement(string statement)
    {
        _statements.Add(statement);
        return this;
    }

    public FunctionBuilder AddArgument(INamedTypeSymbol type, string argumentName,
        bool hasExplicityDefaultValue = false, object? defaultValue = null)
    {
        _argumentBuilder.AddArgument(type, argumentName, hasExplicityDefaultValue, defaultValue);
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();

        sb.Append(_visibility.ToString().ToLower());
        sb.Append(" ");
        sb.Append(_returnType is null 
            ? "void"
            : _returnType);
        sb.Append(" ");
        sb.Append(_name);
        sb.Append('(');
        sb.Append(_argumentBuilder.Build());
        sb.Append(')');
        sb.AppendLine("{");

        foreach(var statement in _statements)
        {
            sb.AppendLine($"{statement};");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}
