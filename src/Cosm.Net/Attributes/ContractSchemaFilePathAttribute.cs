namespace Cosm.Net.Attributes;

[AttributeUsage(AttributeTargets.Interface)]
public class ContractSchemaFilePathAttribute(string path) : Attribute
{
    public string Path { get; } = path;
}
