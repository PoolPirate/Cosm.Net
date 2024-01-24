using Cosm.Net.Models;
using Cosm.Net.Services;

namespace Cosm.Net.Configuration;
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
