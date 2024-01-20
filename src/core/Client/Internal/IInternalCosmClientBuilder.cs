using Cosm.Net.Modules;
using Cosm.Net.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Cosm.Net.Client.Internal;
public interface IInternalCosmClientBuilder
{
    public IServiceCollection ServiceCollection { get; }

    public CosmClientBuilder WithChainInfo(string bech32Prefix);

    public CosmClientBuilder RegisterModule<TIModule, TModule>()
        where TModule : class, IModule, TIModule
        where TIModule : class, IModule;
    public CosmClientBuilder RegisterModulesFromAssembly(Assembly assembly);
    public bool HasModule<TIModule>();

    public CosmClientBuilder WithGasFeeProvider<TGasFeeProvider, TConfiguration>(TConfiguration configuration, bool overrideExisting = false)
        where TGasFeeProvider : class, IGasFeeProvider<TConfiguration>
        where TConfiguration : class;
    public CosmClientBuilder WithGasFeeProvider<TGasFeeProvider>(bool overrideExisting = false)
        where TGasFeeProvider : class, IGasFeeProvider;
    public CosmClientBuilder WithChainDataProvider<TAccountDataProvider, TConfiguration>(TConfiguration configuration, bool overrideExisting = false)
        where TAccountDataProvider : class, IChainDataProvider<TConfiguration>
        where TConfiguration : class;
    public CosmClientBuilder WithChainDataProvider<TAccountDataProvider>(bool overrideExisting = false)
        where TAccountDataProvider : class, IChainDataProvider;
    public CosmClientBuilder WithTxPublisher<TTxPublisher, TConfiguration>(TConfiguration configuration, bool overrideExisting = false)
        where TTxPublisher : class, ITxPublisher<TConfiguration>
        where TConfiguration : class;
    public CosmClientBuilder WithTxPublisher<TTxPublisher>(bool overrideExisting = false)
        where TTxPublisher : class, ITxPublisher;
    public CosmClientBuilder WithTxEncoder<TTxEncoder, TConfiguration>(TConfiguration configuration, bool overrideExisting = false)
        where TTxEncoder : class, ITxEncoder<TConfiguration>
        where TConfiguration : class;
    public CosmClientBuilder WithTxEncoder<TTxEncoder>(bool overrideExisting = false)
        where TTxEncoder : class, ITxEncoder;
}
