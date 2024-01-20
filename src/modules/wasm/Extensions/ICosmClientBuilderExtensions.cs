
using Cosm.Net.Client;
using Cosm.Net.Adapters;
using Cosm.Net.Wasm.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Wasm.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder AddWasmd(this CosmClientBuilder builder, Action<IWasmConfiguration>? wasmConfigAction = null)
    {
        if (!builder.AsInternal().HasModule<IWasmAdapater>())
        {
            throw new InvalidOperationException($"No {nameof(IWasmAdapater)} set. Make sure to install a chain that supports wasmd before calling {nameof(AddWasmd)}");
        }

        var config = new WasmConfiguration();
        wasmConfigAction?.Invoke(config);

        builder.AsInternal().ServiceCollection.AddSingleton(provider =>
        {
            var schemaStore = config.GetSchemaStore();
            schemaStore.InitProvider(provider);
            return schemaStore;
        });

        return builder;
    }
}
