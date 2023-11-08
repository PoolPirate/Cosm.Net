using BenchmarkDotNet.Attributes;
using Cosm.Net.Crypto;
using dotnetstandard_bip39;
using Org.BouncyCastle.Utilities.Encoders;

namespace Cosm.Net.Bench;
[MemoryDiagnoser]

public class BIP32Bench
{
    private readonly string Mnemonic;
    private readonly byte[] MnemonicSeed;
    private readonly string CosmosDerivationPath;

    public BIP32Bench()
    {
        Mnemonic = "file crane delay shadow extend outdoor maximum click approve zebra alert ten";
        MnemonicSeed = Hex.Decode(new BIP39().MnemonicToSeedHex(Mnemonic, ""));
        CosmosDerivationPath = $"m/{44 + 2147483648u}'/{118 + 2147483648u}'/{0 + 2147483648u}'/0'/0'";
    }

    [Benchmark]
    public byte[] DerivePrivateKey_Cosmos()
    {
        var (key, _) = BIP32.DerivePath(MnemonicSeed, CosmosDerivationPath, BIP32Curves.Secp256k1);
        return key;
    }

    [Benchmark]
    public byte[] DerivePrivateKey_Master()
    {
        var (key, _) = BIP32.DeriveMasterKey(MnemonicSeed, BIP32Curves.Secp256k1);
        return key;
    }
}
