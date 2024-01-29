using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.CosmWasm.Models;
using System.Text;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public static class CosmWasmTypeGenerator
{
    public static async Task<string> GenerateCosmWasmBindingFile(ContractAPISchema apiSchema, string contractInterfaceName, string targetNamespace)
    {
        var responseSchemas = await apiSchema.GetResponseSchemasAsync();

        var querySchema = await apiSchema.GetQuerySchemaAsync();
        var msgSchema = await apiSchema.GetExecuteSchemaAsync();

        var contractClassBuilder = new ClassBuilder(GetContractClassName(contractInterfaceName))
            .WithVisibility(ClassVisibility.Internal)
            .WithIsPartial()
            .AddField(new FieldBuilder("global::Cosm.Net.Adapters.IWasmAdapater", "_wasm"))
            .AddProperty(new PropertyBuilder("global::System.String", "ContractAddress").WithSetterVisibility(SetterVisibility.Private))
            .AddProperty(new PropertyBuilder("global::System.String?", "CodeHash").WithSetterVisibility(SetterVisibility.Private))
            .AddBaseType("global::Cosm.Net.Models.IContract", true);

        foreach(var query in querySchema.OneOf)
        {
            var func = QueryGenerator.GenerateQueryFunction(query, querySchema, responseSchemas);
            contractClassBuilder.AddFunction(func);
        }
        foreach(var msg in msgSchema.OneOf)
        {
            var func = MsgGenerator.GenerateMsgFunction(msg, msgSchema, responseSchemas);
            contractClassBuilder.AddFunction(func);
        }

        var generatedTypesSb = new StringBuilder();

        foreach(var type in GeneratedTypeAggregator.GetGeneratedTypes())
        {
            generatedTypesSb.AppendLine(type.Build());
        }

        return 
            $$"""
            #nullable enable
            namespace {{targetNamespace}};
            {{contractClassBuilder.Build(generateFieldConstructor: true, generateInterface: true, interfaceName: contractInterfaceName)}}
            {{generatedTypesSb}}
            """;
    }

    private static string GetContractClassName(string contractInterfaceName) 
        => contractInterfaceName.StartsWith("I") 
            ? contractInterfaceName.Substring(1) 
            : $"{contractInterfaceName}Implementation";
}
