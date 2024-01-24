using Cosm.Net.Wallet.Crypto;
using Org.BouncyCastle.Asn1.Sec;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Cosm.Net.Crypto;
public static partial class BIP32
{
    private const uint HardenedOffset = 2147483648u;
    private const string Secp256k1NHex = "0FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141";
    private static readonly BigInteger Secp256k1N = BigInteger.Parse(Secp256k1NHex, style: NumberStyles.AllowHexSpecifier);

    [GeneratedRegex("^m(\\/[0-9]+')+$")]
    private static partial Regex DerivationPathRegex();

    private static void GetMasterKeyFromSeed(ReadOnlySpan<byte> seed, string curve,
        Span<byte> keyOutput, Span<byte> chainCodeOutput)
    {
        Span<byte> buffer = stackalloc byte[64];

        _ = HMACSHA512.HashData(System.Text.Encoding.ASCII.GetBytes(curve), seed, buffer);

        var il = buffer[..32];
        var ir = buffer[32..];

        var ilNum = new BigInteger(il, isUnsigned: true, isBigEndian: true);

        if(curve != BIP32Curves.ED25519 && (IsZeroPrivateKey(il) || IsGteNPrivateKey(ilNum, curve)))
        {
            GetMasterKeyFromSeed(buffer, curve, il, ir);
        }

        il.CopyTo(keyOutput);
        ir.CopyTo(chainCodeOutput);
    }

    private static bool IsZeroPrivateKey(ReadOnlySpan<byte> privateKey)
        => privateKey.IndexOfAnyExcept((byte) 0) == -1;

    private static bool IsGteNPrivateKey(BigInteger privateKey, string curve)
        => privateKey >= N(curve);

    private static BigInteger N(string curve)
        => curve switch
        {
            BIP32Curves.Secp256k1 => Secp256k1N,
            _ => throw new InvalidOperationException("Curve not supported")
        };

    private static void GetChildKeyDerivation(
        Span<byte> currentKey, Span<byte> currentChainCode, uint index, string curve)
    {
        Span<byte> dataBuffer = stackalloc byte[currentKey.Length + 5];

        if(index < HardenedOffset)
        {
            if(curve == BIP32Curves.ED25519)
            {
                throw new InvalidOperationException("Curve not supported");
            }

            byte[] sp = SerializedPoint(curve, currentKey);
            sp.CopyTo(dataBuffer);
        }
        else
        {
            currentKey.CopyTo(dataBuffer[1..]);
        }

        _ = BitConverter.TryWriteBytes(dataBuffer[^4..], index);
        if(BitConverter.IsLittleEndian)
        {
            dataBuffer[^4..].Reverse();
        }

        Span<byte> digest = stackalloc byte[64];

        while(true)
        {
            _ = HMACSHA512.HashData(currentChainCode, dataBuffer, digest);

            var il = digest[..32];
            var ir = digest[32..];

            if(curve == BIP32Curves.ED25519)
            {
                il.CopyTo(currentKey);
                ir.CopyTo(currentChainCode);
                return;
            }

            var ilNum = new BigInteger(il, isUnsigned: true, isBigEndian: true);
            var returnChildKeyNr = (ilNum + new BigInteger(currentKey, isUnsigned: true, isBigEndian: true)) % N(curve);

            var returnChildKey = il; //Do Not Access il after this
            _ = returnChildKeyNr.TryWriteBytes(returnChildKey, out _, isUnsigned: true, isBigEndian: true);

            if(IsGteNPrivateKey(ilNum, curve) || IsZeroPrivateKey(returnChildKey))
            {
                dataBuffer[0] = 1;
                ir.CopyTo(dataBuffer[1..]);
            }
            else
            {
                returnChildKey.CopyTo(currentKey);
                ir.CopyTo(currentChainCode);
                return;
            }
        }
    }
    private static bool IsValidPath(string path)
    {
        var regex = DerivationPathRegex();

        return regex.IsMatch(path)
            && path.Split('/').Skip(1)
                .All(x => UInt32.TryParse(x.Replace("'", ""), out _));
    }

    private static byte[] SerializedPoint(string curve, ReadOnlySpan<byte> p)
    {
        if(curve != BIP32Curves.Secp256k1)
        {
            throw new InvalidOperationException("Curve not supported");
        }

        var p_i = new Org.BouncyCastle.Math.BigInteger(1, p.ToArray());
        return SecNamedCurves.GetByName("secp256k1").G
            .Multiply(p_i)
            .GetEncoded(true);
    }

    public static (byte[] Key, byte[] ChainCode) DerivePath(ReadOnlySpan<byte> seed, string path, string curve)
    {
        if(!IsValidPath(path))
        {
            throw new FormatException("Invalid derivation path");
        }

        byte[] currentKey = new byte[32];
        byte[] currentChainCode = new byte[32];

        GetMasterKeyFromSeed(seed, curve, currentKey, currentChainCode);

        var source = path.Split('/')
            .Skip(1)
            .Select(x => Int32.Parse(x.Replace("'", "")));

        foreach(string? step in path.Split('/').Skip(1))
        {
            uint derivStep = UInt32.Parse(step.Replace("'", ""));

            GetChildKeyDerivation(currentKey.AsSpan(), currentChainCode.AsSpan(), derivStep, curve);
        }

        return (currentKey, currentChainCode);
    }

    public static (byte[] Key, byte[] ChainCode) DeriveMasterKey(ReadOnlySpan<byte> seed, string curve)
    {
        byte[] key = new byte[32];
        byte[] chainCode = new byte[32];
        GetMasterKeyFromSeed(seed, curve, key, chainCode);

        return (key, chainCode);
    }
}
