using Cosm.Net.Services;
using Cosm.Net.Tx.Msg;
using Google.Protobuf;

namespace Cosm.Net.Tx;
internal class SecretTxMessage<TMsg> : TxMessage<TMsg>, ISecretTxMessage
    where TMsg : IMessage, IMessage<TMsg>
{
    public SecretEncryptionContext EncryptionContext { get; }
    public string TxSender { get; }
    private readonly string _requestJson;

    public SecretTxMessage(TMsg msg, string txSender, string requestJson, SecretEncryptionContext encryptionContext)
        : base(msg)
    {
        EncryptionContext = encryptionContext;
        TxSender = txSender;
        _requestJson = requestJson;
    }

    public string ToJson()
        => _requestJson;
}
