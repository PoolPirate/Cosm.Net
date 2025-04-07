using Cosm.Net.Adapters;
using Cosm.Net.Client;
using Cosm.Net.Modules;
using Injective.Types.V1Beta1;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallInjective(this CosmClientBuilder builder, string bech32Prefix = "inj")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(40))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
            .AsInternal().WithAccountType<EthAccount>(
                EthAccount.Descriptor, 
                x => new Models.AccountData(x.BaseAccount.AccountNumber, x.BaseAccount.Sequence)
            );
}
