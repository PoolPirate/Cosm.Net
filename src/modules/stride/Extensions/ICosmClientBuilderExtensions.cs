using Cosm.Net.Adapters;
using Cosm.Net.Client;
using Cosm.Net.Modules;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallStride(this CosmClientBuilder builder, string bech32Prefix = "stride")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix)
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
            .AsInternal().RegisterModule<IAuthModuleAdapter, AuthModule>()
            .AsInternal().RegisterModule<ITendermintModuleAdapter, TendermintModule>()
            .AsInternal().RegisterModule<ITxModuleAdapter, TxModule>();
}
