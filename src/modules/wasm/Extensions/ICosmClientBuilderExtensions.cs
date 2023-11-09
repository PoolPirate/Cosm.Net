using Cosm.Net.Base;

namespace Cosm.Net.Wasm.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static TCosmClientBuilder AddWasmd<TCosmClientBuilder>(this ICosmClientBuilder<TCosmClientBuilder> builder)
    where TCosmClientBuilder : class
    {
        builder.RegisterModule<WasmModule>();

        return (TCosmClientBuilder) builder;
    }
}
