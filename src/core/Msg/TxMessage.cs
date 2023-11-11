using Google.Protobuf;

namespace Cosm.Net.Core.Msg;
public class TxMessage<TMsg> : ITxMessage<TMsg>
        where TMsg : IMessage, IMessage<TMsg>
{
    private readonly TMsg _msg;

    public TxMessage(TMsg msg)
    {
        _msg = msg;
    }
}
