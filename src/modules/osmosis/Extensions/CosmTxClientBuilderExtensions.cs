using Cosm.Net.Client;
using Cosm.Net.Osmosis.Services;

namespace Cosm.Net.Osmosis.Extensions;
public static class CosmTxClientBuilderExtensions
{
    public static CosmTxClientBuilder WithEIP1559MempoolGasPrice(this CosmTxClientBuilder builder, 
        string feeDenom, decimal gasPriceOffset = 0, int cacheSeconds = 5)
         => builder
            .WithGasFeeProvider<EIP1159MempoolGasFeeProvider, EIP1159MempoolGasFeeProvider.Configuration>(
                new EIP1159MempoolGasFeeProvider.Configuration(feeDenom, gasPriceOffset, cacheSeconds));
}
