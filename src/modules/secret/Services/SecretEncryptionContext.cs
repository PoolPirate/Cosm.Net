namespace Cosm.Net.Services;
public class SecretEncryptionContext
{
    internal byte[] TxEncryptionKey { get; }

    public SecretEncryptionContext(byte[] txEncryptionKey)
    {
        TxEncryptionKey = txEncryptionKey;
    }
}
