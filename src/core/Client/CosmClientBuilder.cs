using Cosm.Net.Adapters;
using Cosm.Net.Client.Internal;
using Cosm.Net.Services;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Grpc.Core;
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

    /// <summary>
    /// Accesses the internal ServiceCollection containing modules and their dependencies.
    /// </summary>
    IServiceCollection IInternalCosmClientBuilder.ServiceCollection
        => _services;
    /// <summary>
    /// Converts to internal client builder for accessing unsupported APIs and performing advanced configuration.
    /// </summary>
    /// <returns></returns>
    public IInternalCosmClientBuilder AsInternal()
        => this;

    /// <summary>
    /// REQUIRED. Configures the underlying GrpcChannel the client is using.
    /// </summary>
    /// <param name="channel">The GrpcChannel to use</param>
    /// <returns></returns>
    public CosmClientBuilder WithChannel(GrpcChannel channel) 
        => WithCallInvoker(channel.CreateCallInvoker());

    /// <summary>
    /// Configures the underlying CallInvoker the client is using. This is a lower-level alternative to calling WithChannel.
    /// </summary>
    /// <param name="callInvoker">The callinvoker to use</param>
    /// <returns></returns>
    public CosmClientBuilder WithCallInvoker(CallInvoker callInvoker)
    {
        if(_services.Any(x => x.ServiceType == typeof(CallInvoker)))
        {
            throw new InvalidOperationException($"{nameof(CallInvoker)} already set.");
        }

        _ = _services.AddSingleton(callInvoker);
        return this;
    }

    /// <summary>
    /// REQUIRED. Configures the chain that this client is connected to.
    /// </summary>
    /// <param name="bech32Prefix">The address prefix to use for address encoding.</param>
    /// <returns></returns>
    CosmClientBuilder IInternalCosmClientBuilder.WithChainInfo(string bech32Prefix)
    {
        if(_chainInfo is not null)
        {
            throw new InvalidOperationException($"{nameof(ChainInfo)} already set.");
        }

        _chainInfo = new ChainInfo(bech32Prefix);
        return this;
    }

    /// <summary>
    /// Registers an on-chain module type supported by the configured chain.
    /// </summary>
    /// <typeparam name="TIModule">Public interface of the module</typeparam>
    /// <typeparam name="TModule">Internal implementation of the module</typeparam>
    /// <returns></returns>
    CosmClientBuilder IInternalCosmClientBuilder.RegisterModule<TIModule, TModule>()
    {
        if(!_services.Any(x => x.ServiceType == typeof(TModule)))
        {
            _ = _services.AddSingleton<TIModule, TModule>();
            _moduleTypes.Add(typeof(TIModule));
        }

        return this;
    }

    /// <summary>
    /// Registers all on-chain module types found in a given assembly.
    /// </summary>
    /// <param name="assembly">The assembly to look for module types in</param>
    /// <returns></returns>
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

    /// <summary>
    /// Checks if a given module type has already been registered in this client.
    /// </summary>
    /// <typeparam name="TIModule">Public interface of the module</typeparam>
    /// <returns></returns>
    bool IInternalCosmClientBuilder.HasModule<TIModule>()
       => _services.Any(x => x.ServiceType == typeof(TIModule));

    /// <summary>
    /// ONLY TXCLIENT. Configures a signer to use for signing transactions.
    /// </summary>
    /// <param name="signer">The signer instance to use</param>
    /// <param name="overrideExisting">If this should override an existing signer</param>
    /// <returns></returns>
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

    /// <summary>
    /// ONLY TXCLIENT. Configures a transaction scheduler.  <br/>
    /// The transaction scheduler is responsible for managing the sequence number and retrying transactions.
    /// </summary>
    /// <typeparam name="TTxScheduler">The type of TransactionScheduler to use</typeparam>
    /// <param name="overrideExisting">If this should override an existing TxScheduler</param>
    /// <returns></returns>
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
    /// <summary>
    /// ONLY TXCLIENT. Configures a transaction scheduler with a configuration instance.  <br/>
    /// The transaction scheduler is responsible for managing the sequence number and retrying transactions.
    /// </summary>
    /// <typeparam name="TTxScheduler">The type of TxScheduler to use</typeparam>
    /// <typeparam name="TConfiguration">The type of configuration required by the TxScheduler</typeparam>
    /// <param name="configuration">An instance of the confgigured Configuration type</param>
    /// <param name="overrideExisting">If this should override an existing TxScheduler</param>
    /// <returns></returns>
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

    /// <summary>
    /// ONLY TXCLIENT. Configures a transaction encoder.  <br/>
    /// The transaction encoder is responsible for converting the chain agnostic Cosm.Net types to chain specific transaction and SignDocument binary representations.
    /// </summary>
    /// <typeparam name="TTxEncoder">The type of TxEncoder to use</typeparam>
    /// <param name="overrideExisting">If this should override an existing TxEncoder</param>
    /// <returns></returns>
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
    /// <summary>
    /// ONLY TXCLIENT. Configures a transaction encoder with a configuration instance. <br/>
    /// The transaction encoder is responsible for converting the chain agnostic Cosm.Net types to chain specific transaction and SignDocument binary representations.
    /// </summary>
    /// <typeparam name="TTxEncoder">The type of TxEncoder to use</typeparam>
    /// <typeparam name="TConfiguration">The type of configuration required by the TxEncoder</typeparam>
    /// <param name="configuration">An instance of the confgigured Configuration type</param>
    /// <param name="overrideExisting">If this should override an existing TxEncoder</param>
    /// <returns></returns>
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

    /// <summary>
    /// ONLY TXCLIENT. Configures a transaction publisher. <br/>
    /// The transaction publisher is responsible for processing signed transactions and publishing them to the connected blockchain.
    /// Custom implementations can for example send transactions to multiple nodes or handle chain specific error codes.
    /// </summary>
    /// <typeparam name="TTxPublisher">The type of TxPublisher to use</typeparam>
    /// <param name="overrideExisting">If this should override an existing TxPublisher</param>
    /// <returns></returns>
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
    /// <summary>
    /// ONLY TXCLIENT. Configures a transaction publisher with a configuration instance. <br/>
    /// The transaction publisher is responsible for processing signed transactions and publishing them to the connected blockchain.
    /// Custom implementations can for example send transactions to multiple nodes or handle chain specific error codes.
    /// </summary>
    /// <typeparam name="TTxPublisher">The type of TxPublisher to use</typeparam>
    /// <typeparam name="TConfiguration">The type of configuration required by the TxPublisher</typeparam>
    /// <param name="configuration">An instance of the confgigured Configuration type</param>
    /// <param name="overrideExisting">If this should override an existing TxPublisher</param>
    /// <returns></returns>
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

    /// <summary>
    /// ONLY TXCLIENT. Configures a gas fee provider. <br/>
    /// The gas fee provider is responsible for figuring out a gas fee to be used for a given gasWanted value. 
    /// </summary>
    /// <typeparam name="TGasFeeProvider">The type of GasFeeProvider to use</typeparam>
    /// <typeparam name="TConfiguration">The type of configuration required by the GasFeeProvider</typeparam>
    /// <param name="configuration">An instance of the confgigured Configuration type</param>
    /// <param name="overrideExisting">If this should override an existing GasFeeProvider</param>
    /// <returns></returns>
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
    /// <summary>
    /// ONLY TXCLIENT. Configures a gas fee provider with a configuration instance. <br/>
    /// The gas fee provider is responsible for figuring out a gas fee to be used for a given gasWanted value. 
    /// </summary>
    /// <typeparam name="TGasFeeProvider">The type of GasFeeProvider to use</typeparam>
    /// <typeparam name="TConfiguration">The type of configuration required by the GasFeeProvider</typeparam>
    /// <param name="configuration">An instance of the confgigured Configuration type</param>
    /// <param name="overrideExisting">If this should override an existing GasFeeProvider</param>
    /// <returns></returns>
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
        if(!_services.Any(x => x.ServiceType == typeof(CallInvoker)))
        {
            throw new InvalidOperationException($"No {nameof(CallInvoker)} set. Make sure to call {nameof(WithChannel)} or {nameof(WithCallInvoker)} before building the client.");
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

    /// <summary>
    /// Build the configured client excluding transaction capabilities.
    /// </summary>
    /// <returns></returns>
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
            throw new InvalidOperationException($"No {nameof(ITxScheduler)} set. Make sure to call {nameof(WithTxScheduler)} before building the client.");
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

    /// <summary>
    /// Build the configured client including transaction capabilities.
    /// </summary>
    /// <returns></returns>
    public ICosmTxClient BuildTxClient()
    {
        AssertValidTxClientServices();

        var chainConfig = new ChainConfiguration(_chainInfo!.Bech32Prefix);
        _ = _services.AddSingleton<IChainConfiguration>(chainConfig);

        var provider = _services.BuildServiceProvider();
        return new CosmClient(provider, _moduleTypes, chainConfig, true);
    }
}
