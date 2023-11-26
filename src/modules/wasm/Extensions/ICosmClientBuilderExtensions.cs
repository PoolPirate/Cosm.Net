
using Cosm.Net.Client;
using Cosm.Net.Client.Internal;
using Cosm.Net.Wasm.Models;
using Cosm.Net.Wasm.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Wasm.Extensions;
public static class ICosmClientBuilderExtensions
{
    private static readonly ContractSchemaStore _contractSchemaStore = new ContractSchemaStore();

    public static CosmClientBuilder AddWasmd(this CosmClientBuilder builder)
    {
        builder.RegisterModule<IWasmModule, WasmModule>();

        ((IInternalCosmClientBuilder) builder).ServiceCollection.AddSingleton(provider =>
        {
            _contractSchemaStore.InitProvider(provider);
            return _contractSchemaStore;
        });

        return builder;
    }

    public static CosmClientBuilder RegisterContractSchema<TContract>(this CosmClientBuilder builder)
        where TContract : IContract
    {
        _contractSchemaStore.RegisterContractSchema<TContract>();
        return builder;
    }
}
