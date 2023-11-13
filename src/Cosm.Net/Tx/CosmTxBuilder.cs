using Cosm.Net.Tx.Msg;

namespace Cosm.Net.Tx;
public class CosmTxBuilder
{
    private readonly List<ITxMessage> _messages;
    private string _memo = string.Empty;
    private ulong _timeoutHeight = 0;

    public CosmTxBuilder()
    {
        _messages = new List<ITxMessage>();
    }

    public CosmTxBuilder AddMessage(ITxMessage msg)
    {
        _messages.Add(msg);
        return this;
    }

    public CosmTxBuilder WithMemo(string memo)
    {
        _memo = memo;
        return this;
    }

    public CosmTxBuilder WithTimeoutHeight(ulong timeoutHeight)
    {
        _timeoutHeight = timeoutHeight;
        return this;
    }

    public CosmTx Build() 
        => new CosmTx(_memo, _timeoutHeight, _messages.ToArray());
}
