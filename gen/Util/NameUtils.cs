using Microsoft.CodeAnalysis;
using System.Text;

namespace Cosm.Net.Generators.Util;
public static class NameUtils
{
    public static string ToValidVariableName(string name)
        => EscapeVariableName(Uncapitalize(name));

    public static string Uncapitalize(string name)
        => name[0].ToString().ToLower() + name.Substring(1);

    public static string EscapeVariableName(string name) 
        => name.ToLower() switch
        {
            "params" => "parameters",
            "namespace" => "@namespace",
            "switch" => "@switch",
            "int" => "@int",
            "string" => "@string",
            _ => name,
        };

    public static string FullyQualifiedTypeName(ITypeSymbol symbol)
    {
        var sb = new StringBuilder();

        sb.Append($"global::{symbol.ContainingNamespace}.{symbol.Name}");

        if (symbol is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
        {
            sb.Append('<');

            foreach(var typeArg in namedType.TypeArguments)
            {
                sb.Append(FullyQualifiedTypeName(typeArg));
            }

            sb.Append('>');
        }

        return sb.ToString();
    }
}
