using Google.Protobuf;

namespace Cosm.Net.Tx.Msg;
public class TxMessage<TMsg> : ITxMessage<TMsg>
        where TMsg : IMessage, IMessage<TMsg>
{
    private readonly TMsg _msg;

    public TxMessage(TMsg msg)
    {
        _msg = msg;
    }

    public string GetTypeUrl()
        => $"/{_msg.Descriptor.FullName}";
    public ByteString ToByteString()
        => _msg.ToByteString();
}
