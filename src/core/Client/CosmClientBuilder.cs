using Cosm.Net.Adapters;
using Cosm.Net.Client.Internal;
using Cosm.Net.Services;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Cosm.Net.Client;
public sealed class CosmClientBuilder : IInternalCosmClientBuilder
{
    private readonly ServiceCollection _services = [];
    private readonly List<Type> _moduleTypes = [];
    private ChainInfo? _chainInfo = null;

    IServiceCollection IInternalCosmClientBuilder.ServiceCollection
        => _services;
    public IInternalCosmClientBuilder AsInternal()
        => this;

    public CosmClientBuilder WithChannel(GrpcChannel channel)
    {
        if(_services.Any(x => x.ServiceType == typeof(GrpcChannel)))
        {
            throw new InvalidOperationException($"{nameof(GrpcChannel)} already set.");
        }

        _ = _services.AddSingleton(channel);
        return this;
    }

    CosmClientBuilder IInternalCosmClientBuilder.WithChainInfo(string bech32Prefix)
    {
        if(_chainInfo is not null)
        {
            throw new InvalidOperationException($"{nameof(ChainInfo)} already set.");
        }

        _chainInfo = new ChainInfo(bech32Prefix);
        return this;
    }

    CosmClientBuilder IInternalCosmClientBuilder.RegisterModule<TIModule, TModule>()
    {
        if(!_services.Any(x => x.ServiceType == typeof(TModule)))
        {
            _ = _services.AddSingleton<TIModule, TModule>();
            _moduleTypes.Add(typeof(TIModule));
        }

        return this;
    }

    CosmClientBuilder IInternalCosmClientBuilder.RegisterModulesFromAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes();
        var moduleTypes = assembly.GetTypes()
            .Where(x => x.IsClass)
            .Select(x => new
            {
                ModuleType = x,
                InterfaceType = GetModuleInterfaceFromModule(x)
            })
            .Where(x => x.InterfaceType is not null);

        var registerMethod = typeof(IInternalCosmClientBuilder).GetMethod(nameof(IInternalCosmClientBuilder.RegisterModule),
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        foreach(var modTypes in moduleTypes)
        {
            _ = registerMethod.MakeGenericMethod(modTypes.InterfaceType, modTypes.ModuleType).Invoke(this, []);
        }

        return this;

        Type GetModuleInterfaceFromModule(Type moduleType)
            => types
                .Where(x => x.IsInterface)
                .Where(x => x.IsAssignableFrom(moduleType))
                .Where(x => x.Name.AsSpan().Slice(1).Equals(moduleType.Name.AsSpan(), StringComparison.Ordinal))
                .SingleOrDefault();
    }

    bool IInternalCosmClientBuilder.HasModule<TIModule>()
       => _services.Any(x => x.ServiceType == typeof(TIModule));

    public CosmClientBuilder WithSigner(IOfflineSigner signer, bool overrideExisting = false)
    {
        if(_services.Any(x => x.ServiceType == typeof(IOfflineSigner)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException($"{nameof(IOfflineSigner)} already set.");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(IOfflineSigner), signer));
        }
        else
        {
            _ = _services.AddSingleton(signer);
        }

