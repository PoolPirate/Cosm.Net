using Google.Protobuf;

namespace Cosm.Net.Core.Msg;
public interface ITxMessage<TMsg>
    where TMsg : IMessage, IMessage<TMsg>
{
}

public interface ITxMessage
{

}