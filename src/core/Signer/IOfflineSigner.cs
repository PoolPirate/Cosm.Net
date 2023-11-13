namespace Cosm.Net.Signer;
public interface IOfflineSigner
{
    public byte[] PublicKey { get; }
    public string GetAddress(string prefix);

    public void SignMessage(ReadOnlySpan<byte> message, Span<byte> signatureOutput);
    public byte[] SignMessage(ReadOnlySpan<byte> message);
}
