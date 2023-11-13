using Cosm.Net.Tx;
using Google.Protobuf;

namespace Cosm.Net.Services;
public interface ITxEncoder
{
    public byte[] GetSignSignDoc(ICosmTx tx, ulong accountNumber, ulong sequence);

    public ByteString EncodeTx(ICosmTx tx, ulong sequence);
    public ByteString EncodeTx(ISignedCosmTx tx);
}
