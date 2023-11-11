using Microsoft.CodeAnalysis;
using System.Linq;

namespace Cosm.Net.Generators.Extensions;
public static class ISymbolExtensions
{
    public static bool IsObsolete(this ISymbol symbol)
    => symbol.GetAttributes().Any(a => a.AttributeClass?.Name == "ObsoleteAttribute");
}
