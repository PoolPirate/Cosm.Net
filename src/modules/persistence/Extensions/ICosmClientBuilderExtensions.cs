using Cosm.Net.Adapters;
using Cosm.Net.Client;
using Cosm.Net.Modules;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallPersistence(this CosmClientBuilder builder, string bech32Prefix = "persistence")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(60))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
}
