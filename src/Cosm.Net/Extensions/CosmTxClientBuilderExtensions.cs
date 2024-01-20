using Cosm.Net.Client;
using Cosm.Net.Client.Internal;
using Cosm.Net.CosmosSdk.Services;
using Cosm.Net.Services;

namespace Cosm.Net.Extensions;
public static class CosmTxClientBuilderExtensions
{
    public static CosmClientBuilder WithConstantGasPrice(this CosmClientBuilder builder, string feeDenom, decimal gasPrice)
        => builder
            .AsInternal().WithGasFeeProvider<ConstantGasFeeProvider, ConstantGasFeeProvider.Configuration>(
                new ConstantGasFeeProvider.Configuration(feeDenom, gasPrice));

    public static CosmClientBuilder UseCosmosTxStructure(this IInternalCosmClientBuilder builder) 
        => builder
            .WithChainDataProvider<CosmosChainDataProvider>()
            .AsInternal().WithTxEncoder<CosmosTxEncoder>()
            .AsInternal().WithTxPublisher<TxModulePublisher>();
}
