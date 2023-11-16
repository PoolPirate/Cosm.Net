namespace Cosm.Net.Wasm.Attributes;

[AttributeUsage(AttributeTargets.Interface)]
public class ContractSchemaFilePathAttribute : Attribute
{
    public string Path { get; }

    public ContractSchemaFilePathAttribute(string path)
    {
        Path = path;
    }
}
