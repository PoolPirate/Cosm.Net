using Cosm.Net.Tx.Msg;

namespace Cosm.Net.Tx;
public interface ICosmTx
{
    public string Memo { get; }
    public ulong TimeoutHeight { get; }
    public IReadOnlyCollection<ITxMessage> Messages { get; }
}
