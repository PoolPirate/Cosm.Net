using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
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
        string[] parts = ReplaceAll(name, '_', '.', ' ', '/', '\\', '+')
            .Split(['_'], StringSplitOptions.RemoveEmptyEntries);

        foreach(string? part in parts)
        {
            _ = finalSb.Append(Capitalize(part));
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
        foreach(char oldChar in oldChars)
        {
            name = name.Replace(oldChar, replacement);
        }

        return name;
    }

    public static string EscapeVariableName(string name)
        => name switch
        {
            "params" => "parameters",
            "namespace" => "@namespace",
            "switch" => "@switch",
            "int" => "@int",
            "string" => "@string",
            "operator" => "@operator",
            "delegate" => "@delegate",
            _ => name,
        };

    public static string FullyQualifiedTypeName(ITypeSymbol symbol)
    {
        var sb = new StringBuilder();

        _ = sb.Append($"global::{symbol.ContainingNamespace}");

        var parentNames = new List<string>();
        var parentType = symbol.ContainingType;
        while(parentType is not null)
        {
            parentNames.Add(parentType.Name);
            parentType = parentType.ContainingType;
        }

        parentNames.Reverse();
        foreach(string parentName in parentNames)
        {
            _ = sb.Append($".{parentName}");
        }

        _ = sb.Append($".{symbol.Name}");

        if(symbol is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
        {
            _ = sb.Append('<');

            foreach(var typeArg in namedType.TypeArguments)
            {
                _ = sb.Append(FullyQualifiedTypeName(typeArg));
            }

            _ = sb.Append('>');
        }

        return sb.ToString();
    }
}
