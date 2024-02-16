using Cosm.Net.Client.Internal;
using Cosm.Net.Models;
using Cosm.Net.Tx;

namespace Cosm.Net.Client;
public interface ICosmTxClient : ICosmClient
{
    /// <summary>
    /// Simulates a transaction.
    /// </summary>
    /// <param name="tx">The transaction to simulate</param>
    /// <returns></returns>
    public Task<TxSimulation> SimulateAsync(ICosmTx tx);

    /// <summary>
    /// Publishes a transaction without simulating it. Uses the gas amount passed in from parameters.
    /// </summary>
    /// <param name="tx">The transaction to publish</param>
    /// <param name="gasFee">The amount of gas fees to pay</param>
    /// <param name="deadline">Deadline for the GRPC Call. Call will be aborted after the given time.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>TxHash of the transaction</returns>
    public Task<string> PublishTxAsync(ICosmTx tx, GasFeeAmount gasFee, 
        DateTime? deadline = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a transaction without simulating it. Uses the configured GasFeeProvider to calculate the gas fee for the gasWanted passed in from paramters.
    /// </summary>
    /// <param name="tx">The transaction to publish</param>
    /// <param name="gasWanted">The amount of gas wanted for the call.</param>
    /// <param name="deadline">Deadline for the GRPC Call. Call will be aborted after the given time.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>TxHash of the transaction</returns>
    public Task<string> PublishTxAsync(ICosmTx tx, ulong gasWanted, 
        DateTime? deadline = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Simulates a transaction and publishes it if the simulation was successful.
    /// </summary>
    /// <param name="tx">The transaction to publish</param>
    /// <param name="gasMultiplier">A value to multiply the simulation gas by.</param>
    /// <param name="gasOffset">A value to add to the simulation gas.</param>
    /// <param name="deadline">Deadline for the GRPC Call. Call will be aborted after the given time.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>TxHash of the transaction</returns>
    public Task<string> SimulateAndPublishTxAsync(ICosmTx tx, decimal gasMultiplier = 1.2m, ulong gasOffset = 20000,
        DateTime? deadline = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts to internal client for accessing unsupported APIs.
    /// </summary>
    /// <returns></returns>
    public new IInternalCosmTxClient AsInternal();
}
