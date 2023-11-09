using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosm.Net.Generators;
public static class QueryModuleProcessor
{
    public static string GetQueryModuleGeneratedCode(INamedTypeSymbol moduleType)
    {
        var clientType = GetQueryClientType(moduleType);
        var queryMethods = GetQueryClientQueryMethods(clientType);

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

        foreach(var property in requestProps)
        {
            _ = callArgsBuilder.Append($"{property.Type} {NameUtils.Uncapitalize(property.Name)}, ");
            _ = requestCtorBuilder.AppendLine($"{property.Name} = {NameUtils.Uncapitalize(property.Name)}, ");
        }

        int i = 0;
        foreach(var parameter in queryParams)
        {
            i++;
            _ = callArgsBuilder.Append(
                $$"""
                {{parameter.Type}} {{parameter.Name}}{{(parameter.HasExplicitDefaultValue
                    ? $"= {parameter.ExplicitDefaultValue ?? "default"}"
                    : "")}}{{(i < queryParams.Length ? ", " : "")}}
                """);
            _ = parameterBuilder.Append($"{parameter.Name}{(i < queryParams.Length ? ", " : "")}");
        }

        return
            $$"""
            public {{queryMethod.ReturnType}} {{queryMethod.Name}}({{callArgsBuilder}}) {
                return Service.{{queryMethod.Name}}(new {{requestType}}() {
                {{requestCtorBuilder}}
                }, {{parameterBuilder}});
            }
            """;
    }

    private static ITypeSymbol GetQueryClientType(INamedTypeSymbol moduleType)
        => moduleType.AllInterfaces
            .Where(x => x.Name == "ICosmModule")
            .Select(x => x.TypeArguments[0])
            .First();

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
