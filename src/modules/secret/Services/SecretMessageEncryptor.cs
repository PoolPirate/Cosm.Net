using Miscreant;
using System.Security.Cryptography;
using X25519;
using Cosm.Net.Modules;

namespace Cosm.Net.Services;
internal class SecretMessageEncryptor : IInitializeableService
{
    private readonly static byte[] HkdfSalt = Convert.FromHexString("000000000000000000024bead8df69990852c202db0e0097c1a12ea637d7e96d");

    private readonly IRegistrationModule _registrationModule;
    private readonly byte[] _encryptionPrivKey;
    private readonly byte[] _encryptionPubKey;

    private byte[]? _consensusIoPubKey;

    public SecretMessageEncryptor(IRegistrationModule registrationModule, byte[]? encryptionSeed)
    {
        _registrationModule = registrationModule;

        if(encryptionSeed is not null && encryptionSeed.Length != 32)
        {
            throw new ArgumentException("EncryptionKey must be 32 bytes long.");
        }

        encryptionSeed ??= Aead.GenerateKey256();
        var keyPair = X25519.X25519KeyAgreement.GenerateKeyFromPrivateKey(encryptionSeed);
        _encryptionPrivKey = keyPair.PrivateKey;
        _encryptionPubKey = keyPair.PublicKey;
    }

    async ValueTask IInitializeableService.InitializeAsync()
    {
        var txKey = await _registrationModule.TxKeyAsync();
        _consensusIoPubKey = txKey.Key_.ToByteArray();
    }

    public byte[] GetTxEncryptionKey(ReadOnlySpan<byte> nonce)
    {
        if(_consensusIoPubKey is null)
        {
            throw new InvalidOperationException("SecretMessageEncryptor has not been initialized!");
        }

        var txEncryptionIkm = new byte[64];

        var sharedKey = X25519KeyAgreement.Agreement(_encryptionPrivKey, _consensusIoPubKey);

        sharedKey.AsSpan(0, 32).CopyTo(txEncryptionIkm);
        nonce.CopyTo(txEncryptionIkm.AsSpan(32, 32));

        return HKDF.DeriveKey(HashAlgorithmName.SHA256, txEncryptionIkm, 32, HkdfSalt);
    }

    public byte[] EncryptMessage(ReadOnlySpan<byte> message, out byte[] nonce)
    {
        nonce = Aead.GenerateNonce(32);

        var nonceStr = Convert.ToHexString(nonce);
        var txEncryptionKey = GetTxEncryptionKey(nonce);

        using var aesSiv = AesSiv.CreateAesCmacSiv(txEncryptionKey);

        var cipherText = aesSiv.Seal(message.ToArray(), [[]]);
        var buffer = new byte[64 + cipherText.Length];

        System.Text.Encoding.UTF8.GetBytes(nonceStr).AsSpan()
            .CopyTo(buffer);

        nonce.CopyTo(buffer.AsSpan(0, 32));
        _encryptionPubKey.CopyTo(buffer.AsSpan(32, 32));
        cipherText.CopyTo(buffer.AsSpan(64, cipherText.Length));

        return buffer;
    }

    public byte[] DecryptMessage(ReadOnlySpan<byte> cipherText, ReadOnlySpan<byte> nonce)
    {
        var txEncryptionKey = GetTxEncryptionKey(nonce);
        using var aesSiv = AesSiv.CreateAesCmacSiv(txEncryptionKey);
        return aesSiv.Open(cipherText.ToArray(), [[]]);
    }
}
