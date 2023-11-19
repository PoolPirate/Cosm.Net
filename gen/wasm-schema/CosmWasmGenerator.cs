using Cosm.Net.Generators.Common.SourceGeneratorKit;
using Cosm.Net.Generators.Common.Util;
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
        => MakeJITHappy(context);

    public void MakeJITHappy(GeneratorExecutionContext context)
    {
        if(context.SyntaxContextReceiver is null)
        {
            return;
        }

        foreach(var contractType in ContractTypeReceiver.Types)
        {
            var schemaPath = contractType.FindAttribute(ContractSchemaFilePathAttribute)!
                .ConstructorArguments[0].Value!.ToString();
            var schemaFile = context.AdditionalFiles
                .Where(x => x.Path == schemaPath)
                .FirstOrDefault();

            if(schemaFile is null)
            {
                throw new FileNotFoundException("Schema file not found!");
            }

            var schemaText = schemaFile.GetText()!.ToString();

            ContractSchema contractSchema = null!;

            try
            {
                contractSchema = JsonSerializer.Deserialize<ContractSchema>(schemaText)!;
            }
            catch(Exception)
            {
                throw new InvalidOperationException("Misformatted schema file");
            }

            string code = contractSchema.GenerateCSharpCodeFileAsync(
                contractType.Name, contractType.ContainingNamespace.ToString())
                .GetAwaiter().GetResult();

            try
            {
                context.AddSource($"{contractType.Name}.generated.cs", code);
            }
            catch
            {
            }
        }
    }
}
