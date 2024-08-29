using Cosm.Net.Models;
using Cosm.Net.Tx.Msg;
using Google.Protobuf;

namespace Cosm.Net.Tx;
public class SignedTx : ISignedCosmTx
{
    private readonly ICosmTx _tx;
    public ulong Sequence { get; }
    public ulong GasWanted { get; }
    public IReadOnlyList<Coin> TxFees { get; }
    public ByteString PublicKey { get; }
    public ByteString Signature { get; }

    public string Memo => _tx.Memo;
    public ulong TimeoutHeight => _tx.TimeoutHeight;
    public IReadOnlyList<ITxMessage> Messages => _tx.Messages;

    public SignedTx(ICosmTx tx, ulong gasWanted, IEnumerable<Coin> txFees, ulong sequence, ReadOnlySpan<byte> publicKey, ReadOnlySpan<byte> signature)
    {
        _tx = tx;
        GasWanted = gasWanted;
        TxFees = txFees.ToList();
        Sequence = sequence;
        PublicKey = ByteString.CopyFrom(publicKey);
        Signature = ByteString.CopyFrom(signature);
    }
}
