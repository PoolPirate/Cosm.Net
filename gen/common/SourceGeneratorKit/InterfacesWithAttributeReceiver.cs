using Cosm.Net.Generators.Common.Util;
using Microsoft.CodeAnalysis;

namespace Cosm.Net.Generators.Common.SourceGeneratorKit;
public class InterfacesWithAttributeReceiver : SyntaxReceiver
{
    private readonly string _attributeName;

    public InterfacesWithAttributeReceiver(string attributeName)
    {
        _attributeName = attributeName;
    }

    public override bool CollectInterfaceSymbol { get; } = true;

    protected override bool ShouldCollectTypeSymbol(INamedTypeSymbol classSymbol)
        => classSymbol.HasAttribute(_attributeName);
}
