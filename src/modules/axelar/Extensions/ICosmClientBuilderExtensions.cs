using Cosm.Net.Adapters;
using Cosm.Net.Client;
using Cosm.Net.Modules;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallAxelar(this CosmClientBuilder builder, string bech32Prefix = "axelar")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix)
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
            .AsInternal().RegisterModule<IWasmAdapater, WasmModule>()
            .AsInternal().RegisterModule<IAuthModuleAdapter, AuthModule>()
            .AsInternal().RegisterModule<ITendermintModuleAdapter, TendermintModule>()
            .AsInternal().RegisterModule<ITxModuleAdapter, TxModule>();
}
