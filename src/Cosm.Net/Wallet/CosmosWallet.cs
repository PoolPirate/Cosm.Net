using Cosm.Net.Encoding;
using Cosm.Net.Signer;
using Keysmith.Net.BIP;
using Keysmith.Net.EC;
using Keysmith.Net.Wallet;
using Org.BouncyCastle.Crypto.Digests;
using System.Security.Cryptography;

namespace Cosm.Net.Wallet;
public class CosmosWallet : BaseWeierstrassHdWallet<Secp256k1>, IOfflineSigner
{
    public byte[] AddressBytes { get; private set; }
    public byte[] PublicKey => _compressedPublicKey;

    public CosmosWallet(ReadOnlySpan<byte> privateKey)
        :base(Secp256k1.Instance, privateKey)
    {
        AddressBytes = GenerateAddress();
    }

    public CosmosWallet(string mnemonic, string passphrase = "", int accountIndex = 0)
        :base(Secp256k1.Instance, mnemonic, passphrase, BIP44.Cosmos(accountIndex))
    {
        AddressBytes = GenerateAddress();
    }

    public CosmosWallet(string mnemonic, string passphrase = "", params ReadOnlySpan<uint> derivationPath)
        : base(Secp256k1.Instance, mnemonic, passphrase, derivationPath)
    {
        AddressBytes = GenerateAddress();
    }

    private byte[] GenerateAddress()
    {
        byte[] pubkeyHash = SHA256.HashData(_compressedPublicKey);
        var ripeMd160 = new RipeMD160Digest();
        ripeMd160.BlockUpdate(pubkeyHash, 0, pubkeyHash.Length);
        var addressBytes = new byte[ripeMd160.GetDigestSize()];
        _ = ripeMd160.DoFinal(addressBytes, 0);
        return addressBytes;
    }

    public string GetAddress(string prefix)
        => Bech32.EncodeAddress(prefix, AddressBytes)
            ?? throw new InvalidOperationException("Creating address failed");

    public bool SignMessage(ReadOnlySpan<byte> message, Span<byte> signatureOutput)
    {
        Span<byte> hashBuffer = stackalloc byte[32];
        SHA256.TryHashData(message, hashBuffer, out _);
        return TrySign(hashBuffer, signatureOutput);
    }

    public byte[] SignMessage(ReadOnlySpan<byte> message)
    {
        Span<byte> hashBuffer = stackalloc byte[32];
        SHA256.TryHashData(message, hashBuffer, out _);
        return Sign(hashBuffer);
    }
}
