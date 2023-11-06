namespace Cosm.Net.Encoding;
public static class Bech32
{
    private const int ChecksumByteLength = 6;

    // used for polymod
    private static readonly uint[] Generator = { 0x3b6a57b2, 0x26508e6d, 0x1ea119fa, 0x3d4233dd, 0x2a1462b3 };

    // charset is the sequence of ascii characters that make up the bech32
    // alphabet.  Each character represents a 5-bit squashed byte.
    // q = 0b00000, p = 0b00001, z = 0b00010, and so on.
    private const string Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";

    private const string LowerCharacters = "abcdefghijklmnopqrstuvwxyz";
    private const string UpperCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // icharset is a mapping of 8-bit ascii characters to the charset positions. Both uppercase and lowercase ascii are mapped to the 5-bit position values.
    private static readonly byte[] icharset =
    {
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        15, 255, 10, 17, 21, 20, 26, 30, 7, 5, 255, 255, 255, 255, 255, 255,
        255, 29, 255, 24, 13, 25, 9, 8, 23, 255, 18, 22, 31, 27, 19, 255,
        1, 0, 3, 16, 11, 28, 12, 14, 6, 4, 2, 255, 255, 255, 255, 255,
        255, 29, 255, 24, 13, 25, 9, 8, 23, 255, 18, 22, 31, 27, 19, 255,
        1, 0, 3, 16, 11, 28, 12, 14, 6, 4, 2, 255, 255, 255, 255, 255
    };

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

    public static bool TryDecodeAddress(ReadOnlySpan<char> address, Span<byte> dataOutput)
    {
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

        var prefix = address[..splitIndex];
        var payload = address[(splitIndex + 1)..];

        Span<byte> buffer = stackalloc byte[(prefix.Length * 2) + address.Length - splitIndex];

        _ = HrpExpand(prefix, buffer); //Uses first (prefix.Length * 2) + 1 of buffer

        if(!TrySquashBase32Bytes(payload, buffer[((prefix.Length * 2) + 1)..]))
        {
            return false;
        }
        if(!VerifyChecksum(buffer))
        {
            return false;
        }
        //
        return ByteSquasher(buffer.Slice(
            (prefix.Length * 2) + 1,
            buffer.Length - (prefix.Length * 2) - 1 - ChecksumByteLength),
            dataOutput, 5, 8);
    }

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

    private static Span<byte> HrpExpand(ReadOnlySpan<char> input, Span<byte> output)
    {
        // first half is the input string shifted down 5 bits.
        // not much is going on there in terms of data / entropy
        for(int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            output[i] = (byte) (c >> 5);
        }

        // then there's a 0 byte separator
        // don't need to set 0 byte in the middle, as it starts out that way

        // second half is the input string, with the top 3 bits zeroed.
        // most of the data / entropy will live here.
        for(int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            output[i + input.Length + 1] = (byte) (c & 0x1f);
        }

        return output;
    }

    // ByteSquasher squashes full-width (8-bit) bytes into "squashed" 5-bit bytes,
    // and vice versa. It can operate on other widths but in this package only
    // goes 5 to 8 and back again. It can return null if the squashed input
    // you give it isn't actually squashed, or if there is padding (trailing q characters)
    // when going from 5 to 8
    private static bool ByteSquasher(Span<byte> input, Span<byte> output, int inputWidth, int outputWidth)
    {
        int bitStash = 0;
        int accumulator = 0;
        int maxOutputValue = (1 << outputWidth) - 1;

        int outputIndex = 0;

        for(int i = 0; i < input.Length; i++)
        {
            byte c = input[i];
            if(c >> inputWidth != 0)
            {
                return false;
            }

            accumulator = (accumulator << inputWidth) | c;
            bitStash += inputWidth;
            for(; bitStash >= outputWidth; outputIndex++)
            {
                if(outputIndex >= output.Length)
                {
                    return false;
                }

                bitStash -= outputWidth;
                output[outputIndex] = (byte) ((accumulator >> bitStash) & maxOutputValue);
            }
        }

        // pad if going from 8 to 5
        if(inputWidth == 8 && outputWidth == 5)
        {
            if(outputIndex >= output.Length)
            {
                return false;
            }

            if(bitStash != 0)
            {
                output[outputIndex++] = (byte) ((accumulator << (outputWidth - bitStash)) & maxOutputValue);
            }
        }
        else if(bitStash >= inputWidth || ((accumulator << (outputWidth - bitStash)) & maxOutputValue) != 0)
        {
            return false;
        }

        return true;
    }

