using Cosm.Net.Services;
using Cosm.Net.Tx.Msg;
using Google.Protobuf;

namespace Cosm.Net.Tx;
internal class SecretTxMessage<TMsg> : TxMessage<TMsg>, ISecretTxMessage
    where TMsg : IMessage, IMessage<TMsg>
{
    public SecretEncryptionContext EncryptionContext { get; }

    public SecretTxMessage(TMsg msg, SecretEncryptionContext encryptionContext) 
        : base(msg)
    {
        EncryptionContext = encryptionContext;
    }
}
