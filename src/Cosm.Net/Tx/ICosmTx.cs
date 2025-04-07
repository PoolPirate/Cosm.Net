using Cosm.Net.Tx.Msg;

namespace Cosm.Net.Tx;
/// <summary>
/// Represents an unsigned Cosmos SDK transaction. Used for transaction simulations.
/// </summary>
public interface ICosmTx
{
    /// <summary>
    /// The memo of the transaction.
    /// </summary>
    public string Memo { get; }

    /// <summary>
    /// The timeout height of the transaction.
    /// </summary>
    public long TimeoutHeight { get; }

    /// <summary>
    /// The messages attached to the transaction.
    /// </summary>
    public IReadOnlyList<ITxMessage> Messages { get; }
}
