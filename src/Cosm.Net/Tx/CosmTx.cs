using Cosm.Net.Tx.Msg;

namespace Cosm.Net.Tx;
public class CosmTx : ICosmTx
{
    public string Memo { get; }
    public ulong TimeoutHeight { get; }
    public IReadOnlyList<ITxMessage> Messages { get; }

    public CosmTx(string memo, ulong timeoutHeight, IList<ITxMessage> messages)
    {
        Messages = messages.AsReadOnly();
        Memo = memo;
        TimeoutHeight = timeoutHeight;
    }
}