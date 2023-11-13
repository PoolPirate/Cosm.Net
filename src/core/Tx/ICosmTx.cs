using Cosm.Net.Tx.Msg;
using Google.Protobuf;

namespace Cosm.Net.Tx;
public interface ICosmTx
{
    public string Memo { get; }
    public ulong TimeoutHeight { get; }
    public IReadOnlyCollection<ITxMessage> Messages { get; }
}
