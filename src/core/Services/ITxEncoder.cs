using Cosm.Net.Models;
using Cosm.Net.Tx;
using Google.Protobuf;

namespace Cosm.Net.Services;
public interface ITxEncoder
{
    public byte[] GetSignSignDoc(ICosmTx tx, GasFeeAmount gasFee, ulong accountNumber, ulong sequence);

    public ByteString EncodeTx(ICosmTx tx, ulong sequence, string feeDenom);
    public ByteString EncodeTx(ISignedCosmTx tx);
}

public interface ITxEncoder<TConfiguration> : ITxEncoder;