    //// we encode the data and the human readable prefix
    //public static string? Encode(string hrp, byte[] data)
    //{
    //    var base5 = Bytes8To5(data);
    //    return base5 == null ? string.Empty : EncodeSquashed(hrp, base5);
    //}

    //// on error, return null
    //private static string? EncodeSquashed(string hrp, byte[] data)
    //{
    //    var checksum = CreateChecksum(hrp, data);
    //    var combined = data.Concat(checksum).ToArray();

    //    // Should be squashed, return empty string if it's not.
    //    var encoded = SquashedBytesToString(combined);
    //    return encoded == null ? null : hrp + "1" + encoded;
    //}

    //private static byte[] CreateChecksum(string hrp, byte[] data)
    //{
    //    var values = HrpExpand(hrp).Concat(data).ToArray();
    //    // put 6 zero bytes on at the end
    //    values = values.Concat(new byte[6]).ToArray();
    //    //get checksum for whole slice

    //    // flip the LSB of the checksum data after creating it
    //    var checksum = PolyMod(values) ^ 1;

    //    var ret = new byte[6];
    //    for (var i = 0; i < 6; i++)
    //    {
    //        // note that this is NOT the same as converting 8 to 5
    //        // this is it's own expansion to 6 bytes from 4, chopping
    //        // off the MSBs.
    //        ret[i] = (byte)(checksum >> (5 * (5 - i)) & 0x1f);
    //    }

    //    return ret;
    //}

    //private static string? SquashedBytesToString(byte[] input)
    //{
    //    var s = string.Empty;
    //    for (var i = 0; i < input.Length; i++)
    //    {
    //        var c = input[i];
    //        if ((c & 0xe0) != 0)
    //        {
    //            Debug.WriteLine("high bits set at position {0}: {1}", i, c);
    //            return null;
    //        }

    //        s += Charset[c];
    //    }

    //    return s;
    //}

    //private static byte[]? Bytes8To5(byte[] data) => ByteSquasher(data, 8, 5);

    //private static byte[]? Bytes5To8(byte[] data) => ByteSquasher(data, 5, 8);

    //// ByteSquasher squashes full-width (8-bit) bytes into "squashed" 5-bit bytes,
    //// and vice versa. It can operate on other widths but in this package only
    //// goes 5 to 8 and back again. It can return null if the squashed input
    //// you give it isn't actually squashed, or if there is padding (trailing q characters)
    //// when going from 5 to 8
    //private static byte[]? ByteSquasher(byte[] input, int inputWidth, int outputWidth)
    //{
    //    var bitStash = 0;
    //    var accumulator = 0;
    //    var output = new List<byte>();
    //    var maxOutputValue = (1 << outputWidth) - 1;

    //    for (var i = 0; i < input.Length; i++)
    //    {
    //        var c = input[i];
    //        if (c >> inputWidth != 0)
    //        {
    //            Debug.WriteLine("byte {0} ({1}) high bits set", i, c);
    //            return null;
    //        }

    //        accumulator = (accumulator << inputWidth) | c;
    //        bitStash += inputWidth;
    //        while (bitStash >= outputWidth)
    //        {
    //            bitStash -= outputWidth;
    //            output.Add((byte)((accumulator >> bitStash) & maxOutputValue));
    //        }
    //    }

    //    // pad if going from 8 to 5
    //    if (inputWidth == 8 && outputWidth == 5)
    //    {
    //        if (bitStash != 0)
    //            output.Add((byte)(accumulator << (outputWidth - bitStash) & maxOutputValue));
    //    }
    //    else if (bitStash >= inputWidth || ((accumulator << (outputWidth - bitStash)) & maxOutputValue) != 0)
    //    {
    //        // no pad from 5 to 8 allowed
    //        Debug.WriteLine("invalid padding from {0} to {1} bits", inputWidth, outputWidth);
    //        return null;
    //    }

    //    return output.ToArray();
    //}
}
