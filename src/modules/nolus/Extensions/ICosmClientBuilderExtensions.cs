using Cosm.Net.Client;
using Cosm.Net.Services;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallNolus(this CosmClientBuilder builder, string bech32Prefix = "nolus")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(50))
            .AsInternal().WithTxEncoder<NolusTxEncoder>(true)
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
}
