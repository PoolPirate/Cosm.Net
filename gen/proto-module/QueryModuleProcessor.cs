using Cosm.Net.Generators.Common.Extensions;
using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.Common.Util;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosm.Net.Generators.Proto;
public static class QueryModuleProcessor
{
    private const string _cosmosCoinTypeName = "global::Cosmos.Base.V1Beta1.Coin";
    private const string _ibcHeightTypeName = "global::Ibc.Core.Client.V1.Height";

    public static ITypeSymbol GetQueryClientType(INamedTypeSymbol moduleType)
        => moduleType.AllInterfaces
            .Where(x => x.Name == "ICosmModule")
            .Select(x => x.TypeArguments[0])
            .First();

    public static string GetQueryModuleGeneratedCode(
        string moduleName, INamedTypeSymbol[] queryClientTypes, 
        IEnumerable<IMethodSymbol>[] queryMethods, IEnumerable<INamedTypeSymbol> messageTypes)
    {
        var methodCodeBuilder = new StringBuilder();
        var interfaceCodeBuilder = new StringBuilder();

        for(int i = 0; i < queryClientTypes.Length; i++)
        {
            foreach(var method in queryMethods[i])
            {
                (string interfaceMethod, string methodCode) = GetQueryMethodGeneratedCode(method, i);

                _ = methodCodeBuilder.AppendLine(methodCode);
                _ = interfaceCodeBuilder.AppendLine(interfaceMethod);
            }
        }

        foreach(var messageType in messageTypes)
        {
            (string interfaceMethod, string methodCode) = GetMessageTypeGeneratedCode(messageType);

            _ = methodCodeBuilder.AppendLine(methodCode);
            _ = interfaceCodeBuilder.AppendLine(interfaceMethod);
        }

        var usingsSection = new StringBuilder();
        var ctorSection = new StringBuilder();
        var fieldSection = new StringBuilder();

        for(int i = 0; i < queryClientTypes.Length; i++)
        {
            var clientType = queryClientTypes[i];

            usingsSection.AppendLine($"using global::{clientType.ContainingNamespace};");
            ctorSection.AppendLine($"_client{i} = new {NameUtils.FullyQualifiedTypeName(clientType)}(callInvoker);");
            fieldSection.AppendLine($"private readonly {NameUtils.FullyQualifiedTypeName(clientType)} _client{i};");
        }

        var code =
            $$"""
            {{usingsSection}}

            namespace Cosm.Net.Modules;

            public interface I{{moduleName}} : IModule {
            {{interfaceCodeBuilder}}
            }

            internal class {{moduleName}} : I{{moduleName}} {
            {{fieldSection}}

            public {{moduleName}}(global::Grpc.Core.CallInvoker callInvoker)
            {
            {{ctorSection}}
            }

            {{methodCodeBuilder}}
            }

            """;

        return code;
    }

    private static (string interfaceMethod, string methodCode) GetQueryMethodGeneratedCode(IMethodSymbol queryMethod, int clientIndex)
    {
        var requestType = GetQueryMethodRequestType(queryMethod);
        var requestProps = GetTypeInstanceProperties(requestType);
        var queryParams = queryMethod.Parameters.Skip(1).ToArray();

        var functionBuilder = new FunctionBuilder(queryMethod.Name)
            .WithReturnType((INamedTypeSymbol) queryMethod.ReturnType);

        var queryFunctionCall = new MethodCallBuilder($"_client{clientIndex}", queryMethod);
        var requestCtorCall = new ConstructorCallBuilder((INamedTypeSymbol) requestType);

        foreach(var property in requestProps)
        {
            string paramName = NameUtils.ToValidVariableName(property.Name);

            if(NameUtils.FullyQualifiedTypeName((INamedTypeSymbol) property.Type) == _cosmosCoinTypeName)
            {
                _ = functionBuilder.AddArgument($"global::Cosm.Net.Models.Coin", paramName);
                _ = requestCtorCall.AddInitializer(property,
                    $$"""
                    new {{_cosmosCoinTypeName}}() {
                        Denom = {{paramName}}.Denom,
                        Amount = {{paramName}}.Amount.ToString()
                    }
                    """);
            }
            else if(NameUtils.FullyQualifiedTypeName((INamedTypeSymbol) property.Type) == _ibcHeightTypeName)
            {
                _ = functionBuilder.AddArgument(_ibcHeightTypeName, paramName);
                _ = requestCtorCall.AddInitializer(property,
                    $$"""
                    new {{_ibcHeightTypeName}}() {
                        RevisionNumber = (ulong) {{paramName}}.RevisionNumber,
                        RevisionHeight = (ulong) {{paramName}}.RevisionHeight
                    }
                    """);
            }
            else
            {
                _ = functionBuilder.AddArgument((INamedTypeSymbol) property.Type, paramName);
                _ = requestCtorCall.AddInitializer(property, paramName);
            }
        }

        string requestVarName = "__request";
        _ = functionBuilder.AddStatement(requestCtorCall.ToVariableAssignment(requestVarName));
        _ = queryFunctionCall.AddArgument(requestVarName);

        foreach(var parameter in queryParams)
        {
            _ = functionBuilder.AddArgument((INamedTypeSymbol) parameter.Type, parameter.Name,
                parameter.HasExplicitDefaultValue, parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : default);
            _ = queryFunctionCall.AddArgument(parameter.Name);
        }

        _ = functionBuilder.AddStatement($"return {queryFunctionCall.Build()}");

        return (functionBuilder.BuildInterfaceDefinition(), functionBuilder.BuildMethodCode());
    }

