using Microsoft.CodeAnalysis;
using System.Linq;

namespace Cosm.Net.Generators.Common.Extensions;
public static class SymbolExtensions
{
    public static bool IsDerivedFromType(this INamedTypeSymbol symbol, string typeName)
        => symbol.Name == typeName || symbol.BaseType != null && symbol.BaseType.IsDerivedFromType(typeName);
}