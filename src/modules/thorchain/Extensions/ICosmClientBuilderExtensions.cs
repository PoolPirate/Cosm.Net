using Cosm.Net.Client;
using Cosm.Net.Models;
using Cosm.Net.Services;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallThorchain(this CosmClientBuilder builder, string bech32Prefix = "thor", Coin? gasFee = null)
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(40))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
            .AsInternal().WithGasFeeProvider<ConstantGasFeeProvider, ConstantGasFeeProvider.Configuration>(
                new ConstantGasFeeProvider.Configuration(gasFee ?? new Coin("rune", 2000000))
            );
}
