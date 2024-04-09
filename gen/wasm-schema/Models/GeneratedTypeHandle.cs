namespace Cosm.Net.Generators.CosmWasm.Models;
public class GeneratedTypeHandle
{
    public string Name { get; set; }

    public string? DefaultValue { get; set; }

    public GeneratedTypeHandle(string name, string? defaultValue)
    {
        Name = name;
        DefaultValue = defaultValue;
    }

    public GeneratedTypeHandle ToNullable()
    {
        if(Name.EndsWith("?"))
        {
            return this;
        }

        Name = $"{Name}?";
        DefaultValue ??= "null";

        return this;
    }

    public GeneratedTypeHandle ToArray()
    {
        Name = $"{Name}[]";
        DefaultValue = null;
        return this;
    }
}
