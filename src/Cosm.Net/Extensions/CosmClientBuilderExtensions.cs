using Cosm.Net.Adapters.Internal;
using Cosm.Net.Client;
using Cosm.Net.Client.Internal;
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
            .AsInternal().WithTxPublisher<TxModulePublisher>()
            .AsInternal().WithTxConfirmer<PollingTxConfirmer>();

    public static CosmClientBuilder AddWasmd(this CosmClientBuilder builder)
    {
        if(!builder.AsInternal().HasModule<IInternalWasmAdapter>())
        {
            throw new InvalidOperationException($"No {nameof(IInternalWasmAdapter)} set. Make sure to install a chain that supports wasmd before calling {nameof(AddWasmd)}");
        }

        _ = builder.AsInternal().ServiceCollection.AddSingleton<IContractFactory>(
            provider => new ContractFactory(provider.GetRequiredService<IInternalWasmAdapter>()));

        return builder;
    }
}
