using Cosm.Net.Tx.Msg;
using Google.Protobuf;

namespace Cosm.Net.Tx;
public class SignedTx : ISignedCosmTx
{
    private readonly ICosmTx _tx;
    public ulong Sequence { get; }
    public ByteString Signature { get; }

    public string Memo => _tx.Memo;
    public ulong TimeoutHeight => _tx.TimeoutHeight;
    public IReadOnlyCollection<ITxMessage> Messages => _tx.Messages;

    public SignedTx(ICosmTx tx, ulong sequence, ByteString signature) 
    {
        _tx = tx;
        Sequence = sequence;
        Signature = signature;
    }
}
