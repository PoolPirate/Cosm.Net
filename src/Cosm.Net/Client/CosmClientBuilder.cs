using Cosm.Net.Adapters.Internal;
using Cosm.Net.Client.Internal;
using Cosm.Net.Configuration;
using Cosm.Net.Models;
using Cosm.Net.Modules;
using Cosm.Net.Services;
using Cosm.Net.Tx;
using Cosm.Net.Wallet;
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
    private readonly AccountParser _accountParser = new AccountParser();

    private ChainInfo? _chainInfo = null;
    private GasBufferConfiguration _gasBufferConfiguration = new GasBufferConfiguration(1.2, 20000);

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
    /// Configures the default transaction timeout duration when waiting for transactions to be confirmed on the network.
    /// </summary>
    /// <remarks>
    /// Must be called after installing a chain package.
    /// </remarks>
    /// <param name="defaultTransactionTimeout">The timeout duratino to use</param>
    /// <returns></returns>
    public CosmClientBuilder WithDefaultTransactionTimeout(TimeSpan defaultTransactionTimeout)
    {
        if(_chainInfo is null)
        {
            throw new InvalidOperationException($"No {nameof(ChainInfo)} has been set. Install a chain before calling this method.");
        }

        _chainInfo.TransactionTimeout = defaultTransactionTimeout;
        return this;
    }

    public CosmClientBuilder WithGasBuffers(double gasMultiplier, ulong gasOffset)
    {
        if(gasMultiplier < 1)
        {
            throw new ArgumentException("Gas Multiplier must be larger or equal to 1");
        }

        _gasBufferConfiguration = new GasBufferConfiguration(gasMultiplier, gasOffset);
        return this;
    }

    CosmClientBuilder IInternalCosmClientBuilder.WithChainInfo(string bech32Prefix, TimeSpan transactionTimeout)
    {
        if(_chainInfo is not null)
        {
            throw new InvalidOperationException($"{nameof(ChainInfo)} already set.");
        }

        _chainInfo = new ChainInfo(bech32Prefix, transactionTimeout);
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
            .Select(moduleType => new
            {
                ModuleType = moduleType,
                InterfaceTypes = moduleType.GetInterfaces()
                    .Where(x => typeof(IModule).IsAssignableFrom(x))
                    .Where(x => !x.Equals(typeof(IModule)))
                    .ToArray()
            })
            .Where(x => x.InterfaceTypes.Length > 0);

        var registerMethod = typeof(IInternalCosmClientBuilder).GetMethod(
            nameof(IInternalCosmClientBuilder.RegisterModule),
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
        );

        foreach(var moduleType in moduleTypes)
        {
            foreach(var interfaceType in moduleType.InterfaceTypes)
            {
                _ = registerMethod.MakeGenericMethod(interfaceType, moduleType.ModuleType).Invoke(this, []);
            }
        }

        return this;
    }

    bool IInternalCosmClientBuilder.HasModule<TIModule>()
       => _services.Any(x => x.ServiceType == typeof(TIModule));

    /// <summary>
    /// ONLY TXCLIENT. Configures a signer to use for signing transactions.
    /// </summary>
    /// <param name="signer">The signer instance to use</param>
    /// <param name="overrideExisting">If this should override an existing signer</param>
    /// <returns></returns>
    public CosmClientBuilder WithSigner(ICosmSigner signer, bool overrideExisting = false)
    {
        if(_services.Any(x => x.ServiceType == typeof(ICosmSigner)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException($"{nameof(ICosmSigner)} already set.");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(ICosmSigner), signer));
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

    CosmClientBuilder IInternalCosmClientBuilder.WithTxEncoder<TTxEncoder>(bool overrideExisting)
    {
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

    CosmClientBuilder IInternalCosmClientBuilder.WithTxConfirmer<TTxConfirmer>(bool overrideExisting)
    {
        if(_services.Any(x => x.ServiceType == typeof(ITxConfirmer)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException($"{nameof(ITxConfirmer)} already registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(ITxConfirmer), typeof(TTxConfirmer), ServiceLifetime.Singleton));
        }
        else
        {
            _ = _services.AddSingleton<ITxConfirmer, TTxConfirmer>();
        }

        return this;
    }
    CosmClientBuilder IInternalCosmClientBuilder.WithTxConfirmer<TTxConfirmer, TConfiguration>(TConfiguration configuration, bool overrideExisting)
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

        if(_services.Any(x => x.ServiceType == typeof(ITxConfirmer)))
        {
            if(!overrideExisting)
            {
                throw new InvalidOperationException($"{nameof(ITxConfirmer)} already registered");
            }

            _ = _services.Replace(new ServiceDescriptor(typeof(ITxConfirmer), typeof(TTxConfirmer), ServiceLifetime.Singleton));
        }
        else
        {
            _ = _services.AddSingleton<ITxConfirmer, TTxConfirmer>();
        }

        return this;
    }

    public CosmClientBuilder WithConstantGasPrice(string feeDenom, decimal gasPrice)
        => AsInternal().WithGasFeeProvider<ConstantGasFeeProvider, ConstantGasFeeProvider.Configuration>(
                new ConstantGasFeeProvider.Configuration(feeDenom, gasPrice));

    CosmClientBuilder IInternalCosmClientBuilder.UseCosmosTxStructure()
        => AsInternal()
            .WithTxEncoder<CosmosTxEncoder>()
            .AsInternal().WithTxPublisher<TxModulePublisher>()
            .AsInternal().WithTxConfirmer<PollingTxConfirmer>();

    CosmClientBuilder IInternalCosmClientBuilder.WithAccountType<TAccount>(
        Google.Protobuf.Reflection.MessageDescriptor descriptor, Func<TAccount, AccountData> handler)
    {
        _accountParser.RegisterAccountType(descriptor, handler);
        return this;
    }

    public CosmClientBuilder AddWasmd()
    {
        if(!AsInternal().HasModule<IInternalWasmAdapter>())
        {
            throw new InvalidOperationException($"No {nameof(IInternalWasmAdapter)} set. Make sure to install a chain that supports wasmd before calling {nameof(AddWasmd)}");
        }

        _ = _services.AddSingleton<IContractFactory>(
            provider => new ContractFactory(provider.GetRequiredService<IInternalWasmAdapter>()));

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

        if(!_services.Any(x => x.ServiceType == typeof(IInternalAuthAdapter)))
        {
            throw new InvalidOperationException($"No {nameof(IInternalAuthAdapter)} set. Make sure to install a chain before building the client.");
        }
        if(!_services.Any(x => x.ServiceType == typeof(IInternalTendermintAdapter)))
        {
            throw new InvalidOperationException($"No {nameof(IInternalTendermintAdapter)} set. Make sure to install a chain before building the client.");
        }
        if(!_services.Any(x => x.ServiceType == typeof(IInternalTxAdapter)))
        {
            throw new InvalidOperationException($"No {nameof(IInternalTxAdapter)} set. Make sure to install a chain before building the client.");
        }
    }

    /// <summary>
    /// Build the configured client excluding transaction capabilities.
    /// </summary>
    /// <returns></returns>
    public ICosmClient BuildReadClient()
    {
        AssertValidReadClientServices();

        var chainConfig = new ChainConfiguration(_chainInfo!.Bech32Prefix, _chainInfo!.TransactionTimeout);

        _ = _services.AddSingleton(_accountParser);
        _ = _services.AddSingleton<IChainConfiguration>(chainConfig);

        var provider = _services.BuildServiceProvider();
        return new CosmClient(provider, _moduleTypes, chainConfig, false);
    }

    private void AssertValidTxClientServices()
    {
        AssertValidReadClientServices();

        if(!_services.Any(x => x.ServiceType == typeof(ICosmSigner)))
        {
            throw new InvalidOperationException($"No {nameof(ICosmSigner)} set. Make sure to call {nameof(WithSigner)} before building the client.");
        }
        if(!_services.Any(x => x.ServiceType == typeof(ITxScheduler)))
        {
            throw new InvalidOperationException($"No {nameof(ITxScheduler)} set. Make sure to call {nameof(WithTxScheduler)} before building the client.");
        }
        if(!_services.Any(x => x.ServiceType == typeof(IGasFeeProvider)))
        {
            throw new InvalidOperationException($"No {nameof(IGasFeeProvider)} set. Make sure to call configure a gas fee before building the client.");
        }
        if(!_services.Any(x => x.ServiceType == typeof(ITxConfirmer)))
        {
            throw new InvalidOperationException($"No {nameof(ITxConfirmer)} set. Make sure to install it before building the client.");
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

        var chainConfig = new ChainConfiguration(_chainInfo!.Bech32Prefix, _chainInfo!.TransactionTimeout);

        _ = _services.AddSingleton(_accountParser);
        _ = _services.AddSingleton<IChainConfiguration>(chainConfig);
        _ = _services.AddSingleton<IGasBufferConfiguration>(_gasBufferConfiguration);

        var provider = _services.BuildServiceProvider();
        return new CosmClient(provider, _moduleTypes, chainConfig, true);
    }
}
