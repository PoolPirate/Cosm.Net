using Cosm.Net.Adapters;
using Cosm.Net.Client.Internal;
using Cosm.Net.Models;
using Cosm.Net.Modules;
using Cosm.Net.Tx;
using Grpc.Core;

namespace Cosm.Net.Client;
public interface ICosmClient
{
    /// <summary>
    /// Information about the chain that this client is connected to.
    /// </summary>
    public IChainConfiguration Chain { get; }

    /// <summary>
    /// Gets an adapter to the chains native bank module.
    /// </summary>
    public IBankAdapter Bank { get; }

    /// <summary>
    /// Initializes the client. Must be called before using any other methods.
    /// </summary>
    /// <returns></returns>
    public Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an instance of an on-chain module binding.
    /// </summary>
    /// <typeparam name="TModule">The type of module to create</typeparam>
    /// <returns></returns>
    public TModule Module<TModule>() where TModule : IModule;

    /// <summary>
    /// Converts to internal client for accessing unsupported APIs.
    /// </summary>
    /// <returns></returns>
    public IInternalCosmClient AsInternal();

    /// <summary>
    /// Searches for the given txHash on the chain and converts it to a common representation.
    /// </summary>
    /// <param name="txHash">The tx hash to search for</param>
    /// <param name="headers">Additional requests headers for the GRPC Call.</param>
    /// <param name="deadline">Deadline for the GRPC Call. Call will be aborted after the given time.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>TxExecution if tx is found, null otherwise</returns>
    public Task<TxExecution?> GetTxByHashAsync(string txHash,
        Metadata? headers = default, DateTime? deadline = default, CancellationToken cancellationToken = default);
}
