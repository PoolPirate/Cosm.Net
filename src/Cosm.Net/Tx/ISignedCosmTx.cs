using Cosm.Net.Models;
using Google.Protobuf;

namespace Cosm.Net.Tx;
/// <summary>
/// Represents a signed Cosmos SDK transaction ready to be published to the network. 
/// </summary>
public interface ISignedCosmTx : ICosmTx
{
    /// <summary>
    /// The amount of gas requested by the transaction.
    /// </summary>
    public ulong GasWanted { get; }

    /// <summary>
    /// The coins attached to the transaction for fees.
    /// </summary>
    public IReadOnlyList<Coin> TxFees { get; }

    /// <summary>
    /// The sequence number of the sender when this transaction was signed.
    /// </summary>
    public ulong Sequence { get; }

    /// <summary>
    /// The public key that this transaction was signed with.
    /// </summary>
    public ByteString PublicKey { get; }

    /// <summary>
    /// The transaction signature.
    /// </summary>
    public ByteString Signature { get; }
}
