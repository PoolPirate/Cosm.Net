using Cosm.Net.Crypto;
using dotnetstandard_bip39;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosm.Net.Test;
public class BIP32Tests
{
    private static readonly string Mnemonic = "file crane delay shadow extend outdoor maximum click approve zebra alert ten";
    private static readonly byte[] PrivateKey = Convert.FromBase64String("WrPycofTq1WW79pZJT8w2ppOf/ocStcaGjrAT1yQPco=");
    private static readonly byte[] MnemonicSeed = Hex.Decode(new BIP39().MnemonicToSeedHex(Mnemonic, ""));

    private static readonly string CosmosDerivationPath = $"m/{44 + 2147483648u}'/{118 + 2147483648u}'/{0 + 2147483648u}'/0'/0'";

    [Fact]
    public void DerivePath_Returns_Correct_PrivateKey_Secp256k1()
    {
        var (key, _) = BIP32.DerivePath(MnemonicSeed, CosmosDerivationPath, BIP32Curves.Secp256k1);

        for(int i = 0; i < PrivateKey.Length; i++)
        {
            byte expected = PrivateKey[i];
            byte actual = key[i];

            Assert.True(expected == actual,
                $"Expected: '{expected}', Actual: '{actual}' at offset {i}."
            );
        }
    }
}
