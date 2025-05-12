﻿using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.CosmWasm.Models;
using Microsoft.CodeAnalysis;
using System.Text;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public class CosmWasmTypeGenerator
{
    private readonly GeneratedTypeAggregator _typeAggregator;
    private readonly ObjectTypeGenerator _objectTypeGenerator;
    private readonly EnumerationTypeGenerator _enumerationGenerator;
    private readonly SchemaTypeGenerator _schemaTypeGenerator;
    private readonly JsonObjectTypeGenerator _jsonObjectTypeGenerator;
    private readonly TupleTypeGenerator _tupleTypeGenerator;

    private readonly QueryGenerator _queryGenerator;
    private readonly MsgGenerator _msgGenerator;

    public CosmWasmTypeGenerator()
    {
        _typeAggregator = new GeneratedTypeAggregator();
        _enumerationGenerator = new EnumerationTypeGenerator();
        _schemaTypeGenerator = new SchemaTypeGenerator();
        _objectTypeGenerator = new ObjectTypeGenerator();
        _jsonObjectTypeGenerator = new JsonObjectTypeGenerator();
        _tupleTypeGenerator = new TupleTypeGenerator();

        _enumerationGenerator.Initialize(_typeAggregator, _objectTypeGenerator);
        _schemaTypeGenerator.Initialize(_enumerationGenerator, _jsonObjectTypeGenerator);
        _objectTypeGenerator.Initialize(_typeAggregator, _schemaTypeGenerator);
        _jsonObjectTypeGenerator.Initialize(_objectTypeGenerator, _schemaTypeGenerator, _tupleTypeGenerator);
        _tupleTypeGenerator.Initialize(_schemaTypeGenerator);

        _queryGenerator = new QueryGenerator(_schemaTypeGenerator);
        _msgGenerator = new MsgGenerator(_schemaTypeGenerator);
    }

    public async Task<string> GenerateCosmWasmBindingFile(ContractAPISchema apiSchema, string contractInterfaceName, string targetNamespace,
        Action<DiagnosticDescriptor, string[]> reportDiagnostic)
    {
        var responseSchemas = await apiSchema.GetResponseSchemasAsync();

        var querySchema = await apiSchema.GetQuerySchemaAsync();
        var msgSchema = await apiSchema.GetExecuteSchemaAsync();

        var contractClassBuilder = new ClassBuilder(GetContractClassName(contractInterfaceName))
            .WithVisibility(ClassVisibility.Internal)
            .WithIsPartial()
            .AddField(new FieldBuilder("global::Cosm.Net.Adapters.Internal.IInternalWasmAdapter", "_wasm"))
            .AddProperty(new PropertyBuilder("global::System.String", "ContractAddress").WithSetterVisibility(SetterVisibility.Private))
            .AddProperty(new PropertyBuilder("global::System.String?", "CodeHash").WithSetterVisibility(SetterVisibility.Private))
            .AddBaseType("global::Cosm.Net.Models.IWasmContract", true);

        foreach(var query in querySchema.OneOf)
        {
            try
            {
                var func = _queryGenerator.GenerateQueryFunction(query, querySchema, responseSchemas);
                contractClassBuilder.AddFunction(func);
            }
            catch(Exception ex)
            {
                var propSchema = query.Properties.Single().Value;
                reportDiagnostic(GeneratorDiagnostics.MemberGenerationFailed, [propSchema.Name, ex.Message]);
            }
        }
        foreach(var msg in msgSchema.OneOf)
        {
            try
            {
                var func = _msgGenerator.GenerateMsgFunction(msg, msgSchema, responseSchemas);
                contractClassBuilder.AddFunction(func);
            }
            catch(Exception ex)
            {
                var propSchema = msg.Properties.Single().Value;
                reportDiagnostic(GeneratorDiagnostics.MemberGenerationFailed, [propSchema.Name, ex.Message]);
            }
        }

        var generatedTypesSb = new StringBuilder();

        foreach(var type in _typeAggregator.GetGeneratedTypes())
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

    private string GetContractClassName(string contractInterfaceName)
        => $"{contractInterfaceName}_Generated_Implementation";
}
