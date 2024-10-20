using System.Text;

namespace Cosm.Net.Encoding;
public static class Bech32
{
    private const int ChecksumByteLength = 6;

    // used for polymod
    private static readonly uint[] Generator = [0x3b6a57b2, 0x26508e6d, 0x1ea119fa, 0x3d4233dd, 0x2a1462b3];

    // charset is the sequence of ascii characters that make up the bech32
    // alphabet.  Each character represents a 5-bit squashed byte.
    // q = 0b00000, p = 0b00001, z = 0b00010, and so on.
    private const string Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";

    private const string LowerCharacters = "abcdefghijklmnopqrstuvwxyz";
    private const string UpperCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // icharset is a mapping of 8-bit ascii characters to the charset positions. Both uppercase and lowercase ascii are mapped to the 5-bit position values.
    private static readonly byte[] icharset =
    [
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        15, 255, 10, 17, 21, 20, 26, 30, 7, 5, 255, 255, 255, 255, 255, 255,
        255, 29, 255, 24, 13, 25, 9, 8, 23, 255, 18, 22, 31, 27, 19, 255,
        1, 0, 3, 16, 11, 28, 12, 14, 6, 4, 2, 255, 255, 255, 255, 255,
        255, 29, 255, 24, 13, 25, 9, 8, 23, 255, 18, 22, 31, 27, 19, 255,
        1, 0, 3, 16, 11, 28, 12, 14, 6, 4, 2, 255, 255, 255, 255, 255
    ];

    // PolyMod takes a byte slice and returns the 32-bit BCH checksum.
    // Note that the input bytes to PolyMod need to be squashed to 5-bits tall
    // before being used in this function.  And this function will not error,
    // but instead return an unusable checksum, if you give it full-height bytes.
    private static uint PolyMod(ReadOnlySpan<byte> data)
    {
        uint chk = 1;
        foreach(byte value in data)
        {
            uint top = chk >> 25;
            chk = ((chk & 0x1ffffff) << 5) ^ value;
            for(int i = 0; i < 5; ++i)
            {
                if(((top >> i) & 1) == 1)
                {
                    chk ^= Generator[i];
                }
            }
        }

        return chk;
    }

    public static int TryDecodeAddress(ReadOnlySpan<char> address, Span<byte> dataOutput)
    {
        if(IsMixedCase(address))
        {
            //Invalid address format, must not be mixed case
            return -1;
        }

        int splitIndex = address.LastIndexOf('1');

        if(splitIndex == -1)
        {
            //Invalid address format, no prefix delimiter
            return -1;
        }
        if(address.Length - splitIndex < ChecksumByteLength + 6)
        {
            //Invalid address format, length after prefix too small
            return -1;
        }

        var prefix = address[..splitIndex];
        var payload = address[(splitIndex + 1)..];

        Span<byte> buffer = stackalloc byte[(prefix.Length * 2) + address.Length - splitIndex];

        HrpExpand(prefix, buffer); //Uses first (prefix.Length * 2) + 1 of buffer

        if(!TrySquashBase32Bytes(payload, buffer[((prefix.Length * 2) + 1)..]))
        {
            return -1;
        }
        if(!VerifyChecksum(buffer))
        {
            return -1;
        }
        //
        return ExpandBytes(buffer.Slice(
            (prefix.Length * 2) + 1,
            buffer.Length - (prefix.Length * 2) - 1 - ChecksumByteLength),
            dataOutput);
    }

