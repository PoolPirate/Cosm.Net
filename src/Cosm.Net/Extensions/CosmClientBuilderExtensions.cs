using Cosm.Net.Adapters;
using Cosm.Net.Client;
using Cosm.Net.Client.Internal;
using Cosm.Net.Configuration;
using Cosm.Net.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Extensions;
public static class CosmTxClientBuilderExtensions
{
    public static CosmClientBuilder WithConstantGasPrice(this CosmClientBuilder builder, string feeDenom, decimal gasPrice)
        => builder
            .AsInternal().WithGasFeeProvider<ConstantGasFeeProvider, ConstantGasFeeProvider.Configuration>(
                new ConstantGasFeeProvider.Configuration(feeDenom, gasPrice));

    public static CosmClientBuilder UseCosmosTxStructure(this IInternalCosmClientBuilder builder)
        => builder
            .WithTxEncoder<CosmosTxEncoder>()
            .AsInternal().WithTxPublisher<TxModulePublisher>();

    public static CosmClientBuilder AddWasmd(this CosmClientBuilder builder, Action<IWasmConfiguration>? wasmConfigAction = null)
    {
        if(!builder.AsInternal().HasModule<IWasmAdapater>())
        {
            throw new InvalidOperationException($"No {nameof(IWasmAdapater)} set. Make sure to install a chain that supports wasmd before calling {nameof(AddWasmd)}");
        }

        var config = new WasmConfiguration();
        wasmConfigAction?.Invoke(config);

        _ = builder.AsInternal().ServiceCollection.AddSingleton(provider =>
        {
            var schemaStore = config.GetSchemaStore();
            schemaStore.InitProvider(provider);
            return schemaStore;
        });

        return builder;
    }
}