    private static (string interfaceMethod, string methodCode) GetMessageTypeGeneratedCode(INamedTypeSymbol messageType)
    {
        string msgName = messageType.Name.Substring(3);
        //Note: typeof does not work as transitive dependencies (Google.Protobuf) are unavailable in source generator
        string wrapperName = "TxMessage";
        //typeof(TxMessage<>).FullName.Substring(0, typeof(TxMessage<>).FullName.Length - 2);
        var msgProps = GetTypeInstanceProperties(messageType)
            .ToArray();

        var functionBuilder = new FunctionBuilder(msgName)
            .WithVisibility(FunctionVisibility.Public)
            .WithReturnTypeRaw($"global::Cosm.Net.Tx.Msg.I{wrapperName}<{NameUtils.FullyQualifiedTypeName(messageType)}>");

        var txObjectBuilder = new ConstructorCallBuilder(
            $"global::Cosm.Net.Tx.Msg.{wrapperName}<{NameUtils.FullyQualifiedTypeName(messageType)}>");
        var msgObjectBuilder = new ConstructorCallBuilder(messageType);

        foreach(var property in msgProps)
        {
            string paramName = NameUtils.ToValidVariableName(property.Name);
            var namedType = (INamedTypeSymbol) property.Type;

            if(NameUtils.FullyQualifiedTypeName(namedType) == _cosmosCoinTypeName)
            {
                _ = functionBuilder.AddArgument("global::Cosm.Net.Models.Coin", paramName);
                _ = msgObjectBuilder.AddInitializer(property,
                    $$"""
                    new {{_cosmosCoinTypeName}}() {
                        Denom = {{paramName}}.Denom,
                        Amount = {{paramName}}.Amount.ToString()
                    }
                    """);
            }
            else if(namedType.Name == "RepeatedField" && NameUtils.FullyQualifiedTypeName((INamedTypeSymbol) namedType.TypeArguments[0]) == _cosmosCoinTypeName)
            {
                _ = functionBuilder.AddArgument($"global::System.Collections.Generic.IEnumerable<global::Cosm.Net.Models.Coin>", paramName);
            }
            else if(NameUtils.FullyQualifiedTypeName(namedType) == _ibcHeightTypeName)
            {
                _ = functionBuilder.AddArgument("global::Cosm.Net.Models.Height", paramName);
                _ = msgObjectBuilder.AddInitializer(property,
                    $$"""
                    new {{_ibcHeightTypeName}}() {
                        RevisionNumber = (ulong) {{paramName}}.RevisionNumber,
                        RevisionHeight = (ulong) {{paramName}}.RevisionHeight
                    }
                    """);
            }
            else
            {
                _ = functionBuilder.AddArgument((INamedTypeSymbol) property.Type, paramName);
                _ = msgObjectBuilder.AddInitializer(property, paramName);
            }
        }

        string msgVarName = "__msg";
        _ = functionBuilder.AddStatement(msgObjectBuilder.ToVariableAssignment(msgVarName));

        foreach(var property in msgProps)
        {
            string paramName = NameUtils.ToValidVariableName(property.Name);
            var namedType = (INamedTypeSymbol) property.Type;

            if(namedType.Name == "RepeatedField" && NameUtils.FullyQualifiedTypeName((INamedTypeSymbol) namedType.TypeArguments[0]) == _cosmosCoinTypeName)
            {
                _ = functionBuilder.AddStatement(
                    $$"""
                    {{msgVarName}}.{{property.Name}}.AddRange(
                        {{paramName}}.Select(x => new {{_cosmosCoinTypeName}}() {
                            Denom = x.Denom,
                            Amount = x.Amount.ToString()
                        }))
                    """);
            }
        }

        _ = txObjectBuilder.AddArgument(msgVarName);
        _ = functionBuilder.AddStatement($"return {txObjectBuilder.ToInlineCall()}");

        return (functionBuilder.BuildInterfaceDefinition(), functionBuilder.BuildMethodCode());
    }

    private static ITypeSymbol GetQueryMethodRequestType(IMethodSymbol methodType)
        => methodType.Parameters[0].Type;

    private static IEnumerable<IPropertySymbol> GetTypeInstanceProperties(ITypeSymbol type)
        => type.GetMembers()
            .Where(x => x is IPropertySymbol)
            .Cast<IPropertySymbol>()
            .Where(x => !x.IsStatic && (!x.IsReadOnly || x.Type.Name.StartsWith("RepeatedField")))
            .Where(x => !x.IsObsolete());
}
