using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;

public class MethodCallBuilder
{
    private readonly CallArgumentsBuilder _typedArgumentsBuilder;

    private readonly string _target;
    private readonly string _methodName;

    public MethodCallBuilder(string target, IMethodSymbol method)
    {
        _typedArgumentsBuilder = new CallArgumentsBuilder();
        _target = target;
        _methodName = method.Name;
    }

    public MethodCallBuilder(string target, string methodName)
    {
        _typedArgumentsBuilder = new CallArgumentsBuilder();
        _target = target;
        _methodName = methodName;
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
        sb.Append(_methodName);
        sb.Append('(');
        sb.Append(_typedArgumentsBuilder.Build());
        sb.Append(')');

        return sb.ToString();
    }
}