    public static bool ValidateAddress(ReadOnlySpan<char> address, out ReadOnlySpan<char> prefix, out int length)
    {
        length = -1;
        prefix = null;

        if(IsMixedCase(address))
        {
            //Invalid address format, must not be mixed case
            return false;
        }
        int splitIndex = address.LastIndexOf('1');

        if(splitIndex == -1)
        {
            //Invalid address format, no prefix delimiter
            return false;
        }
        if(address.Length - splitIndex < ChecksumByteLength + 6)
        {
            //Invalid address format, length after prefix too small
            return false;
        }

        prefix = address[..splitIndex];
        var payload = address[(splitIndex + 1)..];

        Span<byte> buffer = stackalloc byte[(prefix.Length * 2) + address.Length - splitIndex];
        HrpExpand(prefix, buffer); //Uses first (prefix.Length * 2) + 1 of buffer

        if(!TrySquashBase32Bytes(payload, buffer[((prefix.Length * 2) + 1)..]))
        {
            prefix = null;
            return false;
        }
        if(!VerifyChecksum(buffer))
        {
            prefix = null;
            return false;
        }
        //
        int inputSize = buffer.Length - (prefix.Length * 2) - 1 - ChecksumByteLength;
        int outputSize = (inputSize * 5 / 8)
            + ((inputSize * 5 % 8) != 0 ? 1 : 0);

        length = outputSize;
        return true;
    }

    public static byte[] DecodeAddress(ReadOnlySpan<char> address)
    {
        if(IsMixedCase(address))
        {
            throw new InvalidOperationException("Decoding Address failed, mixed case detected");
        }

        int splitIndex = address.LastIndexOf('1');

        if(splitIndex == -1)
        {
            throw new InvalidOperationException("Decoding Address failed, not prefix found");
        }
        if(address.Length - splitIndex < ChecksumByteLength + 6)
        {
            throw new InvalidOperationException("Decoding Address failed, Address too short");
        }

        var prefix = address[..splitIndex];
        var payload = address[(splitIndex + 1)..];

        Span<byte> buffer = stackalloc byte[(prefix.Length * 2) + address.Length - splitIndex];

        HrpExpand(prefix, buffer); //Uses first (prefix.Length * 2) + 1 of buffer

        if(!TrySquashBase32Bytes(payload, buffer[((prefix.Length * 2) + 1)..]))
        {
            throw new InvalidOperationException("Decoding Address failed, Address contains invalid character");
        }
        if(!VerifyChecksum(buffer))
        {
            throw new InvalidOperationException("Decoding Address failed, Checksum check failed");
        }
        //

        var squashedInput = buffer.Slice(
            (prefix.Length * 2) + 1,
            buffer.Length - (prefix.Length * 2) - 1 - ChecksumByteLength);

        int outputSize = (squashedInput.Length * 5 / 8)
            + ((squashedInput.Length * 5 % 8) != 0 ? 1 : 0);
        byte[] dataOutput = new byte[outputSize];

        _ = ExpandBytes(squashedInput,
            dataOutput);

        return dataOutput;
    }

    public static bool TryEncodeAddress(string prefix, ReadOnlySpan<byte> data, out string? address)
    {
        int outputSize = (data.Length * 8 / 5)
            + ((data.Length * 8 % 5) != 0 ? 1 : 0);

        Span<byte> buffer = stackalloc byte[outputSize + 6];

        if(SquashBytes(data, buffer[..^6]) != buffer.Length - 6)
        {
            address = null;
            return false;
        }

        CreateChecksum(prefix, buffer[..^6], buffer[^6..]);

        var sb = new StringBuilder();

        _ = sb.Append(prefix);
        _ = sb.Append('1');

        for(int i = 0; i < buffer.Length; i++)
        {
            byte c = buffer[i];

            if((c & 0xe0) != 0)
            {
                throw new InvalidOperationException("Invalid address");
            }

            _ = sb.Append(Charset[c]);
        }

        address = sb.ToString();
        return true;
    }

    public static string EncodeAddress(string prefix, ReadOnlySpan<byte> data)
        => TryEncodeAddress(prefix, data, out string? address)
            ? address!
            : throw new InvalidOperationException("Failed to encode address");

    private static bool IsMixedCase(ReadOnlySpan<char> address) => address.IndexOfAny(LowerCharacters) != -1 &&
            address.IndexOfAny(UpperCharacters) != -1;

