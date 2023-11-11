using Cosm.Net.Generators.Util;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Cosm.Net.Generators.SyntaxElements;
public class InitializerArgument
{
    public IPropertySymbol TargetProperty { get; }
    public string SourceExpression { get; }

    public InitializerArgument(IPropertySymbol targetProperty, string sourceExpression)
    {
        TargetProperty = targetProperty;
        SourceExpression = sourceExpression;
    }
}
public class ObjectInitializerBuilder
{
    private readonly List<InitializerArgument> _arguments;

    public ObjectInitializerBuilder()
    {
        _arguments = new List<InitializerArgument>();
    }

    public ObjectInitializerBuilder AddArgument(IPropertySymbol property, string sourceExpression)
    {
        _arguments.Add(new InitializerArgument(property, sourceExpression));
        return this;
    }

    public string ToInlineInitializer()
    {
        var sb = new StringBuilder();

        if (_arguments.Count == 0)
        {
            return string.Empty;
        }

        sb.AppendLine("{");

        foreach(var argument in _arguments)
        {
            if (argument.TargetProperty.IsReadOnly)
            {
                DebuggerUtils.Attach();
                throw new InvalidOperationException("Tried to use inline initializer for read-only property");
            }

            sb.AppendLine($"{argument.TargetProperty.Name} = {argument.SourceExpression},");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    public string ToMultilineInitializer(string variableName)
    {
        var sb = new StringBuilder();

        if(_arguments.Count == 0)
        {
            return string.Empty;
        }

        sb.AppendLine("{");

        foreach(var argument in _arguments)
        {
            if(argument.TargetProperty.IsReadOnly)
            {
                continue;
            }

            sb.AppendLine($"{argument.TargetProperty.Name} = {argument.SourceExpression},");
        }

        sb.AppendLine("};");

        foreach(var argument in _arguments)
        {
            if(!argument.TargetProperty.IsReadOnly)
            {
                continue;
            }

            sb.AppendLine(GetReadonlyInitializer(variableName, argument.TargetProperty, argument.SourceExpression));
        }

        return sb.ToString();
    }

    private string GetReadonlyInitializer(string variableName, IPropertySymbol targetProperty, string sourceExpression)
    {
        switch(targetProperty.Type.Name)
        {
            //nameof(RepeatedField<>)
            case "RepeatedField":
                return $"{variableName}.{targetProperty.Name}.AddRange({sourceExpression});";
            default:
                DebuggerUtils.Attach();
                throw new InvalidOperationException("Tried to initialize unsupported readonly property");
        }
    }
}
