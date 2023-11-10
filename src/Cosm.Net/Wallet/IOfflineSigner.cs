namespace Cosm.Net.Wallet;
public interface IOfflineSigner
{
    public void SignMessage(ReadOnlySpan<byte> message, Span<byte> signatureOutput);
}
