using Cosm.Net.Client;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallSaga(this CosmClientBuilder builder, string bech32Prefix = "saga")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(40))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
}
