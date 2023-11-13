using Google.Protobuf;

namespace Cosm.Net.Tx.Msg;
public interface ITxMessage<TMsg> : ITxMessage
    where TMsg : IMessage, IMessage<TMsg>
{
}

public interface ITxMessage
{
    public string GetTypeUrl();
    public ByteString ToByteString();
}