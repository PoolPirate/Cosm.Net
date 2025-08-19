using Google.Protobuf;

namespace Cosm.Net.Tx.Msg;
public class WasmTxMessage<TMsg>(TMsg msg, string requestJson, string txSender) : IWasmTxMessage<TMsg>
    where TMsg : IMessage, IMessage<TMsg>
{
    private readonly TMsg _msg = msg;
    private readonly string _requestJson = requestJson;

    public string TxSender { get; } = txSender;

    public string GetTypeUrl()
        => $"/{_msg.Descriptor.FullName}";
    public ByteString ToByteString()
        => _msg.ToByteString();
    public string ToJson()
        => _requestJson;
}
