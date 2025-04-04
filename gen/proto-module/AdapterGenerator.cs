using System;
using System.Collections.Immutable;
using Cosm.Net.Generators.Proto.Adapters;
using Cosm.Net.Generators.Proto.Adapters.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cosm.Net.Generators.Proto;

[Generator]
public class EventAttributeKeyTypeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var eventAttributeKeyPropertyProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax classDecl && classDecl.Identifier.Text == "EventAttribute",
                transform: (ctx, _) => GetKeyProperty(ctx)
            )
            .Where(x => x is not null);

        var cosmWasmQueryClientProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax classDecl && classDecl.Identifier.Text == "Query",
                transform: (ctx, _) => GetNamespace(ctx)
            )
            .Where(x => x == "Cosmwasm.Wasm.V1");

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(
                eventAttributeKeyPropertyProvider.Collect().Combine(
                cosmWasmQueryClientProvider.Collect())
            ),
            (spc, source) => Execute(spc, source.Left, source.Right.Left!, source.Right.Right)
        );
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

    static void Execute(SourceProductionContext context, Compilation compilation, 
        ImmutableArray<PropertyDeclarationSyntax> eventAttributeKeyProperties, ImmutableArray<string> cosmWasmNamespaces)
    {
        if(eventAttributeKeyProperties.Length == 0)
        {
            throw new InvalidOperationException("Tendermint Attribute not found");
        }
        if(eventAttributeKeyProperties.Length > 1)
        {
            throw new InvalidOperationException($"Too many Tendermint Attributes found: {eventAttributeKeyProperties.Length}");
        }

        var typeInfo = compilation
            .GetSemanticModel(eventAttributeKeyProperties[0].SyntaxTree)
            .GetTypeInfo(eventAttributeKeyProperties[0].Type);

        bool useStringEvents = !typeInfo.Type!.Name.Contains("ByteString");

        string txModuleCode = useStringEvents
            ? TxModuleAdapter.Code.Replace(".ToStringUtf8()", "")
            : TxModuleAdapter.Code;

        context.AddSource("TxModuleAdapter.generated.cs", txModuleCode);
        context.AddSource("AuthModuleAdapter.generated.cs", AuthModuleAdapter.Code);
        context.AddSource("TendermintModuleAdapter.generated.cs", TendermintModuleAdapter.Code);
        context.AddSource("BankModuleAdapter.generated.cs", BankModuleAdapter.Code);

        if (cosmWasmNamespaces.Length != 0)
        {
            context.AddSource("WasmModuleAdapter.generated.cs", WasmModuleAdapater.Code);
        }
    }
}
