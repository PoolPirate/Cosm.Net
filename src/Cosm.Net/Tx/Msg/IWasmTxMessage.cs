using Google.Protobuf;

namespace Cosm.Net.Tx.Msg;
public interface IWasmTxMessage<TMsg> : IWasmTxMessage
    where TMsg : IMessage, IMessage<TMsg>
{
}

public interface IWasmTxMessage : ITxMessage
{
    public string TxSender { get; }

    public string ToJson();
}