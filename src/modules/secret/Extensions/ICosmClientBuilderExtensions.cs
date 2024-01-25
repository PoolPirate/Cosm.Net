using Cosm.Net.Client;
using Cosm.Net.Adapters;
using Cosm.Net.Secret.Modules;
using System.Reflection;
using Cosm.Net.Extensions;

namespace Cosm.Net.Secret.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallSecret(this CosmClientBuilder builder, string bech32Prefix = "secret") 
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix)
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
            //.AsInternal().RegisterModule<IWasmAdapater, WasmModule>()
            .AsInternal().RegisterModule<IAuthModuleAdapter, AuthModule>()
            .AsInternal().RegisterModule<ITendermintModuleAdapter, TendermintModule>()
            .AsInternal().RegisterModule<ITxModuleAdapter, TxModule>();
}
