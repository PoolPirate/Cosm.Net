using Cosm.Net.Tx.Msg;
using Google.Protobuf;

namespace Cosm.Net.Tx;
public class SignedTx : ISignedCosmTx
{
    private readonly ICosmTx _tx;
    public ulong Sequence { get; }
    public ByteString Signature { get; }
    public ulong GasWanted { get; }
    public string FeeDenom { get; }
    public ulong FeeAmount { get; }

    public string Memo => _tx.Memo;
    public ulong TimeoutHeight => _tx.TimeoutHeight;
    public IReadOnlyCollection<ITxMessage> Messages => _tx.Messages;

    public SignedTx(ICosmTx tx, ulong sequence, ByteString signature, 
        ulong gasWanted, string feeDenom, ulong feeAmount) 
    {
        _tx = tx;
        Sequence = sequence;
        Signature = signature;
        GasWanted = gasWanted;
        FeeDenom = feeDenom;
        FeeAmount = feeAmount;
    }
}
