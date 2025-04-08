using Cosm.Net.Client;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallJackal(this CosmClientBuilder builder, string bech32Prefix = "jkl")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(360))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
}
