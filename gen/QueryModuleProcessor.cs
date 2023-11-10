using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Cosm.Net.Generators;
public static class QueryModuleProcessor
{
    public static ITypeSymbol GetQueryClientType(INamedTypeSymbol moduleType)
        => moduleType.AllInterfaces
            .Where(x => x.Name == "ICosmModule")
            .Select(x => x.TypeArguments[0])
            .First();

    public static string GetQueryModuleGeneratedCode(INamedTypeSymbol moduleType, ITypeSymbol queryClientType)
    {
        var queryMethods = GetQueryClientQueryMethods(queryClientType);

        var methodCodeBuilder = new StringBuilder();

        foreach(var method in queryMethods)
        {
            string code = GetQueryMethodGeneratedCode(method);
            _ = methodCodeBuilder.AppendLine(code);
        }

        return
            $$"""
            namespace {{moduleType.ContainingNamespace}};

            public partial class {{moduleType.Name}} {
            {{methodCodeBuilder}}
            }
            """;
    }

    private static string GetQueryMethodGeneratedCode(IMethodSymbol queryMethod)
    {
        var requestType = GetQueryMethodRequestType(queryMethod);
        var requestProps = GetTypeInstanceProperties(requestType);
        var queryParams = queryMethod.Parameters.Skip(1).ToArray();

        var callArgsBuilder = new StringBuilder();
        var requestCtorBuilder = new StringBuilder();
        var parameterBuilder = new StringBuilder();
        var commentBuilder = new StringBuilder();

        foreach(var property in requestProps)
        {
            string paramName = NameUtils.Uncapitalize(property.Name);

            if(CommentUtils.TryGetSummary(property, out string? summary))
            {
                _ = commentBuilder.AppendLine(CommentUtils.MakeParamComment(paramName, summary!));
            }

            _ = callArgsBuilder.Append($"global::{property.Type.ContainingNamespace}.{property.Type.Name} {paramName}, ");
            _ = requestCtorBuilder.AppendLine($"{property.Name} = {paramName}, ");
        }

        int i = 0;
        foreach(var parameter in queryParams)
        {
            i++;
            _ = callArgsBuilder.Append(
                $$"""
                global::{{parameter.Type}} {{parameter.Name}}{{(parameter.HasExplicitDefaultValue
                    ? $"= {parameter.ExplicitDefaultValue ?? "default"}"
                    : "")}}{{(i < queryParams.Length ? ", " : "")}}
                """);
            _ = parameterBuilder.Append($"{parameter.Name}{(i < queryParams.Length ? ", " : "")}");

            if(CommentUtils.TryGetSummary(parameter, out string? summary))
            {
                _ = commentBuilder.AppendLine(CommentUtils.MakeParamComment(parameter.Name, summary!));
            }
        }

        if (queryMethod.ReturnType is not INamedTypeSymbol rt)
        {
            throw new InvalidOperationException("Bad cast");
        }

        var innerType = rt.TypeArguments[0];

        return
            $$"""
            {{(CommentUtils.TryGetSummary(queryMethod, out string? methodSummary) ? CommentUtils.MakeSummaryComment(methodSummary!) : "")}}
            {{commentBuilder}}
            public global::{{rt.ContainingNamespace}}.{{rt.Name}}<global::{{innerType.ContainingNamespace}}.{{innerType.Name}}> {{queryMethod.Name}}({{callArgsBuilder}}) {
                return Service.{{queryMethod.Name}}(new global::{{requestType}}() {
                {{requestCtorBuilder}}
                }, {{parameterBuilder}});
            }

            """;
    }

    private static IEnumerable<IMethodSymbol> GetQueryClientQueryMethods(ITypeSymbol queryClientType)
        => queryClientType.GetMembers()
            .Where(x => x.Name.EndsWith("Async"))
            .Where(x => x is IMethodSymbol)
            .Cast<IMethodSymbol>();

    private static ITypeSymbol GetQueryMethodRequestType(IMethodSymbol methodType)
        => methodType.Parameters[0].Type;

    private static IEnumerable<IPropertySymbol> GetTypeInstanceProperties(ITypeSymbol type)
        => type.GetMembers()
            .Where(x => x is IPropertySymbol)
            .Cast<IPropertySymbol>()
            .Where(x => !x.IsStatic && !x.IsReadOnly)
            .Where(x => !x.GetAttributes().Any(a => a.AttributeClass?.Name == "ObsoleteAttribute"));
}
