using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.CosmWasm;
public class GeneratedType
{
    public string Name { get; }
    public bool HasDefaultValue { get; }

    public string ExplicitDefaultValue { get; }

    public GeneratedType(string name, bool hasDefaultValue, string explicitDefaultValue = "default")
    {
        Name = name;
        HasDefaultValue = hasDefaultValue;
        ExplicitDefaultValue = explicitDefaultValue;
    }

    public override string ToString()
        => throw new NotSupportedException();
}
