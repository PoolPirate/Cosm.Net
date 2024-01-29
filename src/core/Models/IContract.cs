namespace Cosm.Net.Models;
public interface IContract
{
    public string ContractAddress { get; }
    public string? CodeHash { get; }
}