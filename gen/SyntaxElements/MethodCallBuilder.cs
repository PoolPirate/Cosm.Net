using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.SyntaxElements;

public class MethodCallBuilder
{
    private readonly string _target;
    private readonly IMethodSymbol _method;
    private readonly CallArgumentsBuilder _typedArgumentsBuilder;

    public MethodCallBuilder(string target, IMethodSymbol method)
    {
        _target = target;
        _method = method;
        _typedArgumentsBuilder = new CallArgumentsBuilder();
    }

    public MethodCallBuilder AddArgument(string sourceExpression)
    {
        _typedArgumentsBuilder.AddArgument(sourceExpression);
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();

        sb.Append(_target);
        sb.Append('.');
        sb.Append(_method.Name);
        sb.Append('(');
        sb.Append(_typedArgumentsBuilder.Build());
        sb.Append(')');

        return sb.ToString();
    }
}
