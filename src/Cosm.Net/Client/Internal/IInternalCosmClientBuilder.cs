using Cosm.Net.Models;
using Cosm.Net.Modules;
using Cosm.Net.Services;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Cosm.Net.Client.Internal;
public interface IInternalCosmClientBuilder
{
    /// <summary>
    /// Accesses the internal ServiceCollection containing modules and their dependencies.
    /// </summary>
    public IServiceCollection ServiceCollection { get; }

    /// <summary>
    /// REQUIRED. Configures the chain that this client is connected to.
    /// </summary>
    /// <param name="bech32Prefix">The address prefix to use for address encoding.</param>
    /// <returns></returns>
    public CosmClientBuilder WithChainInfo(string bech32Prefix, TimeSpan transactionTimeout);

    /// <summary>
    /// Configures Cosm.Net to use the Cosmos default transaction encoding.
    /// </summary>
    /// <returns></returns>
    public CosmClientBuilder UseCosmosTxStructure();

    /// <summary>
    /// Registers an on-chain module type supported by the configured chain.
    /// </summary>
    /// <typeparam name="TIModule">Public interface of the module</typeparam>
    /// <typeparam name="TModule">Internal implementation of the module</typeparam>
    /// <returns></returns>
    public CosmClientBuilder RegisterModule<TIModule, TModule>()
        where TModule : class, IModule, TIModule
        where TIModule : class, IModule;

    /// <summary>
    /// Registers all on-chain module types found in a given assembly.
    /// </summary>
    /// <param name="assembly">The assembly to look for module types in</param>
    /// <returns></returns>
    public CosmClientBuilder RegisterModulesFromAssembly(Assembly assembly);

    /// <summary>
    /// Checks if a given module type has already been registered in this client.
    /// </summary>
    /// <typeparam name="TIModule">Public interface of the module</typeparam>
    /// <returns></returns>
    public bool HasModule<TIModule>();

    /// <summary>
    /// ONLY TXCLIENT. Configures a gas fee provider with a configuration instance. <br/>
    /// The gas fee provider is responsible for figuring out a gas fee to be used for a given gasWanted value. 
    /// </summary>
    /// <typeparam name="TGasFeeProvider">The type of GasFeeProvider to use</typeparam>
    /// <typeparam name="TConfiguration">The type of configuration required by the GasFeeProvider</typeparam>
    /// <param name="configuration">An instance of the confgigured Configuration type</param>
    /// <param name="overrideExisting">If this should override an existing GasFeeProvider</param>
    /// <returns></returns>
    public CosmClientBuilder WithGasFeeProvider<TGasFeeProvider, TConfiguration>(TConfiguration configuration, bool overrideExisting = false)
        where TGasFeeProvider : class, IGasFeeProvider<TConfiguration>
        where TConfiguration : class;

    /// <summary>
    /// ONLY TXCLIENT. Configures a gas fee provider. <br/>
    /// The gas fee provider is responsible for figuring out a gas fee to be used for a given gasWanted value. 
    /// </summary>
    /// <typeparam name="TGasFeeProvider">The type of GasFeeProvider to use</typeparam>
    /// <typeparam name="TConfiguration">The type of configuration required by the GasFeeProvider</typeparam>
    /// <param name="configuration">An instance of the confgigured Configuration type</param>
    /// <param name="overrideExisting">If this should override an existing GasFeeProvider</param>
    /// <returns></returns>
    public CosmClientBuilder WithGasFeeProvider<TGasFeeProvider>(bool overrideExisting = false)
        where TGasFeeProvider : class, IGasFeeProvider;

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
    public CosmClientBuilder WithTxPublisher<TTxPublisher, TConfiguration>(TConfiguration configuration, bool overrideExisting = false)
        where TTxPublisher : class, ITxPublisher<TConfiguration>
        where TConfiguration : class;

    /// <summary>
    /// ONLY TXCLIENT. Configures a transaction publisher. <br/>
    /// The transaction publisher is responsible for processing signed transactions and publishing them to the connected blockchain.
    /// Custom implementations can for example send transactions to multiple nodes or handle chain specific error codes.
    /// </summary>
    /// <typeparam name="TTxPublisher">The type of TxPublisher to use</typeparam>
    /// <param name="overrideExisting">If this should override an existing TxPublisher</param>
    /// <returns></returns>
    public CosmClientBuilder WithTxPublisher<TTxPublisher>(bool overrideExisting = false)
        where TTxPublisher : class, ITxPublisher;

    /// <summary>
    /// ONLY TXCLIENT. Configures a transaction encoder with a configuration instance. <br/>
    /// The transaction encoder is responsible for converting the chain agnostic Cosm.Net types to chain specific transaction and SignDocument binary representations.
    /// </summary>
    /// <typeparam name="TTxEncoder">The type of TxEncoder to use</typeparam>
    /// <typeparam name="TConfiguration">The type of configuration required by the TxEncoder</typeparam>
    /// <param name="configuration">An instance of the confgigured Configuration type</param>
    /// <param name="overrideExisting">If this should override an existing TxEncoder</param>
    /// <returns></returns>
    public CosmClientBuilder WithTxEncoder<TTxEncoder, TConfiguration>(TConfiguration configuration, bool overrideExisting = false)
        where TTxEncoder : class, ITxEncoder<TConfiguration>
        where TConfiguration : class;

    /// <summary>
    /// ONLY TXCLIENT. Configures a transaction encoder.  <br/>
    /// The transaction encoder is responsible for converting the chain agnostic Cosm.Net types to chain specific transaction and SignDocument binary representations.
    /// </summary>
    /// <typeparam name="TTxEncoder">The type of TxEncoder to use</typeparam>
    /// <param name="overrideExisting">If this should override an existing TxEncoder</param>
    /// <returns></returns>
    public CosmClientBuilder WithTxEncoder<TTxEncoder>(bool overrideExisting = false)
        where TTxEncoder : class, ITxEncoder;

    /// <summary>
    /// ONLY TXCLIENT. Configures a transaction confirmer with a configuration instance. <br/>
    /// The transaction confirmer is responsible for allowing the client to wait for certain txHashes to be confirmed on-chain.
    /// </summary>
    /// <typeparam name="TTxConfirmer">The type of TxConfirmer to use</typeparam>
    /// <typeparam name="TConfiguration">The type of configuration required by the TxConfirmer</typeparam>
    /// <param name="configuration">An instance of the confgigured Configuration type</param>
    /// <param name="overrideExisting">If this should override an existing TxConfirmer</param>
    /// <returns></returns>
    public CosmClientBuilder WithTxConfirmer<TTxConfirmer, TConfiguration>(TConfiguration configuration, bool overrideExisting = false)
        where TTxConfirmer : class, ITxConfirmer<TConfiguration>
        where TConfiguration : class;
    /// <summary>
    /// ONLY TXCLIENT. Configures a transaction confirmer.  <br/>
    /// The transaction confirmer is responsible for allowing the client to wait for certain txHashes to be confirmed on-chain.
    /// </summary>
    /// <typeparam name="TTxConfirmer">The type of TxConfirmer to use</typeparam>
    /// <param name="overrideExisting">If this should override an existing TxConfirmer</param>
    /// <returns></returns>
    public CosmClientBuilder WithTxConfirmer<TTxConfirmer>(bool overrideExisting = false)
        where TTxConfirmer : class, ITxConfirmer;

    public CosmClientBuilder WithAccountType<TAccount>(MessageDescriptor descriptor, Func<TAccount, AccountData> handler)
        where TAccount : IMessage<TAccount>;
}
