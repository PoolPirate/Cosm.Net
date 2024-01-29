using Cosm.Net.Generators.Common.Util;
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
        _ = _argumentsBuilder.AddArgument(sourceExpression);
        return this;
    }

    public ConstructorCallBuilder AddInitializer(IPropertySymbol property, string sourceExpression)
    {
        _ = _objectInitializerBuilder.AddArgument(property, sourceExpression);
        return this;
    }

    public ConstructorCallBuilder AddInitializer(string propertyName, string sourceExpression, bool isReadonlyList = false)
    {
        _ = _objectInitializerBuilder.AddArgument(propertyName, sourceExpression, isReadonlyList);
        return this;
    }

    public string ToInlineCall()
    {
        var sb = new StringBuilder();

        _ = sb.Append("new ");
        _ = sb.Append(_constructedType);
        _ = sb.Append('(');
        _ = sb.Append(_argumentsBuilder.Build());
        _ = sb.Append(')');

        _ = sb.Append(_objectInitializerBuilder.ToInlineInitializer());

        return sb.ToString();
    }

    public string ToVariableAssignment(string variableName)
    {
        var sb = new StringBuilder();

        _ = sb.Append($"var {variableName} = new ");
        _ = sb.Append(_constructedType);
        _ = sb.Append('(');
        _ = sb.Append(_argumentsBuilder.Build());
        _ = sb.Append(')');

        _ = sb.Append(_objectInitializerBuilder.ToMultilineInitializer(variableName));

        return sb.ToString();
    }
}
