using Cosm.Net.Client;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallOrai(this CosmClientBuilder builder, string bech32Prefix = "orai")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(50))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
}
