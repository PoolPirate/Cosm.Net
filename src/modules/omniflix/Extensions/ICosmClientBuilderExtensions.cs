using Cosm.Net.Client;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallOmniflix(this CosmClientBuilder builder, string bech32Prefix = "omniflix")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(50))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
}
