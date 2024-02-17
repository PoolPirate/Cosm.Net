namespace Cosm.Net.Services;
public interface IMessageDescryptor : IDisposable
{
    public byte[] DecryptMessage(ReadOnlySpan<byte> cipherText);
}
