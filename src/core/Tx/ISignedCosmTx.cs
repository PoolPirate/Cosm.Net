using Google.Protobuf;

namespace Cosm.Net.Tx;
public interface ISignedCosmTx : ICosmTx
{
    public ulong Sequence { get; }
    public ByteString Signature { get; }

    public ulong GasWanted { get; }
    public string FeeDenom { get; }
    public ulong FeeAmount { get; }
}
