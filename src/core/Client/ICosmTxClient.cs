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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<TxSimulation> SimulateAsync(ICosmTx tx, CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the gas fee required for a tx with a given amount of gas wanted.
    /// </summary>
    /// <param name="gasWanted">The amount of gas used by the tx.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public ValueTask<Coin> EstimateTxFeesAsync(ulong gasWanted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Simulates a transaction, applies gas buffers, and estimates the tx fee necessary to publish that tx.
    /// </summary>
    /// <param name="tx">The transaction to publish</param>
    /// <param name="gasMultiplier">A value to multiply the simulation gas by.</param>
    /// <param name="gasOffset">A value to add to the simulation gas.</param>    
    /// <param name="cancellationToken"></param>
    /// <returns>TxHash of the transaction</returns>
    public Task<TxEstimation> SimulateAndEstimateTxFeesAsync(ICosmTx tx, 
        double? gasMultiplier = null, ulong? gasOffset = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a transaction without simulating it. Uses the gas amount passed in from parameters.
    /// </summary>
    /// <param name="tx">The transaction to publish.</param>
    /// <param name="gasWanted">The amount of gas requested by the transaction.</param>
    /// <param name="txFee">The coin attached to the transaction for gas fees.</param>
    /// <param name="deadline">Deadline for the GRPC Call. Call will be aborted after the given time.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>TxHash of the transaction</returns>
    public Task<string> PublishTxAsync(ICosmTx tx, ulong gasWanted, Coin txFee,
        DateTime? deadline = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a transaction without simulating it. Uses the gas amount passed in from parameters.
    /// </summary>
    /// <param name="tx">The transaction to publish.</param>
    /// <param name="gasWanted">The amount of gas requested by the transaction.</param>
    /// <param name="txFees">The coins attached to the transaction for gas fees.</param>
    /// <param name="deadline">Deadline for the GRPC Call. Call will be aborted after the given time.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>TxHash of the transaction</returns>
    public Task<string> PublishTxAsync(ICosmTx tx, ulong gasWanted, IEnumerable<Coin> txFees, 
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
    public Task<string> SimulateAndPublishTxAsync(ICosmTx tx, double? gasMultiplier = null, ulong? gasOffset = null,
        DateTime? deadline = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a given txHash to be confirmed on-chain.
    /// </summary>
    /// <param name="txHash">Transaction hash to wait for.</param>
    /// <param name="timeout">The timeout after which to stop waiting.</param>
    /// <param name="throwOnRevert">If the call should throw an exception when the tx is confirmed but reverted on-chain.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A TxExecution object for the confirmed tx.</returns>
    public Task<TxExecution> WaitForTxConfirmationAsync(string txHash, TimeSpan? timeout = null, bool throwOnRevert = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts to internal client for accessing unsupported APIs.
    /// </summary>
    /// <returns></returns>
    public new IInternalCosmTxClient AsInternal();
}
