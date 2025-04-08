using Cosm.Net.Generators.Common.Extensions;
using Cosm.Net.Generators.Common.Util;
using Cosm.Net.Generators.Proto.Adapters;
using Cosm.Net.Generators.Proto.Adapters.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Cosm.Net.Generators.Proto;

[Generator]
public class QuerierGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var queryClientTypesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, ct) => node is ClassDeclarationSyntax,
                transform: GetSemanticTargetForGeneration
            )
            .Where(type => type is not null)
            .Select((type, ct) => type!);

        var msgClassesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, ct) => node is ClassDeclarationSyntax cd && cd.Identifier.Text.StartsWith("Msg"),
                transform: TransformToMsgClassInfo)
            .Where(msgInfo => msgInfo.HasValue)
            .Select((x, _) => x!.Value)
            .Collect()
            .Select((msgClasses, _) => MatchMsgAndResponsePairs(msgClasses));

        var eventAttributeKeyPropertyProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax classDecl && classDecl.Identifier.Text == "EventAttribute",
                transform: (ctx, _) => GetKeyProperty(ctx)
            )
            .Where(x => x is not null);

        var compilationAndClasses = context.CompilationProvider
            .Combine(queryClientTypesProvider.Collect())
            .Combine(msgClassesProvider)
            .Combine(eventAttributeKeyPropertyProvider.Collect());

        context.RegisterSourceOutput(compilationAndClasses, (spc, combined) =>
        {
            var (((compilation, queryClients), matchedMsgClasses), eventKeyAttributes) = combined;
            Execute(spc, compilation, queryClients, matchedMsgClasses, eventKeyAttributes!);
        });
    }

    public static PropertyDeclarationSyntax? GetKeyProperty(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax) context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

        if(classSymbol is null)
        {
            return null;
        }
        if(!classSymbol.ContainingNamespace.ToString().Contains("Tendermint.Abci"))
        {
            return null;
        }

        foreach(var member in classDeclaration.Members)
        {
            if(
                member is PropertyDeclarationSyntax property &&
                property.Identifier.Text == "Key"
            )
            {
                return property;
            }
        }

        return null;
    }

    public static string GetNamespace(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax) context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        return classSymbol?.ContainingNamespace?.ToString() ?? "";
    }

    private static (string Name, INamedTypeSymbol Symbol)? TransformToMsgClassInfo(GeneratorSyntaxContext context, CancellationToken _)
    {
        var classDeclaration = (ClassDeclarationSyntax) context.Node;

        if(context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }
        //
        return (classSymbol.Name, classSymbol);
    }

    private static ImmutableArray<INamedTypeSymbol> MatchMsgAndResponsePairs(ImmutableArray<(string Name, INamedTypeSymbol Symbol)> msgClasses)
    {
        var msgTypeCandidates = new List<INamedTypeSymbol>();
        var msgResponseCandidates = new List<INamedTypeSymbol>();
        var matchedMsgTypes = new List<INamedTypeSymbol>();

        foreach(var (name, symbol) in msgClasses)
        {
            if(IsMsgCandidate(name))
            {
                var matchingResponse = msgResponseCandidates
                    .Find(response => AreMatchingCandidates(name, response));
                if(matchingResponse != null)
                {
                    msgResponseCandidates.Remove(matchingResponse);
                    matchedMsgTypes.Add(symbol);
                }
                else
                {
                    msgTypeCandidates.Add(symbol);
                }
            }
            else if(IsMsgResponseCandidate(name))
            {
                var matchingMsg = msgTypeCandidates
                    .Find(msg => AreMatchingCandidates(msg, name));
                if(matchingMsg != null)
                {
                    msgTypeCandidates.Remove(matchingMsg);
                    matchedMsgTypes.Add(matchingMsg);
                }
                else
                {
                    msgResponseCandidates.Add(symbol);
                }
            }
        }

        return [.. matchedMsgTypes];
    }

    private static bool IsMsgCandidate(string name)
        => name.StartsWith("Msg") && !name.EndsWith("Response");

    private static bool IsMsgResponseCandidate(string name)
        => name.StartsWith("Msg") && name.EndsWith("Response");

    private static bool AreMatchingCandidates(INamedTypeSymbol msgCandidate, string msgResponseName)
        => msgResponseName == msgCandidate.Name + "Response";

    private static bool AreMatchingCandidates(string msgName, INamedTypeSymbol msgResponseCandidate)
        => msgResponseCandidate.Name == msgName + "Response";

    private static INamedTypeSymbol? GetSemanticTargetForGeneration(
        GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var typeDeclarationSyntax = (ClassDeclarationSyntax) context.Node;
        if(!typeDeclarationSyntax.Modifiers.Any(SyntaxKind.PublicKeyword) ||
           !typeDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return null;
        }

        var nestedClients = typeDeclarationSyntax.Members
            .OfType<ClassDeclarationSyntax>()
            .Where(x => x.Identifier.Text.EndsWith("Client"))
            .Where(x => x.Modifiers.Any(SyntaxKind.PublicKeyword))
            .ToArray();

        if(nestedClients.Length != 1)
        {
            return null;
        }

        var queryClient = nestedClients.Single();

        var typeSymbol = context.SemanticModel.GetDeclaredSymbol(queryClient, cancellationToken);
        if(typeSymbol is null)
        {
            return null;
        }
        if(typeSymbol.Name.Contains("Msg") || typeSymbol.Name.StartsWith("Reflection"))
        {
            return null;
        }
        //
        return typeSymbol;
    }

    private void Execute(SourceProductionContext context, Compilation compilation,
        ImmutableArray<INamedTypeSymbol> queryClients, ImmutableArray<INamedTypeSymbol> msgTypes,
        ImmutableArray<PropertyDeclarationSyntax> eventAttributeKeyProperties)
    {
        var moduleIdentifiers = queryClients.Select(x =>
        {
            var (prefix, name, version) = GetModuleIdentifier(x);
            return new ModuleIdentifier([x], prefix, name, version, [GetQueryClientQueryMethods(x)]);
        }).ToList();
        var moduleNames = new Dictionary<string, string>();

        foreach(var nameGroup in moduleIdentifiers.GroupBy(x => new { x.Prefix, x.Name }).ToArray())
        {
            if(nameGroup.Count() == 1)
            {
                continue;
            }
            if(!TryJoinQueryLists(nameGroup.SelectMany(x => x.QueryMethods)))
            {
                continue;
            }

            moduleIdentifiers.RemoveAll(x => nameGroup.Contains(x));
            moduleIdentifiers.Add(new ModuleIdentifier(
                    [.. nameGroup.SelectMany(x => x.ClientTypes)],
                    nameGroup.Key.Prefix,
                    nameGroup.Key.Name,
                    "",
                    [.. nameGroup.SelectMany(x => x.QueryMethods)]
                )
            );
        }

        foreach(var moduleIdentifier in moduleIdentifiers)
        {
            string uniqueModuleName = GetModuleName(moduleIdentifier, moduleIdentifiers);

            foreach(var client in moduleIdentifier.ClientTypes)
            {
                moduleNames.Add(NameUtils.FullyQualifiedTypeName(client), uniqueModuleName);
            }

            var matchingMsgTypes = msgTypes
                .Where(msgTypeSymbol =>
                    moduleIdentifier.ClientTypes.Any(clientType =>
                        clientType.ContainingNamespace.Equals(msgTypeSymbol.ContainingNamespace, SymbolEqualityComparer.Default)));

            if(!moduleIdentifier.QueryMethods.SelectMany(x => x).Any() && !matchingMsgTypes.Any())
            {
                continue;
            }

            var code = QueryModuleProcessor.GetQueryModuleGeneratedCode(
                uniqueModuleName,
                moduleIdentifier.ClientTypes,
                moduleIdentifier.QueryMethods,
                matchingMsgTypes
            );

            context.AddSource($"{uniqueModuleName}.generated.cs", SourceText.From(code, Encoding.UTF8));
        }

        if(eventAttributeKeyProperties.Length == 0)
        {
            throw new InvalidOperationException("Tendermint Attribute not found");
        }
        if(eventAttributeKeyProperties.Length > 1)
        {
            throw new InvalidOperationException($"Too many Tendermint Attributes found: {eventAttributeKeyProperties.Length}");
        }

        var eventKeyProperty = compilation
            .GetSemanticModel(eventAttributeKeyProperties[0].SyntaxTree)
            .GetTypeInfo(eventAttributeKeyProperties[0].Type);

        bool useStringEvents = !eventKeyProperty.Type!.Name.Contains("ByteString");

        string txModuleCode = useStringEvents
            ? TxModuleAdapter.Code.Replace(".ToStringUtf8()", "")
            : TxModuleAdapter.Code;

        context.AddSource("TxModuleAdapter.generated.cs", txModuleCode);
        context.AddSource("AuthModuleAdapter.generated.cs", AuthModuleAdapter.Code);
        context.AddSource("TendermintModuleAdapter.generated.cs", TendermintModuleAdapter.Code);
        context.AddSource("BankModuleAdapter.generated.cs", BankModuleAdapter.Code);
        context.AddSource("BlocksAdapter.generated.cs", BlocksAdapter.Code);

        var clientModuleName = moduleNames.First(x => x.Key.Contains("Ibc.Core.Client.V1")).Value;
        var channelModuleName = moduleNames.First(x => x.Key.Contains("Ibc.Core.Channel.V1")).Value;

        context.AddSource("IbcAdapter.generated.cs", IbcAdapter.Code(clientModuleName, channelModuleName));

        var wasmModuleName = moduleNames.FirstOrDefault(x => x.Key.Contains("Cosmwasm.Wasm.V1")).Value;
        if(wasmModuleName is not null)
        {
            context.AddSource("WasmModuleAdapter.generated.cs", WasmModuleAdapter.Code(wasmModuleName));
        }
    }

    private string GetModuleName(ModuleIdentifier moduleIdentifier, IReadOnlyList<ModuleIdentifier> moduleIdentifiers)
    {
        if(!moduleIdentifiers.Any(x => x.Name == moduleIdentifier.Name && x != moduleIdentifier))
        {
            return $"{moduleIdentifier.Name}Module";
        }

        var conflicts = moduleIdentifiers.Where(x => x.Name == moduleIdentifier.Name).ToArray();

        bool allPrefixesMatch = conflicts.All(x => x.Prefix == moduleIdentifier.Prefix);
        bool allVersionsMatch = conflicts.All(x => x.Version == moduleIdentifier.Version);

        if(allPrefixesMatch && allVersionsMatch)
        {
            throw new NotSupportedException(
                $"Unresolvable conflict: {string.Join(",", conflicts.SelectMany(x => x.ClientTypes).Select(NameUtils.FullyQualifiedTypeName))}");
        }
        else if(allPrefixesMatch)
        {
            if(string.IsNullOrEmpty(moduleIdentifier.Version))
            {
                throw new NotSupportedException(
                    $"Unresolvable conflict: {string.Join(",", conflicts.SelectMany(x => x.ClientTypes).Select(NameUtils.FullyQualifiedTypeName))}");
            }
            //
            return $"{moduleIdentifier.Name}{moduleIdentifier.Version}Module";
        }
        else if(allVersionsMatch)
        {
            if(string.IsNullOrEmpty(moduleIdentifier.Prefix))
            {
                throw new NotSupportedException(
                    $"Unresolvable conflict: {string.Join(",", conflicts.SelectMany(x => x.ClientTypes).Select(NameUtils.FullyQualifiedTypeName))}");
            }
            //
            return $"{moduleIdentifier.Prefix}{moduleIdentifier.Name}Module";
        }
        //
        return $"{moduleIdentifier.Prefix}{moduleIdentifier.Name}{moduleIdentifier.Version}Module";
    }

    private static (string Prefix, string Name, string Version) GetModuleIdentifier(INamedTypeSymbol queryClientType)
    {
        string fullNamespace = queryClientType.ContainingNamespace.ToString();
        var namespaceParts = fullNamespace.Split('.').ToList();

        string namePrefix = namespaceParts[0] == "Ibc" ? "Ibc" : "";

        int versionIndex = namespaceParts.FindIndex(x => x[0] == 'V' && char.IsDigit(x[x.Length - 1]));

        if(versionIndex == -1)
        {
            return (
                $"{namespaceParts[namespaceParts.Count - 2]}",
                $"{namePrefix}{namespaceParts[namespaceParts.Count - 1]}",
                ""
            );
        }
        if(versionIndex < 1)
        {
            throw new NotSupportedException($"Module identifier does not contain prefix or name: {NameUtils.FullyQualifiedTypeName(queryClientType)}");
        }
        //
        return (
            versionIndex >= 2 ? namespaceParts[versionIndex - 2] : "",
            $"{namePrefix}{namespaceParts[versionIndex - 1]}",
            namespaceParts[versionIndex]
        );
    }

    private static IMethodSymbol[] GetQueryClientQueryMethods(ITypeSymbol queryClientType)
        => queryClientType.GetMembers()
            .Where(x => x is IMethodSymbol)
            .Cast<IMethodSymbol>()
            .Where(x => !x.IsObsolete())
            .Where(x => x.ReturnType.Name == "AsyncUnaryCall" || x.ReturnType.Name == "AsyncServerStreamingCall")
            .ToArray();

    private static bool TryJoinQueryLists(IEnumerable<IEnumerable<IMethodSymbol>> queryMethods)
    {
        var output = new List<IMethodSymbol>();

        foreach(var queryList in queryMethods)
        {
            foreach(var query in queryList)
            {
                if(output.Any(x => x.Name == query.Name))
                {
                    return false;
                }
            }

            output.AddRange(queryList);
        }

        return true;
    }
}
