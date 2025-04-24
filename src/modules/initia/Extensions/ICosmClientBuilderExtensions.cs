using Cosm.Net.Client;
using Cosm.Net.Services;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallInitia(this CosmClientBuilder builder, string bech32Prefix = "init")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(40))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
            .WithInitiaTxModuleGasPrice();

    public static CosmClientBuilder WithInitiaTxModuleGasPrice(this CosmClientBuilder builder,
        string feeDenom = "uinit", decimal gasPriceOffset = 0, int cacheSeconds = 5)
         => builder
            .AsInternal().WithGasFeeProvider<InitiaTxModuleGasFeeProvider, InitiaTxModuleGasFeeProvider.Configuration>(
                new InitiaTxModuleGasFeeProvider.Configuration(feeDenom, gasPriceOffset, cacheSeconds));
}
