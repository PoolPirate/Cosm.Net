using Microsoft.CodeAnalysis;

namespace Cosm.Net.Generators.Common.SourceGeneratorKit;

    /// <summary>
    /// Provides syntax context receivier which delegates work to multiple receivers.
    /// </summary>
public class AggregateSyntaxContextReceiver : ISyntaxContextReceiver
{
    public ISyntaxContextReceiver[] Receivers { get; }

    public AggregateSyntaxContextReceiver(params ISyntaxContextReceiver[] receivers)
    {
        Receivers = receivers;
        }

    /// <inheritdoc/>
    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        foreach(var receiver in Receivers)
        {
            receiver.OnVisitSyntaxNode(context);
        }
    }
}
