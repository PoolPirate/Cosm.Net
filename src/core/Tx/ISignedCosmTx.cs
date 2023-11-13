using Google.Protobuf;

namespace Cosm.Net.Tx;
public interface ISignedCosmTx : ICosmTx
{
    public ulong Sequence { get; }
    public ByteString Signature { get; }
}
