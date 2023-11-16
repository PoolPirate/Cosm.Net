using Microsoft.CodeAnalysis;
using System.Linq;

namespace Cosm.Net.Generators.Common.SourceGeneratorKit;
public static class SymbolExtensions
{
    public static bool HasAttribute(this ISymbol symbol, string atrributeName)
        => symbol.GetAttributes()
            .Any(_ => _.AttributeClass?.ToDisplayString() == atrributeName);

    public static AttributeData? FindAttribute(this ISymbol symbol, string atrributeName)
        => symbol.GetAttributes()
            .FirstOrDefault(_ => _.AttributeClass?.ToDisplayString() == atrributeName);

    public static bool IsDerivedFromType(this INamedTypeSymbol symbol, string typeName)
        => symbol.Name == typeName || (symbol.BaseType != null && symbol.BaseType.IsDerivedFromType(typeName));

    public static bool IsImplements(this INamedTypeSymbol symbol, string typeName)
        => symbol.AllInterfaces.Any(x =>
        {
            string name = x.Name;
            return name == typeName;
        });
}