    //Both need the same length
    private static bool TrySquashBase32Bytes(ReadOnlySpan<char> data, Span<byte> squashedDataOutput)
    {
        for(int i = 0; i < data.Length; i++)
        {
            byte buffer = icharset[data[i]];
            if(buffer == 255)
            {
                //Invalid character
                return false;
            }

            squashedDataOutput[i] = buffer;
        }

        return true;
    }

    private static bool VerifyChecksum(ReadOnlySpan<byte> data)
    {
        uint checksum = PolyMod(data);
        return checksum == 1;
    }

    private static void HrpExpand(ReadOnlySpan<char> input, Span<byte> output)
    {
        for(int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            output[i] = (byte) (c >> 5);
        }

        for(int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            output[i + input.Length + 1] = (byte) (c & 0x1f);
        }
    }

    //squashes full-width (8-bit) bytes into "squashed" 5-bit bytes
    public static int SquashBytes(ReadOnlySpan<byte> input, Span<byte> output)
    {
        int outputSize = (input.Length * 8 / 5)
            + ((input.Length * 8 % 5) != 0 ? 1 : 0);

        if(output.Length != outputSize)
        {
            return -1;
        }

        int accumulator = 0;
        int bitsStashed = 0;
        int outputIndex = 0;

        for(int i = 0; i < input.Length; i++)
        {
            byte c = input[i];
            accumulator = (accumulator << 8) | c;

            if(bitsStashed >= 2)
            {
                output[outputIndex] = (byte) ((accumulator >> (bitsStashed + 3)) & 31);
                output[outputIndex + 1] = (byte) ((accumulator >> (bitsStashed - 2)) & 31);

                bitsStashed -= 2;
                outputIndex += 2;
            }
            else
            {
                output[outputIndex] = (byte) ((accumulator >> (bitsStashed + 3)) & 31);

                bitsStashed += 3;
                outputIndex += 1;
            }
        }

        if(bitsStashed != 0)
        {
            output[outputIndex] = (byte) ((accumulator << (5 - bitsStashed)) & 31);
        }

        return outputSize;
    }

    //expands "squashed" 5-bit bytes into full-width (8-bit) bytes
    private static int ExpandBytes(ReadOnlySpan<byte> input, Span<byte> output)
    {
        int outputSize = (input.Length * 5 / 8)
            + ((input.Length * 5 % 8) != 0 ? 1 : 0);

        if(output.Length < outputSize)
        {
            return -1;
        }

        int accumulator = 0;
        int bitsStashed = 0;
        int outputIndex = 0;

        for(int i = 0; i < input.Length; i++)
        {
            byte c = input[i];

            if(c > 31)
            {
                return -1;
            }

            accumulator = (accumulator << 5) | c;

            if(bitsStashed >= 3)
            {
                output[outputIndex] = (byte) (accumulator >> (bitsStashed - 3));

                bitsStashed -= 3;
                outputIndex += 1;
            }
            else
            {
                bitsStashed += 5;
            }
        }

        return bitsStashed >= 5 || ((accumulator << (8 - bitsStashed)) & 255) != 0
            ? -1
            : outputIndex;
    }

    private static void CreateChecksum(string prefix, ReadOnlySpan<byte> data, Span<byte> checksumOut)
    {
        Span<byte> buffer = stackalloc byte[(prefix.Length * 2) + 1 + data.Length + 6];

        HrpExpand(prefix, buffer); //Uses first (prefix.Length * 2) + 1 of buffer

        data.CopyTo(buffer[((prefix.Length * 2) + 1)..]);

        // flip the LSB of the checksum data after creating it
        uint checksum = PolyMod(buffer) ^ 1;

        for(int i = 0; i < 6; i++)
        {
            checksumOut[i] = (byte) ((checksum >> (5 * (5 - i))) & 0x1f);
        }
    }
}
