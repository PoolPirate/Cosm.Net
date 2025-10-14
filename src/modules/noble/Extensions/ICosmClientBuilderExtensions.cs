using Cosm.Net.Client;
using Cosm.Net.Services;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallNoble(this CosmClientBuilder builder, string bech32Prefix = "noble")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(60))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
}
