using Cosm.Net.Services;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cosm.Net.Client;
public class CosmTxClientBuilder
{
    private readonly ServiceCollection _services = new ServiceCollection();
    private ICosmClient? _cosmClient;
    private readonly TxUserChainConfiguration _userChainConfiguration = new TxUserChainConfiguration();

    public CosmTxClientBuilder WithCosmClient(ICosmClient client, bool overrideExisting = false)
    {
        if (_services.Any(x => x.ServiceType == typeof(ICosmClient)))
        {
            if (!overrideExisting)
            {
                throw new InvalidOperationException("CosmClient already registered");
            }

            _services.Replace(new ServiceDescriptor(typeof(ICosmClient), client));
            _cosmClient = client;
        }
        else
        {
            _services.AddSingleton(client);
            _cosmClient = client;
        }

        return this;
    }

    public CosmTxClientBuilder WithSigner(IOfflineSigner signer, bool overrideExisting = false)
    {
        if(_services.Any(x => x.ServiceType == typeof(IOfflineSigner)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("IOfflineSigner already registered");
            }

            _services.Replace(new ServiceDescriptor(typeof(IOfflineSigner), signer));
        }
        else
        {
            _services.AddSingleton(signer);
        }

        return this;
    }

    public CosmTxClientBuilder WithTxEncoder<TTxEncoder>(bool overrideExisting = false)
        where TTxEncoder : class, ITxEncoder
    {
        if(_services.Any(x => x.ServiceType == typeof(ITxEncoder)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("ITxEncoder already registered");
            }

            _services.Replace(new ServiceDescriptor(typeof(ITxEncoder), typeof(TTxEncoder), ServiceLifetime.Singleton));
        }
        else
        {
            _services.AddSingleton<ITxEncoder, TTxEncoder>();
        }

        return this;
    }

    public CosmTxClientBuilder WithTxPublisher<TTxPublisher>(bool overrideExisting = false)
    where TTxPublisher : class, ITxPublisher
    {
        if(_services.Any(x => x.ServiceType == typeof(ITxPublisher)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("ITxPublisher already registered");
            }

            _services.Replace(new ServiceDescriptor(typeof(ITxPublisher), typeof(ITxPublisher), ServiceLifetime.Singleton));
        }
        else
        {
            _services.AddSingleton<ITxPublisher, TTxPublisher>();
        }

        return this;
    }

    public CosmTxClientBuilder WithTxScheduler<TTxScheduler>(bool overrideExisting = false)
    where TTxScheduler : class, ITxScheduler
    {
        if(_services.Any(x => x.ServiceType == typeof(ITxScheduler)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("ITxScheduler already registered");
            }

            _services.Replace(new ServiceDescriptor(typeof(ITxScheduler), typeof(TTxScheduler), ServiceLifetime.Singleton));
        }
        else
        {
            _services.AddSingleton<ITxScheduler, TTxScheduler>();
        }

        return this;
    }

    public CosmTxClientBuilder WithAccountDataProvider<TAccountDataProvider>(bool overrideExisting = false)
        where TAccountDataProvider : class, IChainDataProvider
    {
        if(_services.Any(x => x.ServiceType == typeof(IChainDataProvider)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("IAccountDataProvider already registered");
            }

            _services.Replace(new ServiceDescriptor(typeof(IChainDataProvider), typeof(TAccountDataProvider), ServiceLifetime.Singleton));
        }
        else
        {
            _services.AddSingleton<IChainDataProvider, TAccountDataProvider>();
        }

        return this;
    }

    public CosmTxClientBuilder WithTxChainConfiguration(Action<TxUserChainConfiguration> action)
    {
        action.Invoke(_userChainConfiguration);
        return this;
    }

    public async Task<CosmTxClient> BuildAsync()
    {
        if (!_services.Any(x => x.ServiceType == typeof(ICosmClient)) || _cosmClient is null) 
        {
            throw new InvalidOperationException("Missing CosmClient");
        }
        if(!_services.Any(x => x.ServiceType == typeof(IOfflineSigner)))
        {
            throw new InvalidOperationException("Missing Signer");
        }
        if(!_services.Any(x => x.ServiceType == typeof(ITxEncoder)))
        {
            throw new InvalidOperationException("Missing ITxEncoder");
        }
        if(!_services.Any(x => x.ServiceType == typeof(ITxPublisher)))
        {
            throw new InvalidOperationException("Missing ITxPublisher");
        }
        if(!_services.Any(x => x.ServiceType == typeof(ITxScheduler)))
        {
            throw new InvalidOperationException("Missing ITxScheduler");
        }
        if(!_services.Any(x => x.ServiceType == typeof(IChainDataProvider)))
        {
            throw new InvalidOperationException("Missing IAccountDataProvider");
        }

        foreach(var (type, module) in _cosmClient.GetAllModules())
        {
            _services.AddSingleton(type, module);   
        }

        var setupProvider = _services.BuildServiceProvider();

        var dataProvider = setupProvider.GetRequiredService<IChainDataProvider>();
        var chainId = await dataProvider.GetChainIdAsync();

        var chainConfig = new TxChainConfiguration(chainId, _userChainConfiguration.Prefix, 
            _userChainConfiguration.FeeDenom, _userChainConfiguration.GasPrice);

        _services.AddSingleton<ITxChainConfiguration>(chainConfig);

        var provider = _services.BuildServiceProvider();

        var txScheduler = provider.GetRequiredService<ITxScheduler>();
        await txScheduler.InitializeAsync();

        return new CosmTxClient(provider);
    }
}
