
using Cosm.Net.Client;

namespace Cosm.Net.Wasm.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder AddWasmd(this CosmClientBuilder builder)
    {
        builder.RegisterModule<IWasmModule, WasmModule>();

        return builder;
    }
}
