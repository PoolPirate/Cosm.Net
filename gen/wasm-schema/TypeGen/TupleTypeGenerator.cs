using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.CosmWasm.Models;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public class TupleTypeGenerator
{
    private SchemaTypeGenerator _schemaTypeGenerator = null!;

    public void Initialize(SchemaTypeGenerator schemaTypeGenerator)
    {
        _schemaTypeGenerator = schemaTypeGenerator;
    }

    public GeneratedTypeHandle GenerateTupleType(JsonSchema arraySchema, JsonSchema definitionsSource)
    {
        var tupleBuilder = new TupleBuilder();

        foreach(var itemSchema in arraySchema.Items)
        {
            var itemType = _schemaTypeGenerator.GetOrGenerateSchemaType(itemSchema, definitionsSource);
            tupleBuilder.AddElement(itemType.Name);
        }

        return new GeneratedTypeHandle(tupleBuilder.TypeName, null);
    }
}
