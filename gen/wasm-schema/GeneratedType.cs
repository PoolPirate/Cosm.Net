using Cosm.Net.Generators.Common.SyntaxElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.CosmWasm;
public class GeneratedType
{
    public SyntaxId? GeneratedSyntaxId { get; }
    public string Name { get; }
    public bool HasDefaultValue { get; }

    public string ExplicitDefaultValue { get; }

    public GeneratedType(string name, bool hasDefaultValue, SyntaxId? generatedSyntaxId = null, string explicitDefaultValue = "default")
    {
        Name = name;
        HasDefaultValue = hasDefaultValue;
        GeneratedSyntaxId = generatedSyntaxId;
        ExplicitDefaultValue = explicitDefaultValue;
    }

    public override string ToString()
        => throw new NotSupportedException();
}
