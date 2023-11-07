using dotnetstandard_bip32;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Cosm.Net.Crypto;
public static partial class BIP32
{
    private const uint HardenedOffset = 2147483648u;

    [GeneratedRegex("^m(\\/[0-9]+')+$")]
    private static partial Regex DerivationPathRegex();

    public static (ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> ChainCode) GetMasterKeyFromSeed(ReadOnlySpan<byte> seed, string curve)
    {
        var hash = HMACSHA512.HashData(System.Text.Encoding.UTF8.GetBytes(curve), seed)
            .AsMemory();
        return (hash[..32], hash[32..]);
    }

    private static (ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> ChainCode) GetChildKeyDerivation(
        ReadOnlySpan<byte> key, ReadOnlySpan<byte> chainCode, uint index)
    {
        Span<byte> dataBuffer = stackalloc byte[key.Length + 5];
        key.CopyTo(dataBuffer[1..]);
        BitConverter.TryWriteBytes(dataBuffer[^4..], index);

        if (BitConverter.IsLittleEndian)
        {
            dataBuffer[^4..].Reverse();
        }

        var hash = HMACSHA512.HashData(chainCode, dataBuffer).AsMemory();
        return (hash[..32], hash[32..]);
    }
    private static bool IsValidPath(string path)
    {
        var regex = DerivationPathRegex();

        return regex.IsMatch(path)
            && !(from a in path.Split(new char[1] { '/' }).Slice(1)
                 select a.Replace("'", "")).Any((a) => !int.TryParse(a, out int _));
    }

    public static (ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> ChainCode) DerivePath(ReadOnlySpan<byte> seed, string path, string curve)
    {
        if(!IsValidPath(path))
        {
            throw new FormatException("Invalid derivation path");
        }

        var (key, chainCode) = GetMasterKeyFromSeed(seed, curve);

        var source = path.Split('/')
            .Skip(1)
            .Select(x => int.Parse(x.Replace("'", "")));

        foreach(var step in path.Split('/').Skip(1))
        {
            uint derivStep = uint.Parse(step.Replace("'", ""));

            (key, chainCode) = GetChildKeyDerivation(key.Span, chainCode.Span, derivStep + HardenedOffset);
        }

        return (key, chainCode);
    }
}
