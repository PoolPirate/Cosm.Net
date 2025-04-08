using Cosm.Net.Modules;
using Miscreant;
using System.Security.Cryptography;
using X25519;

namespace Cosm.Net.Services;
public class SecretEncryptionProvider : IInitializeableService
{
    private readonly static byte[] _hkdfSalt = Convert.FromHexString("000000000000000000024bead8df69990852c202db0e0097c1a12ea637d7e96d");
    private readonly static RandomNumberGenerator _random = RandomNumberGenerator.Create();

    private readonly IRegistrationModule? _registrationModule;

    private readonly byte[] _encryptionPrivKey;
    private readonly byte[] _encryptionPubKey;

    private byte[]? _consensusIoPubKey;

    public SecretEncryptionProvider(IRegistrationModule registrationModule, byte[]? encryptionSeed)
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

    public SecretEncryptionProvider(byte[] consensusIoPubKey, byte[]? encryptionSeed)
    {
        _consensusIoPubKey = consensusIoPubKey;
        if(encryptionSeed is not null && encryptionSeed.Length != 32)
        {
            throw new ArgumentException("EncryptionKey must be 32 bytes long.");
        }

        encryptionSeed ??= Aead.GenerateKey256();
        var keyPair = X25519.X25519KeyAgreement.GenerateKeyFromPrivateKey(encryptionSeed);
        _encryptionPrivKey = keyPair.PrivateKey;
        _encryptionPubKey = keyPair.PublicKey;
    }

    async ValueTask IInitializeableService.InitializeAsync(CancellationToken cancellationToken)
    {
        if(_registrationModule is not null)
        {
            var txKey = await _registrationModule.TxKeyAsync(cancellationToken: cancellationToken);
            _consensusIoPubKey = txKey.Key_.ToByteArray();
        }
    }

    public byte[] CalculateTxEncryptionKey(ReadOnlySpan<byte> nonce)
    {
        if(_consensusIoPubKey is null)
        {
            throw new InvalidOperationException("SecretMessageEncryptor has not been initialized!");
        }

        var txEncryptionIkm = new byte[64];

        var sharedKey = X25519KeyAgreement.Agreement(_encryptionPrivKey, _consensusIoPubKey);

        sharedKey.AsSpan(0, 32).CopyTo(txEncryptionIkm);
        nonce.CopyTo(txEncryptionIkm.AsSpan(32, 32));

        return HKDF.DeriveKey(HashAlgorithmName.SHA256, txEncryptionIkm, 32, _hkdfSalt);
    }

    public byte[] EncryptMessage(ReadOnlySpan<byte> message, out SecretEncryptionContext context, out SecretMessageDecryptor decryptor)
    {
        Span<byte> nonce = stackalloc byte[32];
        _random.GetBytes(nonce);
        return Encrypt(message, nonce, out context, out decryptor);
    }

    public byte[] Encrypt(ReadOnlySpan<byte> message, ReadOnlySpan<byte> nonce, out SecretEncryptionContext context, out SecretMessageDecryptor decryptor)
    {
        var txEncryptionKey = CalculateTxEncryptionKey(nonce);

        var aesSiv = AesSiv.CreateAesCmacSiv(txEncryptionKey);
        decryptor = new SecretMessageDecryptor(aesSiv);

        var cipherText = aesSiv.Seal(message.ToArray(), [[]]);
        var buffer = new byte[64 + cipherText.Length];

        nonce.CopyTo(buffer.AsSpan(0, 32));
        _encryptionPubKey.CopyTo(buffer.AsSpan(32, 32));
        cipherText.CopyTo(buffer.AsSpan(64, cipherText.Length));

        context = new SecretEncryptionContext(txEncryptionKey);
        return buffer;
    }
}
