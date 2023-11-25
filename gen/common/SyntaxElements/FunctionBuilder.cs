using Cosm.Net.Generators.Common.Util;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public enum FunctionVisibility
{
    Public,
    Private,
}

public class FunctionBuilder : ISyntaxBuilder
{
    private readonly List<string> _statements;
    private TypedArgumentsBuilder _argumentBuilder;

    private readonly string _name;

    private FunctionVisibility _visibility = FunctionVisibility.Public;
    private string? _returnType = null;
    private string? _summaryComment = null;
    private bool _isAsync = false;
    private bool _isStatic = false;

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

    public FunctionBuilder WithIsAsync(bool isAsync = true)
    {
        _isAsync = isAsync;
        return this;
    }

    public FunctionBuilder WithIsStatic(bool isStatic = true)
    {
        _isStatic = isStatic; 
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

        sb.Append($"""
        {_visibility.ToString().ToLower()} {(_returnType is null ? "void" : _returnType)} {_name}({_argumentBuilder.Build()});
        """);

        return sb.ToString();
    }

    public string BuildMethodCode()
    {
        var sb = new StringBuilder();

        if(_summaryComment is not null)
        {
            sb.AppendLine(CommentUtils.MakeSummaryComment(_summaryComment));
        }
        sb.Append($"{_visibility.ToString().ToLower()} ");

        if (_isStatic)
        {
            sb.Append("static ");
        }
        if (_isAsync)
        {
            sb.Append("async ");
        }

        sb.Append($"""
        {(_returnType is null ? "void" : _returnType)} {_name}({_argumentBuilder.Build()})
        """);
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
            _isAsync = _isAsync,
            _isStatic = _isStatic,
        };

        clone._statements.AddRange(_statements);
        clone._argumentBuilder = _argumentBuilder.Clone();

        return clone;
    }

    public SyntaxId GetSyntaxId()
    {
        int innerHashCode = _statements.Count;

        foreach(int val in _statements.Select(x => HashCode.Combine(x)))
        {
            innerHashCode = unchecked((innerHashCode * 314159) + val);
        }

        return new SyntaxId(HashCode.Combine(
            nameof(FunctionBuilder),
            _name,
            _visibility,
            _returnType,
            _isAsync,
            _isStatic,
            innerHashCode
        )).Combine(_argumentBuilder.GetSyntaxId());
    }
}
