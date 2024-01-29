using Cosm.Net.Crypto;
using Cosm.Net.Encoding;
using Cosm.Net.Signer;
using Cosm.Net.Wallet.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Secp256k1Net;
using System.Security.Cryptography;

namespace Cosm.Net.Wallet;

public sealed class Secp256k1Wallet : IOfflineSigner
{
    private static readonly Secp256k1 Secp256k1 = new Secp256k1();

    public byte[] PrivateKey { get; }
    public byte[] PublicKey { get; }

    private readonly byte[] UncompressedPublicKey;
    private readonly byte[] AddressBytes;

    public Secp256k1Wallet(Span<byte> privateKey)
    {
        if(privateKey.Length != Secp256k1.PRIVKEY_LENGTH)
        {
            throw new ArgumentException("Unexpected private key length", nameof(privateKey));
        }
        if(!Secp256k1.SecretKeyVerify(privateKey))
        {
            throw new ArgumentException("Private key verification failed", nameof(privateKey));
        }

        PrivateKey = privateKey.ToArray(); //Prevent user from changing private key after ctor
        PublicKey = new byte[Secp256k1.SERIALIZED_COMPRESSED_PUBKEY_LENGTH];

        UncompressedPublicKey = new byte[Secp256k1.PUBKEY_LENGTH];
        if(!Secp256k1.PublicKeyCreate(UncompressedPublicKey, privateKey) ||
           !Secp256k1.PublicKeySerialize(PublicKey, UncompressedPublicKey, Flags.SECP256K1_EC_COMPRESSED))
        {
            throw new ArgumentException("Failed to derive public key from private key", nameof(privateKey));
        }

        byte[] pubkeyHash = SHA256.HashData(PublicKey);

        var ripeMd160 = new RipeMD160Digest();
        ripeMd160.BlockUpdate(pubkeyHash, 0, pubkeyHash.Length);
        AddressBytes = new byte[ripeMd160.GetDigestSize()];
        _ = ripeMd160.DoFinal(AddressBytes, 0);
    }

    public Secp256k1Wallet(string mnemonic, string passphrase = "", BIP39Wordlist wordlist = BIP39Wordlist.English)
        : this(DerivePrivateKeyFromMnemonic(mnemonic, passphrase, wordlist))
    {
    }

    private static byte[] DerivePrivateKeyFromMnemonic(string mnemonic, string passphrase, BIP39Wordlist wordlist)
    {
        //ToDo: Validate mnemonic
        byte[] seed = BIP39.MnemonicToSeed(mnemonic, passphrase);
        string path = $"m/{44 + 2147483648u}'/{118 + 2147483648u}'/{0 + 2147483648u}'/0'/0'";

        var (key, _) = BIP32.DerivePath(seed, path, BIP32Curves.Secp256k1);
        return key;
    }

    public void SignMessage(ReadOnlySpan<byte> message, Span<byte> signatureOutput)
    {
        if(signatureOutput.Length != Secp256k1.SERIALIZED_SIGNATURE_SIZE)
        {
            throw new ArgumentException($"Unexpected signatureOutput length. Expected {Secp256k1.SIGNATURE_LENGTH}, got {signatureOutput.Length}", nameof(signatureOutput));
        }

        Span<byte> MessageHashBuffer = stackalloc byte[32];
        Span<byte> UnserializedSignatureBuffer = stackalloc byte[Secp256k1.UNSERIALIZED_SIGNATURE_SIZE];

        _ = SHA256.HashData(message, MessageHashBuffer);

        if(!Secp256k1.Sign(UnserializedSignatureBuffer, MessageHashBuffer, PrivateKey))
        {
            throw new Exception("Signing failed");
        }
        if(!Secp256k1.SignatureSerializeCompact(signatureOutput, UnserializedSignatureBuffer))
        {
            throw new Exception("Compacting signature failed");
        }
    }

    public byte[] SignMessage(ReadOnlySpan<byte> message)
    {
        byte[] buffer = new byte[Secp256k1.SERIALIZED_SIGNATURE_SIZE];
        SignMessage(message, buffer);
        return buffer;
    }

    public string GetAddress(string prefix)
        => Bech32.EncodeAddress(prefix, AddressBytes)
            ?? throw new InvalidOperationException("Creating address failed");
}
