using Cosm.Net.Core.Msg;
using Cosm.Net.Generators.Extensions;
using Cosm.Net.Generators.SyntaxElements;
using Cosm.Net.Generators.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Cosm.Net.Generators;
public static class QueryModuleProcessor
{
    public static ITypeSymbol GetQueryClientType(INamedTypeSymbol moduleType)
        => moduleType.AllInterfaces
            .Where(x => x.Name == "ICosmModule")
            .Select(x => x.TypeArguments[0])
            .First();

    public static string GetQueryModuleGeneratedCode(INamedTypeSymbol moduleType, ITypeSymbol queryClientType, 
        IEnumerable<INamedTypeSymbol> messageTypes)
    {
        var queryMethods = GetQueryClientQueryMethods(queryClientType);

        var methodCodeBuilder = new StringBuilder();
        var interfaceCodeBuilder = new StringBuilder();

        foreach(var method in queryMethods)
        {
            (string interfaceMethod, string methodCode) = GetQueryMethodGeneratedCode(method);

            _ = methodCodeBuilder.AppendLine(methodCode);
            _ = interfaceCodeBuilder.AppendLine(interfaceMethod);
        }

        foreach(var messageType in  messageTypes)
        {
            string code = GetMessageTypeGeneratedCode(messageType);
            _ = methodCodeBuilder.AppendLine(code);
        }

        return
            $$"""
            namespace {{moduleType.ContainingNamespace}};

            public interface I{{moduleType.Name}} {
            {{interfaceCodeBuilder}}
            }

            internal partial class {{moduleType.Name}} : I{{moduleType.Name}} {
            {{methodCodeBuilder}}
            }

            """;
    }

    private static (string interfaceMethod, string methodCode) GetQueryMethodGeneratedCode(IMethodSymbol queryMethod)
    {
        var requestType = GetQueryMethodRequestType(queryMethod);
        var requestProps = GetTypeInstanceProperties(requestType);
        var queryParams = queryMethod.Parameters.Skip(1).ToArray();

        var functionBuilder = new FunctionBuilder(queryMethod.Name)
            .WithReturnType((INamedTypeSymbol) queryMethod.ReturnType);

        var queryFunctionCall = new MethodCallBuilder("Service", queryMethod);
        var requestCtorCall = new ConstructorCallBuilder((INamedTypeSymbol) requestType);

        foreach(var property in requestProps)
        {
            string paramName = NameUtils.ToValidVariableName(property.Name);
            functionBuilder.AddArgument((INamedTypeSymbol) property.Type, paramName);
            requestCtorCall.AddInitializer(property, paramName);
        }

        var requestVarName = "__request";
        functionBuilder.AddStatement(requestCtorCall.ToVariableAssignment(requestVarName));
        queryFunctionCall.AddArgument(requestVarName);

        foreach(var parameter in queryParams)
        {
            functionBuilder.AddArgument((INamedTypeSymbol) parameter.Type, parameter.Name, 
                parameter.HasExplicitDefaultValue, parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : default);
        }

        functionBuilder.AddStatement($"return {queryFunctionCall.Build()}");

        return (functionBuilder.BuildInterfaceDefinition(), functionBuilder.BuildMethodCode());
    }

    private static string GetMessageTypeGeneratedCode(INamedTypeSymbol messageType)
    {
        string msgName = messageType.Name.Substring(3);
        //Note: typeof does not work as transitive dependencies (Google.Protobuf) are unavailable in source generator
        string wrapperName = "TxMessage";
        //typeof(TxMessage<>).FullName.Substring(0, typeof(TxMessage<>).FullName.Length - 2);

        var msgProps = GetTypeInstanceProperties(messageType)
            .ToArray();

        var functionBuilder = new FunctionBuilder(msgName)
            .WithVisibility(FunctionVisibility.Public)
            .WithReturnTypeRaw($"global::Cosm.Net.Core.Msg.I{wrapperName}<{NameUtils.FullyQualifiedTypeName(messageType)}>");

        var txObjectBuilder = new ConstructorCallBuilder(
            $"global::Cosm.Net.Core.Msg.{wrapperName}<{NameUtils.FullyQualifiedTypeName(messageType)}>");
        var msgObjectBuilder = new ConstructorCallBuilder(messageType);

        foreach(var property in msgProps)
        {
            string paramName = NameUtils.ToValidVariableName(property.Name);

            functionBuilder.AddArgument((INamedTypeSymbol) property.Type, paramName);
            msgObjectBuilder.AddInitializer(property, paramName);
        }

        string msgVarName = "__msg";
        functionBuilder.AddStatement(msgObjectBuilder.ToVariableAssignment(msgVarName));
        txObjectBuilder.AddArgument(msgVarName);
        functionBuilder.AddStatement($"return {txObjectBuilder.ToInlineCall()}");

        return functionBuilder.BuildMethodCode();
    }

    private static IEnumerable<IMethodSymbol> GetQueryClientQueryMethods(ITypeSymbol queryClientType)
        => queryClientType.GetMembers()
            .Where(x => x.Name.EndsWith("Async"))
            .Where(x => x is IMethodSymbol)
            .Where(x => !x.IsObsolete())
            .Cast<IMethodSymbol>();

    private static ITypeSymbol GetQueryMethodRequestType(IMethodSymbol methodType)
        => methodType.Parameters[0].Type;

    private static IEnumerable<IPropertySymbol> GetTypeInstanceProperties(ITypeSymbol type)
        => type.GetMembers()
            .Where(x => x is IPropertySymbol)
            .Cast<IPropertySymbol>()
            .Where(x => !x.IsStatic && (!x.IsReadOnly || x.Type.Name == "RepeatedField"))
            .Where(x => !x.IsObsolete());
}
