using Cosm.Net.Services;
using Cosm.Net.Tx.Msg;

namespace Cosm.Net.Tx;
public interface ISecretTxMessage : IWasmTxMessage
{
    public SecretEncryptionContext EncryptionContext { get; }
}
