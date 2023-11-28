using Cosm.Net.Client;
using Cosm.Net.Services;

namespace Cosm.Net.Extensions;
public static class CosmTxClientBuilderExtensions
{
    public static CosmTxClientBuilder WithConstantGasPrice(this CosmTxClientBuilder builder, string feeDenom, decimal gasPrice)
         => builder
            .WithGasFeeProvider<ConstantGasFeeProvider, ConstantGasFeeProvider.Configuration>(
                new ConstantGasFeeProvider.Configuration(feeDenom, gasPrice));
}
