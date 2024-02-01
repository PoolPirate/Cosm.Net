using Cosm.Net.Adapters;
using Cosm.Net.Client;
using Cosm.Net.Extensions;
using Cosm.Net.Noble.Modules;
using System.Reflection;

namespace Cosm.Net.Noble.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallNoble(this CosmClientBuilder builder, string bech32Prefix = "noble")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix)
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
            .AsInternal().RegisterModule<IAuthModuleAdapter, AuthModule>()
            .AsInternal().RegisterModule<ITendermintModuleAdapter, TendermintModule>()
            .AsInternal().RegisterModule<ITxModuleAdapter, TxModule>();
}
