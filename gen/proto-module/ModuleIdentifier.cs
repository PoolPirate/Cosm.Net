using Microsoft.CodeAnalysis;

namespace Cosm.Net.Generators.Proto;
public class ModuleIdentifier(INamedTypeSymbol[] clientTypes, string prefix, string name, string version, IMethodSymbol[][] queryMethods)
{
    public INamedTypeSymbol[] ClientTypes { get; } = clientTypes;
    public IMethodSymbol[][] QueryMethods { get; } = queryMethods;
    public string Prefix { get; } = prefix;
    public string Name { get; } = name;
    public string Version { get; } = version;
}
