using Google.Protobuf;

namespace Cosm.Net.Tx.Msg;
public class WasmTxMessage<TMsg> : IWasmTxMessage<TMsg>
        where TMsg : IMessage, IMessage<TMsg>
{
    private readonly TMsg _msg;
    private readonly string _requestJson;

    public WasmTxMessage(TMsg msg, string requestJson)
    {
        _msg = msg;
        _requestJson = requestJson;
    }

    public string GetTypeUrl()
        => $"/{_msg.Descriptor.FullName}";
    public ByteString ToByteString()
        => _msg.ToByteString();
    public string ToJson()
        => _requestJson;
}
