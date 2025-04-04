using Cosm.Net.Client;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallAgoric(this CosmClientBuilder builder, string bech32Prefix = "agoric")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(120))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
}