        return this;
    }

    public CosmClientBuilder WithTxScheduler<TTxScheduler>(bool overrideExisting = false)
       where TTxScheduler : class, ITxScheduler
    {
        if(_services.Any(x => x.ServiceType == typeof(ITxScheduler)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("ITxScheduler already registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(ITxScheduler), typeof(TTxScheduler), ServiceLifetime.Singleton));
        }
        else
        {
            _ = _services.AddSingleton<ITxScheduler, TTxScheduler>();
        }

        return this;
    }
    public CosmClientBuilder WithTxScheduler<TTxScheduler, TConfiguration>(TConfiguration configuration, bool overrideExisting)
        where TTxScheduler : class, ITxScheduler<TConfiguration>
        where TConfiguration : class
    {
        if(_services.Any(x => x.ServiceType == typeof(TConfiguration)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("Configuration type has already been registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(TConfiguration), configuration));
        }
        else
        {
            _ = _services.AddSingleton(configuration);
        }

        if(_services.Any(x => x.ServiceType == typeof(ITxScheduler)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException($"{nameof(ITxScheduler)} already registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(ITxScheduler), typeof(TTxScheduler), ServiceLifetime.Singleton));
        }
        else
        {
            _ = _services.AddSingleton<ITxScheduler, TTxScheduler>();
        }

        return this;
    }

    CosmClientBuilder IInternalCosmClientBuilder.WithTxEncoder<TTxEncoder>(bool overrideExisting)
    {
        if(_services.Any(x => x.ServiceType == typeof(ITxEncoder)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("ITxEncoder already registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(ITxEncoder), typeof(TTxEncoder), ServiceLifetime.Singleton));
        }
        else
        {
            _ = _services.AddSingleton<ITxEncoder, TTxEncoder>();
        }

        return this;
    }
    CosmClientBuilder IInternalCosmClientBuilder.WithTxEncoder<TTxEncoder, TConfiguration>(TConfiguration configuration, bool overrideExisting)
    {
        if(_services.Any(x => x.ServiceType == typeof(TConfiguration)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("Configuration type has already been registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(TConfiguration), configuration));
        }
        else
        {
            _ = _services.AddSingleton(configuration);
        }

        if(_services.Any(x => x.ServiceType == typeof(ITxEncoder)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException($"{nameof(ITxEncoder)} already registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(ITxEncoder), typeof(TTxEncoder), ServiceLifetime.Singleton));
        }
        else
        {
            _ = _services.AddSingleton<ITxEncoder, TTxEncoder>();
        }

        return this;
    }

    CosmClientBuilder IInternalCosmClientBuilder.WithTxPublisher<TTxPublisher>(bool overrideExisting)
    {
        if(_services.Any(x => x.ServiceType == typeof(ITxPublisher)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("ITxPublisher already registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(ITxPublisher), typeof(ITxPublisher), ServiceLifetime.Singleton));
        }
        else
        {
            _ = _services.AddSingleton<ITxPublisher, TTxPublisher>();
        }

        return this;
    }
    CosmClientBuilder IInternalCosmClientBuilder.WithTxPublisher<TTxPublisher, TConfiguration>(TConfiguration configuration, bool overrideExisting)
    {
        if(_services.Any(x => x.ServiceType == typeof(TConfiguration)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("Configuration type has already been registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(TConfiguration), configuration));
        }
        else
        {
            _ = _services.AddSingleton(configuration);
        }

        if(_services.Any(x => x.ServiceType == typeof(ITxPublisher)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException($"{nameof(ITxPublisher)} already registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(ITxPublisher), typeof(TTxPublisher), ServiceLifetime.Singleton));
        }
        else
        {
            _ = _services.AddSingleton<ITxPublisher, TTxPublisher>();
        }

        return this;
    }

    CosmClientBuilder IInternalCosmClientBuilder.WithGasFeeProvider<TGasFeeProvider>(bool overrideExisting)
    {
        if(_services.Any(x => x.ServiceType == typeof(IGasFeeProvider)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("IGasFeeProvider already registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(IGasFeeProvider), typeof(TGasFeeProvider), ServiceLifetime.Singleton));
        }
        else
        {
            _ = _services.AddSingleton<IGasFeeProvider, TGasFeeProvider>();
        }

        return this;
    }
    CosmClientBuilder IInternalCosmClientBuilder.WithGasFeeProvider<TGasFeeProvider, TConfiguration>(TConfiguration configuration, bool overrideExisting)
    {
        if(_services.Any(x => x.ServiceType == typeof(TConfiguration)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException("Configuration type has already been registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(TConfiguration), configuration));
        }
        else
        {
            _ = _services.AddSingleton(configuration);
        }

        if(_services.Any(x => x.ServiceType == typeof(IGasFeeProvider)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException($"{nameof(IGasFeeProvider)} already registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(IGasFeeProvider), typeof(TGasFeeProvider), ServiceLifetime.Singleton));
        }
        else
        {
            _ = _services.AddSingleton<IGasFeeProvider, TGasFeeProvider>();
        }

        return this;
    }

    private void AssertValidReadClientServices()
    {
        if(!_services.Any(x => x.ServiceType == typeof(GrpcChannel)))
        {
            throw new InvalidOperationException($"No {nameof(GrpcChannel)} set. Make sure to call {nameof(WithChannel)} before building the client.");
        }
        if(_chainInfo is null)
        {
            throw new InvalidOperationException($"No {nameof(ChainInfo)} set. Make sure to install a chain before building the client.");
        }

        if(!_services.Any(x => x.ServiceType == typeof(IAuthModuleAdapter)))
        {
            throw new InvalidOperationException($"No {nameof(IAuthModuleAdapter)} set. Make sure to install a chain before building the client.");
        }
        if(!_services.Any(x => x.ServiceType == typeof(ITendermintModuleAdapter)))
        {
            throw new InvalidOperationException($"No {nameof(ITendermintModuleAdapter)} set. Make sure to install a chain before building the client.");
        }
        if(!_services.Any(x => x.ServiceType == typeof(ITxModuleAdapter)))
        {
            throw new InvalidOperationException($"No {nameof(ITxModuleAdapter)} set. Make sure to install a chain before building the client.");
        }
    }

    public ICosmClient BuildReadClient()
    {
        AssertValidReadClientServices();

        var chainConfig = new ChainConfiguration(_chainInfo!.Bech32Prefix);
        _ = _services.AddSingleton<IChainConfiguration>(chainConfig);

        var provider = _services.BuildServiceProvider();
        return new CosmClient(provider, _moduleTypes, chainConfig, false);
    }

    private void AssertValidTxClientServices()
    {
        AssertValidReadClientServices();

        if(!_services.Any(x => x.ServiceType == typeof(IOfflineSigner)))
        {
            throw new InvalidOperationException($"No {nameof(IOfflineSigner)} set. Make sure to call {nameof(WithSigner)} before building the client.");
        }
        if(!_services.Any(x => x.ServiceType == typeof(ITxScheduler)))
        {
            throw new InvalidOperationException($"No {nameof(ITxPublisher)} set. Make sure to call {nameof(WithTxScheduler)} before building the client.");
        }
        if(!_services.Any(x => x.ServiceType == typeof(IGasFeeProvider)))
        {
            throw new InvalidOperationException($"No {nameof(IGasFeeProvider)} set. Make sure to call configure a gas fee before building the client.");
        }

        if(!_services.Any(x => x.ServiceType == typeof(ITxEncoder)))
        {
            throw new InvalidOperationException($"No {nameof(ITxEncoder)} set. Make sure to install a chain before building the client.");
        }
        if(!_services.Any(x => x.ServiceType == typeof(ITxPublisher)))
        {
            throw new InvalidOperationException($"No {nameof(ITxPublisher)} set. Make sure to install a chain before building the client.");
        }
    }

    public ICosmTxClient BuildTxClient()
    {
        AssertValidTxClientServices();

        var chainConfig = new ChainConfiguration(_chainInfo!.Bech32Prefix);
        _ = _services.AddSingleton<IChainConfiguration>(chainConfig);

        var provider = _services.BuildServiceProvider();
        return new CosmClient(provider, _moduleTypes, chainConfig, true);
    }
}
