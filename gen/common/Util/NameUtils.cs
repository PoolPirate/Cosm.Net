using Microsoft.CodeAnalysis;
using System;
using System.Text;

namespace Cosm.Net.Generators.Common.Util;
public static class NameUtils
{
    public static string ToValidVariableName(string name)
        => EscapeVariableName(Uncapitalize(name));

    public static string ToValidPropertyName(string name)
        => EscapeVariableName(ToValidNamespaceName(Capitalize(name)));

    public static string ToValidNamespaceName(string name)
    {
        var finalSb = new StringBuilder();
        var parts = ReplaceAll(name, '_', '.', ' ', '/', '\\', '+')
            .Split(['_'], StringSplitOptions.RemoveEmptyEntries);

        foreach(var part in parts)
        {
            finalSb.Append(Capitalize(part));
        }

        return finalSb.ToString();
    }

    public static string ToValidClassName(string name)
        => ToValidNamespaceName(name);

    public static string ToValidFunctionName(string name)
        => ToValidNamespaceName(name);

    public static string Uncapitalize(string name)
        => name[0].ToString().ToLower() + name.Substring(1);

    public static string Capitalize(string name)
        => name[0].ToString().ToUpper() + name.Substring(1);

    public static string ReplaceAll(string name, char replacement, params char[] oldChars)
    {
        foreach(var oldChar in oldChars)
        {
            name = name.Replace(oldChar, replacement);
        }

        return name;
    }

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
