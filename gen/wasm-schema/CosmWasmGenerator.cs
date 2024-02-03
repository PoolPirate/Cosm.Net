using Cosm.Net.Generators.Common.SourceGeneratorKit;
using Cosm.Net.Generators.CosmWasm.Models;
using Cosm.Net.Generators.CosmWasm.TypeGen;
using Microsoft.CodeAnalysis;
using System.Text.Json;

namespace Cosm.Net.Generators.CosmWasm;

[Generator]
public class CosmWasmGenerator : ISourceGenerator
{
    private const string ContractSchemaFilePathAttribute = "ContractSchemaFilePathAttribute";
    private readonly InterfacesWithAttributeReceiver ContractTypeReceiver = new InterfacesWithAttributeReceiver(ContractSchemaFilePathAttribute);

    public void Initialize(GeneratorInitializationContext context)
        => context.RegisterForSyntaxNotifications(() => ContractTypeReceiver);

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            MakeJITHappy(context);
        }
        catch(Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                GeneratorDiagnostics.ExecutionFailed, Location.None, ex));
            throw;
        }
    }

    public void MakeJITHappy(GeneratorExecutionContext context)
    {
        if(context.SyntaxContextReceiver is null)
        {
            return;
        }

        var contractTypes = ContractTypeReceiver.Types.ToArray();
        foreach(var contractType in contractTypes)
        {
            string schemaPath = contractType
                .FindAttribute(ContractSchemaFilePathAttribute)!
                .ConstructorArguments[0].Value!.ToString();
            var schemaFile = context.AdditionalFiles
                .Where(x => x.Path.EndsWith(schemaPath))
                .FirstOrDefault();
            string? schemaText = schemaFile.GetText()?.ToString();

            if(schemaFile is null || schemaText is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.SchemaFileNotFound, contractType.Locations[0], Array.Empty<object>()));
                continue;
            }

            ContractAPISchema contractSchema = null!;

            try
            {
                contractSchema = JsonSerializer.Deserialize<ContractAPISchema>(schemaText)!;
            }
            catch(Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.SchemaFileMalformed, contractType.Locations[0], ex));
                continue;
            }

            var generator = new CosmWasmTypeGenerator();
            string codeFile = generator.GenerateCosmWasmBindingFile(
                contractSchema, contractType.Name, contractType.ContainingNamespace.ToString())
                .GetAwaiter().GetResult();

            try
            {
                context.AddSource($"{contractType.Name}.generated.cs", codeFile);
            }
            catch
            {
            }
        }
    }
}
