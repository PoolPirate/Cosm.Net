using Cosm.Net.Models;
using Cosm.Net.Tx.Msg;
using Google.Protobuf;

namespace Cosm.Net.Tx;
public class SignedTx : ISignedCosmTx
{
    public GasFeeAmount GasFee { get; }
    private readonly ICosmTx _tx;
    public ulong Sequence { get; }
    public ByteString Signature { get; }

    public string Memo => _tx.Memo;
    public ulong TimeoutHeight => _tx.TimeoutHeight;
    public IReadOnlyCollection<ITxMessage> Messages => _tx.Messages;

    public SignedTx(ICosmTx tx, GasFeeAmount gasFee, ulong sequence, ReadOnlySpan<byte> signature) 
    {
        _tx = tx;
        GasFee = gasFee;
        Sequence = sequence;
        Signature = ByteString.CopyFrom(signature);
    }
}
