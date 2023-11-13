﻿using Cosm.Net.Generators.Util;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.SyntaxElements;
public class UntypedArgument
{
    public string SourceExpression { get; }

    public UntypedArgument(string sourceExpression)
    {
        SourceExpression = sourceExpression;
    }
}
public class CallArgumentsBuilder
{
    private readonly List<UntypedArgument> _arguments;

    public CallArgumentsBuilder()
    {
        _arguments = new List<UntypedArgument>();
    }

    public CallArgumentsBuilder AddArgument(string sourceExpression)
    {
        _arguments.Add(new UntypedArgument(sourceExpression));
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();

        for(int i = 0; i < _arguments.Count; i++)
        {
            var argument = _arguments[i];
            sb.Append(argument.SourceExpression);

            if(i + 1 < _arguments.Count)
            {
                sb.Append(", ");
            }            
        }

        return sb.ToString();
    }
}