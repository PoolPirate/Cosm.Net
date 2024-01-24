using Cosm.Net.Models;

namespace Cosm.Net.Configuration;
public interface IWasmConfiguration
{
    public IWasmConfiguration RegisterContractSchema<TContract>()
        where TContract : IContract;
}
