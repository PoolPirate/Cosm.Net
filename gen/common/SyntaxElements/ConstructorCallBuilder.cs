﻿using Cosm.Net.Generators.Common.Util;
using Microsoft.CodeAnalysis;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public class ConstructorCallBuilder
{
    private readonly CallArgumentsBuilder _argumentsBuilder;
    private readonly ObjectInitializerBuilder _objectInitializerBuilder;

    private readonly string _constructedType;

    public ConstructorCallBuilder(string constructedType)
    {
        _argumentsBuilder = new CallArgumentsBuilder();
        _objectInitializerBuilder = new ObjectInitializerBuilder();
        _constructedType = constructedType;
    }
    public ConstructorCallBuilder(INamedTypeSymbol constructedType)
        : this(NameUtils.FullyQualifiedTypeName(constructedType))
    {
    }

    public ConstructorCallBuilder AddArgument(string sourceExpression)
    {
        _argumentsBuilder.AddArgument(sourceExpression);
        return this;
    }

    public ConstructorCallBuilder AddInitializer(IPropertySymbol property, string sourceExpression)
    {
        _objectInitializerBuilder.AddArgument(property, sourceExpression);
        return this;
    }

    public ConstructorCallBuilder AddInitializer(string propertyName, string sourceExpression, bool isReadonlyList = false)
    {
        _objectInitializerBuilder.AddArgument(propertyName, sourceExpression, isReadonlyList);
        return this;
    }

    public string ToInlineCall()
    {
        var sb = new StringBuilder();

        sb.Append("new ");
        sb.Append(_constructedType);
        sb.Append('(');
        sb.Append(_argumentsBuilder.Build());
        sb.Append(')');

        sb.Append(_objectInitializerBuilder.ToInlineInitializer());

        return sb.ToString();
    }

    public string ToVariableAssignment(string variableName)
    {
        var sb = new StringBuilder();

        sb.Append($"var {variableName} = new ");
        sb.Append(_constructedType);
        sb.Append('(');
        sb.Append(_argumentsBuilder.Build());
        sb.Append(')');

        sb.Append(_objectInitializerBuilder.ToMultilineInitializer(variableName));

        return sb.ToString();
    }
}
