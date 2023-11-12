using Google.Protobuf;

namespace Cosm.Net.Core.Msg;
public interface ITxMessage<TMsg> : ITxMessage
    where TMsg : IMessage, IMessage<TMsg>
{
}

public interface ITxMessage
{

}