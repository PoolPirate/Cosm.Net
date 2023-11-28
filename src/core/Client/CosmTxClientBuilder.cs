using Cosm.Net.Services;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cosm.Net.Client;
public class CosmTxClientBuilder
{
    private readonly ServiceCollection _services = [];

    private string? _bech32Prefix = null;
    private ICosmClient? _cosmClient = null;
    private IOfflineSigner? _signer = null;

    public CosmTxClientBuilder WithBech32Prefix(string bech32Prefix, bool overrideExisting = false)
    {
        if(_bech32Prefix is not null && !overrideExisting)
        {
            throw new InvalidOperationException("Bech32Prefix already set");
        }

        _bech32Prefix = bech32Prefix;
        return this;
    }

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
        if (_signer is not null && !overrideExisting)
        {
            throw new InvalidOperationException("Signer already set!");
        }

        _signer = signer;
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
    public CosmTxClientBuilder WithTxEncoder<TTxEncoder, TConfiguration>(TConfiguration configuration)
        where TTxEncoder : class, ITxEncoder<TConfiguration>
        where TConfiguration : class
    {
        if(_services.Any(x => x.ServiceType == typeof(TConfiguration)))
        {
            throw new InvalidOperationException("Configuration type has already been registered");
        }
        if(_services.Any(x => x.ServiceType == typeof(ITxEncoder)))
        {
            throw new InvalidOperationException("ITxEncoder already registered");
        }

        _services.AddSingleton<ITxEncoder, TTxEncoder>();
        _services.AddSingleton(configuration);
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
    public CosmTxClientBuilder WithTxPublisher<TTxPublisher, TConfiguration>(TConfiguration configuration)
        where TTxPublisher : class, ITxPublisher<TConfiguration>
        where TConfiguration : class
    {
        if(_services.Any(x => x.ServiceType == typeof(TConfiguration)))
        {
            throw new InvalidOperationException("Configuration type has already been registered");
        }
        if(_services.Any(x => x.ServiceType == typeof(ITxPublisher)))
        {
            throw new InvalidOperationException("ITxPublisher already registered");
        }

        _services.AddSingleton<ITxPublisher, TTxPublisher>();
        _services.AddSingleton(configuration);
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
    public CosmTxClientBuilder WithTxScheduler<TTxScheduler, TConfiguration>(TConfiguration configuration)
        where TTxScheduler : class, ITxScheduler<TConfiguration>
        where TConfiguration : class
    {
        if(_services.Any(x => x.ServiceType == typeof(TConfiguration)))
        {
            throw new InvalidOperationException("Configuration type has already been registered");
        }
        if(_services.Any(x => x.ServiceType == typeof(ITxScheduler)))
        {
            throw new InvalidOperationException("ITxScheduler already registered");
        }

        _services.AddSingleton<ITxScheduler, TTxScheduler>();
        _services.AddSingleton(configuration);
        return this;
    }

    public CosmTxClientBuilder WithAccountDataProvider<TAccountDataProvider>(bool overrideExisting = false)
        where TAccountDataProvider : class, IChainDataProvider
    {
        if(_services.Any(x => x.ServiceType == typeof(IChainDataProvider)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("IChainDataProvider already registered");
            }

            _services.Replace(new ServiceDescriptor(typeof(IChainDataProvider), typeof(TAccountDataProvider), ServiceLifetime.Singleton));
        }
        else
        {
            _services.AddSingleton<IChainDataProvider, TAccountDataProvider>();
        }

        return this;
    }
    public CosmTxClientBuilder WithAccountDataProvider<TAccountDataProvider, TConfiguration>(TConfiguration configuration)
        where TAccountDataProvider : class, IChainDataProvider<TConfiguration>
        where TConfiguration : class
    {
        if(_services.Any(x => x.ServiceType == typeof(TConfiguration)))
        {
            throw new InvalidOperationException("Configuration type has already been registered");
        }
        if(_services.Any(x => x.ServiceType == typeof(IChainDataProvider)))
        {
            throw new InvalidOperationException("IChainDataProvider already registered");
        }

        _services.AddSingleton<IChainDataProvider, TAccountDataProvider>();
        _services.AddSingleton(configuration);
        return this;
    }

    public CosmTxClientBuilder WithGasFeeProvider<TGasFeeProvider>(bool overrideExisting = false)
        where TGasFeeProvider : class, IGasFeeProvider
    {
        if(_services.Any(x => x.ServiceType == typeof(IGasFeeProvider)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("IGasFeeProvider already registered");
            }

            _services.Replace(new ServiceDescriptor(typeof(IGasFeeProvider), typeof(TGasFeeProvider), ServiceLifetime.Singleton));
        }
        else
        {
            _services.AddSingleton<IGasFeeProvider, TGasFeeProvider>();
        }

        return this;
    }
    public CosmTxClientBuilder WithGasFeeProvider<TGasFeeProvider, TConfiguration>(TConfiguration configuration)
        where TGasFeeProvider : class, IGasFeeProvider<TConfiguration>
        where TConfiguration : class
    {
        if (_services.Any(x => x.ServiceType == typeof(TConfiguration)))
        {
            throw new InvalidOperationException("Configuration type has already been registered");
        }
        if(_services.Any(x => x.ServiceType == typeof(IGasFeeProvider)))
        {
            throw new InvalidOperationException("IGasFeeProvider already registered");
        }

        _services.AddSingleton<IGasFeeProvider, TGasFeeProvider>();
        _services.AddSingleton(configuration);
        return this;
    }

    public async Task<ICosmTxClient> BuildAsync()
    {
        if (_bech32Prefix is null)
        {
            throw new InvalidOperationException("Missing Bech32Prefix");
        }
        if (_cosmClient is null) 
        {
            throw new InvalidOperationException("Missing CosmClient");
        }
        if(_signer is null)
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
        if(!_services.Any(x => x.ServiceType == typeof(IGasFeeProvider)))
        {
            throw new InvalidOperationException("Missing IGasFeeProvider");
        }

        foreach(var (type, module) in _cosmClient.AsInternal().GetAllModules())
        {
            _services.AddSingleton(type, module);   
        }

        ITxChainConfiguration chainConfig = new TxChainConfiguration();

        _services.AddSingleton(chainConfig);
        _services.AddSingleton(_cosmClient);
        _services.AddSingleton(_signer);

        var provider = _services.BuildServiceProvider();

        var dataProvider = provider.GetRequiredService<IChainDataProvider>();
        string chainId = await dataProvider.GetChainIdAsync();

        chainConfig.Initialize(chainId, _bech32Prefix);
  
        var txScheduler = provider.GetRequiredService<ITxScheduler>();
        await txScheduler.InitializeAsync();

        var gasFeeProvider = provider.GetRequiredService<IGasFeeProvider>();

        return new CosmTxClient(provider, _cosmClient, gasFeeProvider, chainConfig);
    }
}
