using Cosm.Net.Generators.Common.Util;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public enum FunctionVisibility
{
    Omit,
    Public,
    Private,
}

public class FunctionBuilder : ISyntaxBuilder
{
    private readonly List<string> _statements;
    private TypedArgumentsBuilder _argumentBuilder;
    private readonly Dictionary<string, string> _argumentComments;

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
        _argumentComments = [];
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

    public FunctionBuilder AddStatement(string statement, bool requireSemicolon = true)
    {
        _statements.Add($"{statement}{(requireSemicolon ? ";" : "")}");
        return this;
    }

    public FunctionBuilder AddArgument(INamedTypeSymbol type, string argumentName,
        bool hasExplicityDefaultValue = false, object? defaultValue = null, string? xmlComment = null)
    {
        AddArgumentXmlComment(argumentName, xmlComment);
        _ = _argumentBuilder.AddArgument(type, argumentName, hasExplicityDefaultValue, defaultValue);
        return this;
    }

    public FunctionBuilder AddArgument(string type, string argumentName,
    bool hasExplicityDefaultValue = false, object? defaultValue = null, string? xmlComment = null)
    {
        AddArgumentXmlComment(argumentName, xmlComment);
        _ = _argumentBuilder.AddArgument(type, argumentName, hasExplicityDefaultValue, defaultValue);
        return this;
    }

    private void AddArgumentXmlComment(string argumentName, string? xmlComment)
    {
        if(xmlComment is not null)
        {
            if(_argumentComments.ContainsKey(argumentName))
            {
                throw new InvalidOperationException("xmlComment for that argument already set!");
            }
            _argumentComments.Add(argumentName, xmlComment);
        }
    }

    public string BuildInterfaceDefinition()
    {
        if (_visibility == FunctionVisibility.Omit)
        {
            return String.Empty;
        }

        var sb = new StringBuilder();

        if(_summaryComment is not null)
        {
            _ = sb.AppendLine(CommentUtils.MakeSummaryComment(_summaryComment));
        }

            sb.Append($"{_visibility.ToString().ToLower()} ");
        _ = sb.Append($"{(_returnType is null ? "void" : _returnType)} {_name}({_argumentBuilder.Build()});");

        return sb.ToString();
    }

    public string BuildMethodCode()
    {
        var sb = new StringBuilder();

        if(_summaryComment is not null)
        {
            _ = sb.AppendLine(CommentUtils.MakeSummaryComment(_summaryComment));
        }

        foreach(var argumentComment in _argumentComments)
        {
            sb.AppendLine(CommentUtils.MakeParamComment(argumentComment.Key, argumentComment.Value));
        }

        if (_visibility != FunctionVisibility.Omit)
        {
            _ = sb.Append($"{_visibility.ToString().ToLower()} ");
        }
        if(_isStatic)
        {
            _ = sb.Append("static ");
        }
        if(_isAsync)
        {
            _ = sb.Append("async ");
        }

        _ = sb.Append($"""
        {(_returnType is null ? "void" : _returnType)} {_name}({_argumentBuilder.Build()})
        """);
        _ = sb.AppendLine("{");

        foreach(string statement in _statements)
        {
            _ = sb.AppendLine($"{statement}");
        }

        _ = sb.AppendLine("}");

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
