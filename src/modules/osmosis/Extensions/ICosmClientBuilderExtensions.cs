using Cosm.Net.Modules;
using Cosm.Net.Services;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallOsmosis(this CosmClientBuilder builder, string bech32Prefix = "osmo")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix)
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
            .AsInternal().RegisterModule<IWasmAdapater, WasmModule>()
            .AsInternal().RegisterModule<IAuthModuleAdapter, AuthModule>()
            .AsInternal().RegisterModule<ITendermintModuleAdapter, TendermintModule>()
            .AsInternal().RegisterModule<ITxModuleAdapter, TxModule>();

    public static CosmClientBuilder WithEIP1559MempoolGasPrice(this CosmClientBuilder builder,
        string feeDenom = "uosmo", decimal gasPriceOffset = 0, int cacheSeconds = 5)
         => builder
            .AsInternal().WithGasFeeProvider<EIP1159MempoolGasFeeProvider, EIP1159MempoolGasFeeProvider.Configuration>(
                new EIP1159MempoolGasFeeProvider.Configuration(feeDenom, gasPriceOffset, cacheSeconds));
}
