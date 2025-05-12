namespace Cosm.Net.Models;
public interface IWasmContract
{
    public string ContractAddress { get; }
    public string? CodeHash { get; }
}