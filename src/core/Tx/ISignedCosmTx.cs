using Cosm.Net.Models;
using Google.Protobuf;

namespace Cosm.Net.Tx;
public interface ISignedCosmTx : ICosmTx
{
    public GasFeeAmount GasFee { get; }
    public ulong Sequence { get; }
    public ByteString Signature { get; }
}
