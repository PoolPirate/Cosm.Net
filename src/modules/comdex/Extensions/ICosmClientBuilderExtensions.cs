using Cosm.Net.Client;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallComdex(this CosmClientBuilder builder, string bech32Prefix = "comdex")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(80))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
}
