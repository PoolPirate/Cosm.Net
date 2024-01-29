using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public class UntypedArgument
{
    public string SourceExpression { get; }

    public UntypedArgument(string sourceExpression)
    {
        SourceExpression = sourceExpression;
    }
}
public class CallArgumentsBuilder : ISyntaxBuilder
{
    private readonly List<UntypedArgument> _arguments;

    public CallArgumentsBuilder()
    {
        _arguments = [];
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
            _ = sb.Append(argument.SourceExpression);

            if(i + 1 < _arguments.Count)
            {
                _ = sb.Append(", ");
            }
        }

        return sb.ToString();
    }

    public SyntaxId GetSyntaxId()
    {
        int hashCode = HashCode.Combine(
            nameof(CallArgumentsBuilder),
            _arguments.Count
        );
        return new SyntaxId(hashCode);
    }
}
