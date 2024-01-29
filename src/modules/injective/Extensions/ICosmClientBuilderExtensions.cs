using Cosm.Net.Adapters;
using Cosm.Net.Client;
using Cosm.Net.Extensions;
using Cosm.Net.Injective.Modules;
using System.Reflection;

namespace Cosm.Net.Injective.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallInjective(this CosmClientBuilder builder, string bech32Prefix = "inj")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix)
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
            .AsInternal().RegisterModule<IWasmAdapater, WasmModule>()
            .AsInternal().RegisterModule<IAuthModuleAdapter, AuthModule>()
            .AsInternal().RegisterModule<ITendermintModuleAdapter, TendermintModule>()
            .AsInternal().RegisterModule<ITxModuleAdapter, TxModule>();
}
