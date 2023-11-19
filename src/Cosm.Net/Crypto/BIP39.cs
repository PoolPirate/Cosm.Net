using System.Security.Cryptography;
using System.Text;

namespace Cosm.Net.Crypto;
public static class BIP39
{
    public static byte[] MnemonicToSeed(string mnemonic, string password = "")
    {
        string normalizedMnemonic = mnemonic.Normalize(NormalizationForm.FormKD);
        string normalizedSaltedPassword = $"mnemonic{password.Normalize(NormalizationForm.FormKD)}";

        int passwordSize = System.Text.Encoding.UTF8.GetByteCount(normalizedMnemonic);
        int saltBufferSize = System.Text.Encoding.UTF8.GetByteCount(normalizedSaltedPassword);

        Span<byte> passwordBuffer = stackalloc byte[passwordSize];
        Span<byte> saltBuffer = stackalloc byte[saltBufferSize];

        System.Text.Encoding.UTF8.GetBytes(normalizedMnemonic, passwordBuffer);
        System.Text.Encoding.UTF8.GetBytes(normalizedSaltedPassword, saltBuffer);

        return Rfc2898DeriveBytes.Pbkdf2(passwordBuffer, saltBuffer, 2048, HashAlgorithmName.SHA512, 64);
    }
}