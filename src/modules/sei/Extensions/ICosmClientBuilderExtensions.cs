using Cosm.Net.Adapters;
using Cosm.Net.Client;
using Cosm.Net.Extensions;
using Cosm.Net.Sei.Modules;
using System.Reflection;

namespace Cosm.Net.Sei.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallSei(this CosmClientBuilder builder, string bech32Prefix = "sei")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix)
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
            .AsInternal().RegisterModule<IWasmAdapater, WasmModule>()
            .AsInternal().RegisterModule<IAuthModuleAdapter, AuthModule>()
            .AsInternal().RegisterModule<ITendermintModuleAdapter, TendermintModule>()
            .AsInternal().RegisterModule<ITxModuleAdapter, TxModule>();
}
