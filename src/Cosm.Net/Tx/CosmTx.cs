using Cosm.Net.Tx.Msg;

namespace Cosm.Net.Tx;
public class CosmTx : ICosmTx
{
    public string Memo { get; }
    public long TimeoutHeight { get; }
    public IReadOnlyList<ITxMessage> Messages { get; }

    public CosmTx(string memo, long timeoutHeight, IList<ITxMessage> messages)
    {
        Messages = messages.AsReadOnly();
        Memo = memo;
        TimeoutHeight = timeoutHeight;
    }
}