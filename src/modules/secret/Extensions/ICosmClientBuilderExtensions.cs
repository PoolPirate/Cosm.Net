using Cosm.Net.Adapters;
using Cosm.Net.Client;
using Cosm.Net.Extensions;
using Cosm.Net.Secret.Modules;
using Cosm.Net.Secret.Services;
using Cosm.Net.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Cosm.Net.Secret.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallSecret(this CosmClientBuilder builder, string bech32Prefix = "secret", byte[]? encryptionSeed = null)
    {
        builder.AsInternal().ServiceCollection.AddSingleton(
            provider => new SecretMessageEncryptor(provider.GetRequiredService<IRegistrationModule>(), encryptionSeed));
        builder.AsInternal().ServiceCollection.AddSingleton<IInitializeableService, SecretMessageEncryptor>(
            provider => provider.GetRequiredService<SecretMessageEncryptor>());

        return builder
                .AsInternal().UseCosmosTxStructure()
                .AsInternal().WithChainInfo(bech32Prefix)
                .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly())
                .AsInternal().RegisterModule<IWasmAdapater, ComputeModule>()
                .AsInternal().RegisterModule<IAuthModuleAdapter, AuthModule>()
                .AsInternal().RegisterModule<ITendermintModuleAdapter, TendermintModule>()
                .AsInternal().RegisterModule<ITxModuleAdapter, TxModule>();
    }
}
