using Cosm.Net.Adapters;
using Cosm.Net.Client;
using Cosm.Net.CosmosHub.Modules;
using Cosm.Net.Extensions;
using System.Reflection;

namespace Cosm.Net.CosmosHub.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallCosmosHub(this CosmClientBuilder builder, string bech32Prefix = "cosmos")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix)
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
            .AsInternal().RegisterModule<IAuthModuleAdapter, AuthModule>()
            .AsInternal().RegisterModule<ITendermintModuleAdapter, TendermintModule>()
            .AsInternal().RegisterModule<ITxModuleAdapter, TxModule>();
}
