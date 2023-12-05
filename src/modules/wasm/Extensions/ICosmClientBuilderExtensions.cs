
using Cosm.Net.Client;
using Cosm.Net.Client.Internal;
using Cosm.Net.Wasm.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Wasm.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder AddWasmd(this CosmClientBuilder builder, Action<IWasmConfiguration>? wasmConfigAction = null)
    {
        builder.RegisterModule<IWasmModule, WasmModule>();

        var config = new WasmConfiguration();
        wasmConfigAction?.Invoke(config);

        ((IInternalCosmClientBuilder) builder).ServiceCollection.AddSingleton(provider =>
        {
            var schemaStore = config.GetSchemaStore();
            schemaStore.InitProvider(provider);
            return schemaStore;
        });

        return builder;
    }
}
