using Cosm.Net.Client;
using Eth.Types.V1;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallNibiru(this CosmClientBuilder builder, string bech32Prefix = "nibi")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(40))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
            .AsInternal().WithAccountType<EthAccount>(
                EthAccount.Descriptor,
                x => new Models.AccountData(x.BaseAccount.AccountNumber, x.BaseAccount.Sequence)
            );
}
