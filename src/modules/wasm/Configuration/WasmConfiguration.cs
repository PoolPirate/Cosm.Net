using Cosm.Net.Wasm.Models;
using Cosm.Net.Wasm.Services;

namespace Cosm.Net.Wasm.Configuration;
internal class WasmConfiguration : IWasmConfiguration
{
    private readonly ContractSchemaStore _contractSchemaStore = new ContractSchemaStore();

    public IWasmConfiguration RegisterContractSchema<TContract>() 
        where TContract : IContract
    {
        _contractSchemaStore.RegisterContractSchema<TContract>();
        return this;
    }

    public ContractSchemaStore GetSchemaStore() 
        => _contractSchemaStore;
}
