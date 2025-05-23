using Miscreant;

namespace Cosm.Net.Services;
public class SecretMessageDecryptor : IMessageDescryptor
{
    private readonly AesSiv _aesSiv;

    public SecretMessageDecryptor(byte[] txEncryptionKey)
    {
        _aesSiv = AesSiv.CreateAesCmacSiv(txEncryptionKey);
    }
    public SecretMessageDecryptor(AesSiv aesSiv)
    {
        _aesSiv = aesSiv;
    }

    public byte[] DecryptMessage(ReadOnlySpan<byte> cipherText)
        => _aesSiv.Open(cipherText.ToArray(), [[]]);

    public void Dispose()
    {
        try
        {
            _aesSiv.Dispose();
        }
        catch
        {
        }
        GC.SuppressFinalize(this);
    }
}
