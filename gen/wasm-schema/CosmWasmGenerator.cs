using Cosm.Net.Generators.CosmWasm.Models;
using Cosm.Net.Generators.CosmWasm.TypeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace Cosm.Net.Generators.CosmWasm;

[Generator]
public class CosmWasmGenerator : IIncrementalGenerator
{
    private const string ContractSchemaFilePathAttribute = "ContractSchemaFilePathAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var contractTypesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(IsCandidateNode, TransformToContractTypeInfo)
            .Where(contractType => contractType is not null)
            .Select((contractType, _) => contractType!);

        var additionalFilesProvider = context.AdditionalTextsProvider
            .Select((file, _) => file)
            .Where(file => file.Path.EndsWith(".json"));

        var combined = contractTypesProvider.Combine(additionalFilesProvider.Collect());

        context.RegisterSourceOutput(combined, GenerateSource);
    }

    private static bool IsCandidateNode(SyntaxNode node, CancellationToken _) 
        => node is ClassDeclarationSyntax cd &&
               cd.AttributeLists
                   .SelectMany(list => list.Attributes)
                   .Any(attr => attr.Name.ToString().Contains(ContractSchemaFilePathAttribute));

    private static ContractTypeInfo? TransformToContractTypeInfo(GeneratorSyntaxContext context, CancellationToken _)
    {
        // Extract the contract type information based on the attribute
        var classDeclaration = (ClassDeclarationSyntax) context.Node;

        if(context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol contractTypeSymbol)
        {
            return null;
        }

        var attributeData = contractTypeSymbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == ContractSchemaFilePathAttribute);

        if(attributeData == null)
        {
            return null;
        }

        var schemaPath = attributeData.ConstructorArguments[0].Value?.ToString();
        return new ContractTypeInfo(contractTypeSymbol, schemaPath);
    }

    private static void GenerateSource(SourceProductionContext context, (ContractTypeInfo contractType, ImmutableArray<AdditionalText> additionalFiles) combined)
    {
        var (contractType, additionalFiles) = combined;

        if(contractType.SchemaPath is null)
        {
            ReportDiagnostic(context, GeneratorDiagnostics.SchemaFileNotFound, contractType.Symbol);
            return;
        }

        // Find the schema file that matches the schema path
        var schemaFile = additionalFiles.FirstOrDefault(file => file.Path.EndsWith(contractType.SchemaPath));
        if(schemaFile is null)
        {
            ReportDiagnostic(context, GeneratorDiagnostics.SchemaFileNotFound, contractType.Symbol);
            return;
        }

        var schemaText = schemaFile.GetText()?.ToString();
        if(string.IsNullOrEmpty(schemaText) || schemaText is null)
        {
            ReportDiagnostic(context, GeneratorDiagnostics.SchemaFileMalformed, contractType.Symbol);
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
            ReportDiagnostic(context, GeneratorDiagnostics.SchemaFileMalformed, contractType.Symbol, ex);
            return;
        }

        // Generate the binding file using CosmWasmTypeGenerator
        var generator = new CosmWasmTypeGenerator();
        var generatedCode = generator
            .GenerateCosmWasmBindingFile(contractSchema, contractType.Symbol.Name, contractType.Symbol.ContainingNamespace.ToString())
            .GetAwaiter().GetResult();

        // Add the generated source code to the compilation
        context.AddSource($"{contractType.Symbol.Name}.generated.cs", SourceText.From(generatedCode, Encoding.UTF8));
    }

    private static void ReportDiagnostic(SourceProductionContext context, DiagnosticDescriptor descriptor, ISymbol symbol, Exception? ex = null)
    {
        var diagnostic = Diagnostic.Create(descriptor, symbol.Locations.FirstOrDefault(), ex?.Message ?? string.Empty);
        context.ReportDiagnostic(diagnostic);
    }
}

// Helper record to store contract type information
internal class ContractTypeInfo(INamedTypeSymbol symbol, string? schemaPath)
{
    public INamedTypeSymbol Symbol { get; } = symbol;
    public string? SchemaPath { get; } = schemaPath;
}