using Cosm.Net.Models;
using Google.Protobuf;

namespace Cosm.Net.Tx;
/// <summary>
/// Represents a signed Cosmos SDK transaction ready to be published to the network. 
/// </summary>
public interface ISignedCosmTx : ICosmTx
{
    /// <summary>
    /// The gas fee paid when publishing this transaction.
    /// </summary>
    public GasFeeAmount GasFee { get; }

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
