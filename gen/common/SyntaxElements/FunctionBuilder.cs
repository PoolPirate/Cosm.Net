using Cosm.Net.Generators.Common.Util;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public enum FunctionVisibility
{
    Public,
    Private,
}

public class FunctionBuilder
{
    private readonly List<string> _statements;
    private TypedArgumentsBuilder _argumentBuilder;

    private readonly string _name;

    private FunctionVisibility _visibility = FunctionVisibility.Public;
    private string? _returnType = null;
    private string? _summaryComment = null;

    public FunctionBuilder(string name)
    {
         _argumentBuilder = new TypedArgumentsBuilder();
        _statements = [];
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

    public FunctionBuilder WithSummaryComment(string summaryComment)
    {
        _summaryComment = summaryComment;
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

    public FunctionBuilder AddArgument(string type, string argumentName,
    bool hasExplicityDefaultValue = false, object? defaultValue = null)
    {
        _argumentBuilder.AddArgument(type, argumentName, hasExplicityDefaultValue, defaultValue);
        return this;
    }

    public string BuildInterfaceDefinition()
    {
        var sb = new StringBuilder();

        if (_summaryComment is not null)
        {
            sb.AppendLine(CommentUtils.MakeSummaryComment(_summaryComment));
        }
        sb.Append(_visibility.ToString().ToLower());
        sb.Append(" ");
        sb.Append(_returnType is null
            ? "void"
            : _returnType);
        sb.Append(" ");
        sb.Append(_name);
        sb.Append('(');
        sb.Append(_argumentBuilder.Build());
        sb.AppendLine(");");

        return sb.ToString();
    }

    public string BuildMethodCode()
    {
        var sb = new StringBuilder();

        if(_summaryComment is not null)
        {
            sb.AppendLine(CommentUtils.MakeSummaryComment(_summaryComment));
        }
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

    public FunctionBuilder Clone()
    {
        var clone = new FunctionBuilder(_name)
        {
            _returnType = _returnType,
            _visibility = _visibility,
            _summaryComment = _summaryComment,
        };

        clone._statements.AddRange(_statements);
        clone._argumentBuilder = _argumentBuilder.Clone();

        return clone;
    }
}
