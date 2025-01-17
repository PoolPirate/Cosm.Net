namespace Cosm.Net.Signer;
public interface ICosmSigner
{
    /// <summary>
    /// Public key of this signer
    /// </summary>
    public ReadOnlySpan<byte> PublicKey { get; }

    /// <summary>
    /// The unencoded bytes that make up the address of this signer.
    /// </summary>
    public ReadOnlySpan<byte> AddressBytes { get; }

    /// <summary>
    /// Get a Bech32 encoded address from the public key given a prefix.
    /// </summary>
    /// <param name="prefix">The Bech32 prefix to use</param>
    /// <returns></returns>
    public string GetAddress(string prefix);

    /// <summary>
    /// Signs a message and writes the generated signature into an output buffer.
    /// </summary>
    /// <param name="message">The message to sign</param>
    /// <param name="signatureOutput">The output buffer to write into. Must be at least 64 bytes.</param>
    /// <returns>A boolean representing if the signature was written into the output successfully</returns>
    public bool SignMessage(ReadOnlySpan<byte> message, Span<byte> signatureOutput);

    /// <summary>
    /// Signs a message.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <returns>Signature as a byte array</returns>
    public byte[] SignMessage(ReadOnlySpan<byte> message);
}
