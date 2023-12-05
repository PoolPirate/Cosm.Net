using Cosm.Net.Wasm.Models;

namespace Cosm.Net.Wasm.Configuration;
public interface IWasmConfiguration
{
    public IWasmConfiguration RegisterContractSchema<TContract>()
        where TContract : IContract;
}
