using Cosm.Net.Generators.CosmWasm.Models;
using Cosm.Net.Generators.CosmWasm.TypeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace Cosm.Net.Generators.CosmWasm;

[Generator]
public class CosmWasmGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var contractTypesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                IsCandidateNode,
                (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol((InterfaceDeclarationSyntax) ctx.Node)
            )
            .Where(contractType =>
                contractType is not null &&
                contractType.AllInterfaces.Any(static x => x.Name == "IContract")
            )
            .Select((contractType, _) =>
            {
                var attribute = contractType!.GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass?.Name == "ContractSchemaFilePathAttribute");

                return (
                    contractType!,
                    attribute?.ConstructorArguments.Length == 1
                        ? attribute.ConstructorArguments[0].Value?.ToString()
                        : null
                );
            });

        var additionalFilesProvider = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".json"));

        var combined = contractTypesProvider.Combine(additionalFilesProvider.Collect());

        context.RegisterSourceOutput(combined, GenerateSource);
    }

    private static bool IsCandidateNode(SyntaxNode node, CancellationToken _)
        => node is InterfaceDeclarationSyntax cd &&
            cd.AttributeLists.Count != 0 &&
            cd.BaseList is not null &&
            cd.Modifiers.Any(SyntaxKind.PartialKeyword);

    private static void GenerateSource(SourceProductionContext context,
        ((INamedTypeSymbol, string?), ImmutableArray<AdditionalText> additionalFiles) combined)
    {
        var ((contractSymbol, schemaFileName), additionalFiles) = combined;

        if(schemaFileName is null)
        {
            ReportDiagnostic(context, GeneratorDiagnostics.SchemaFileNotSpecified, contractSymbol, contractSymbol.Name);
            return;
        }

        var schemaFile = additionalFiles.FirstOrDefault(file => file.Path.EndsWith(schemaFileName));
        if(schemaFile is null)
        {
            ReportDiagnostic(context, GeneratorDiagnostics.SchemaFileNotFound, contractSymbol, schemaFileName);
            return;
        }

        var schemaText = schemaFile.GetText()?.ToString();
        if(string.IsNullOrEmpty(schemaText) || schemaText is null)
        {
            ReportDiagnostic(context, GeneratorDiagnostics.SchemaFileMalformed, contractSymbol);
            return;
        }

        ContractAPISchema? contractSchema;
        try
        {
            contractSchema = JsonSerializer.Deserialize<ContractAPISchema>(schemaText)
                ?? throw new NotSupportedException("Parsing schema file to ContractAPISchema failed");
        }
        catch(Exception ex)
        {
            ReportDiagnostic(context, GeneratorDiagnostics.SchemaFileMalformed, contractSymbol, ex);
            return;
        }

        try
        {
            var generator = new CosmWasmTypeGenerator();
            var generatedCode = generator
                .GenerateCosmWasmBindingFile(contractSchema, contractSymbol.Name, contractSymbol.ContainingNamespace.ToString())
                .GetAwaiter().GetResult();

            context.AddSource($"{contractSymbol.Name}.generated.cs", SourceText.From(generatedCode, Encoding.UTF8));
        }
        catch(Exception ex)
        {
            ReportDiagnostic(context, GeneratorDiagnostics.GenerationFailed, contractSymbol, ex);
        }
    }

    private static void ReportDiagnostic(SourceProductionContext context, DiagnosticDescriptor descriptor, ISymbol symbol, params string[] args)
    {
        var diagnostic = Diagnostic.Create(descriptor, symbol.Locations.FirstOrDefault(), args);
        context.ReportDiagnostic(diagnostic);
    }

    private static void ReportDiagnostic(SourceProductionContext context, DiagnosticDescriptor descriptor, ISymbol symbol, Exception e)
    {
        var diagnostic = Diagnostic.Create(descriptor, symbol.Locations.FirstOrDefault(), e.Message);
        context.ReportDiagnostic(diagnostic);
    }
}