using Microsoft.CodeAnalysis;

namespace Cosm.Net.Generators.Common.SourceGeneratorKit;
public class ClassesWithInterfacesReceiver : SyntaxReceiver
{
    private readonly string implementedInterface;

    public ClassesWithInterfacesReceiver(string implementedInterface)
    {
        this.implementedInterface = implementedInterface;
    }

    public override bool CollectClassSymbol { get; } = true;

    protected override bool ShouldCollectClassSymbol(INamedTypeSymbol classSymbol)
        => classSymbol.IsImplements(implementedInterface);
}