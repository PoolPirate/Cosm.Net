using Cosm.Net.Core.Msg;

namespace Cosm.Net.Tx;
public class CosmTxBuilder
{
    private readonly List<ITxMessage> _messages;

    public CosmTxBuilder()
    {
        _messages = new List<ITxMessage>();
    }

    public CosmTxBuilder AddMessage(ITxMessage msg)
    {
        _messages.Add(msg);
        return this;
    }

    public CosmTx Build() 
        => new CosmTx(_messages.ToArray());
}
