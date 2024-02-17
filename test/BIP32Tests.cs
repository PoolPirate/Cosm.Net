using Cosm.Net.Crypto;
using Cosm.Net.Wallet.Crypto;
using Org.BouncyCastle.Utilities.Encoders;

namespace Cosm.Net.Test;
public class BIP32Tests
{
    private static readonly string _mnemonic = "file crane delay shadow extend outdoor maximum click approve zebra alert ten";
    private static readonly byte[] _privateKey = Convert.FromBase64String("WrPycofTq1WW79pZJT8w2ppOf/ocStcaGjrAT1yQPco=");
    private static readonly byte[] _mnemonicSeed = BIP39.MnemonicToSeed(_mnemonic, "");

    private static readonly string _cosmosDerivationPath = $"m/{44 + 2147483648u}'/{118 + 2147483648u}'/{0 + 2147483648u}'/0'/0'";

    [Fact]
    public void DerivePath_Returns_Correct_PrivateKey_Secp256k1()
    {
        var (key, _) = BIP32.DerivePath(_mnemonicSeed, _cosmosDerivationPath, BIP32Curves.Secp256k1);

        for(int i = 0; i < _privateKey.Length; i++)
        {
            byte expected = _privateKey[i];
            byte actual = key[i];

            Assert.True(expected == actual,
                $"Expected: '{expected}', Actual: '{actual}' at offset {i}."
            );
        }
    }
}
