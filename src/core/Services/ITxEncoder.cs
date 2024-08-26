using Cosm.Net.Models;
using Cosm.Net.Tx;
using Google.Protobuf;

namespace Cosm.Net.Services;
public interface ITxEncoder
{
    public byte[] GetSignSignDoc(ICosmTx tx, ByteString publicKey, ulong gasWanted, IEnumerable<Coin> txFees, ulong accountNumber, ulong sequence);

    public ByteString EncodeTx(ICosmTx tx, ByteString publicKey, ulong sequence, string feeDenom);
    public ByteString EncodeTx(ISignedCosmTx tx);
}

public interface ITxEncoder<TConfiguration> : ITxEncoder;