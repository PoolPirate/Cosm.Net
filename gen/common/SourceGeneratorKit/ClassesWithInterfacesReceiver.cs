using Microsoft.CodeAnalysis;

namespace Cosm.Net.Generators.Common.SourceGeneratorKit;
public class ClassesWithInterfacesReceiver : SyntaxReceiver
{
    private readonly string _implementedInterface;

    public ClassesWithInterfacesReceiver(string implementedInterface)
    {
        _implementedInterface = implementedInterface;
    }

    public override bool CollectClassSymbol { get; } = true;

    protected override bool ShouldCollectTypeSymbol(INamedTypeSymbol classSymbol)
        => classSymbol.IsImplements(_implementedInterface);
}