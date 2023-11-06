/* Copyright (c) 2017 Guillaume Bonnot and Palekhov Ilia
* Based on the work of Pieter Wuille
* Special Thanks to adiabat
*                  
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using System.Diagnostics;

namespace Cosm.Net;

public static class Bech32Encoder
{
    // used for polymod
    private static readonly uint[] Generator = { 0x3b6a57b2, 0x26508e6d, 0x1ea119fa, 0x3d4233dd, 0x2a1462b3 };

    // charset is the sequence of ascii characters that make up the bech32
    // alphabet.  Each character represents a 5-bit squashed byte.
    // q = 0b00000, p = 0b00001, z = 0b00010, and so on.

    private const string Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";

    // icharset is a mapping of 8-bit ascii characters to the charset positions. Both uppercase and lowercase ascii are mapped to the 5-bit position values.
    private static readonly short[] icharset =
    {
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        15, -1, 10, 17, 21, 20, 26, 30, 7, 5, -1, -1, -1, -1, -1, -1,
        -1, 29, -1, 24, 13, 25, 9, 8, 23, -1, 18, 22, 31, 27, 19, -1,
        1, 0, 3, 16, 11, 28, 12, 14, 6, 4, 2, -1, -1, -1, -1, -1,
        -1, 29, -1, 24, 13, 25, 9, 8, 23, -1, 18, 22, 31, 27, 19, -1,
        1, 0, 3, 16, 11, 28, 12, 14, 6, 4, 2, -1, -1, -1, -1, -1
    };

    // PolyMod takes a byte slice and returns the 32-bit BCH checksum.
    // Note that the input bytes to PolyMod need to be squashed to 5-bits tall
    // before being used in this function.  And this function will not error,
    // but instead return an unusable checksum, if you give it full-height bytes.
    public static uint PolyMod(byte[] values)
    {
        uint chk = 1;
        foreach(byte value in values)
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

    // on error, data == null
    public static void Decode(string encoded, out string? hrp, out byte[]? data)
    {
        DecodeSquashed(encoded, out hrp, out byte[]? squashed);
        if(squashed == null!)
        {
            data = null!;
            return;
        }

        data = Bytes5To8(squashed);
    }

    // on error, data == null
    private static void DecodeSquashed(string address, out string? hrp, out byte[]? data)
    {
        string? adr = CheckAndFormat(address);
        if(adr == null!)
        {
            data = null!;
            hrp = null!;
            return;
        }

        // find the last "1" and split there
        int splitLoc = adr.LastIndexOf("1", StringComparison.Ordinal);
        if(splitLoc == -1)
        {
            Debug.WriteLine("1 separator not present in address");
            data = null;
            hrp = null;
            return;
        }

        // hrp comes before the split
        hrp = adr[..splitLoc];

        // get squashed data
        byte[]? squashed = StringToSquashedBytes(adr[(splitLoc + 1)..]);
        if(squashed == null)
        {
            data = null;
            return;
        }

        // make sure checksum works
        if(!VerifyChecksum(hrp, squashed))
        {
            Debug.WriteLine("Checksum invalid");
            data = null;
            return;
        }

        // chop off checksum to return only payload
        int length = squashed.Length - 6;
        data = new byte[length];
        Array.Copy(squashed, 0, data, 0, length);
    }

    // on error, return null
    private static string? CheckAndFormat(string adr)
    {
        // make an all lowercase and all uppercase version of the input string
        string lowAdr = adr.ToLower();
        string highAdr = adr.ToUpper();

        // if there's mixed case, that's not OK
        if(adr != lowAdr && adr != highAdr)
        {
            Debug.WriteLine("mixed case address");
            return null;
        }

        // default to lowercase
        return lowAdr;
    }

    private static bool VerifyChecksum(string hrp, byte[] data)
    {
        byte[] values = HrpExpand(hrp).Concat(data).ToArray();
        uint checksum = PolyMod(values);
        // make sure it's 1 (from the LSB flip in CreateChecksum
        return checksum == 1;
    }

    // on error, return null
    private static byte[]? StringToSquashedBytes(string input)
    {
        byte[] squashed = new byte[input.Length];

        for(int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            short buffer = icharset[c];
            if(buffer == -1)
            {
                Debug.WriteLine("contains invalid character " + c);
                return null;
            }

            squashed[i] = (byte) buffer;
        }

        return squashed;
    }

    // we encode the data and the human readable prefix
    public static string? Encode(string hrp, byte[] data)
    {
        byte[]? base5 = Bytes8To5(data);
        return base5 == null ? String.Empty : EncodeSquashed(hrp, base5);
    }

    // on error, return null
    private static string? EncodeSquashed(string hrp, byte[] data)
    {
        byte[] checksum = CreateChecksum(hrp, data);
        byte[] combined = data.Concat(checksum).ToArray();

        // Should be squashed, return empty string if it's not.
        string? encoded = SquashedBytesToString(combined);
        return encoded == null ? null : hrp + "1" + encoded;
    }

    public static byte[] CreateChecksum(string hrp, byte[] data)
    {
        byte[] values = HrpExpand(hrp).Concat(data).ToArray();
        // put 6 zero bytes on at the end
        values = values.Concat(new byte[6]).ToArray();
        //get checksum for whole slice

        // flip the LSB of the checksum data after creating it
        uint checksum = PolyMod(values) ^ 1;

        byte[] ret = new byte[6];
        for(int i = 0; i < 6; i++)
        {
            // note that this is NOT the same as converting 8 to 5
            // this is it's own expansion to 6 bytes from 4, chopping
            // off the MSBs.
            ret[i] = (byte) ((checksum >> (5 * (5 - i))) & 0x1f);
        }

        return ret;
    }

    // HRPExpand turns the human readable part into 5bit-bytes for later processing
    private static byte[] HrpExpand(string input)
    {
        byte[] output = new byte[(input.Length * 2) + 1];

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

    private static string? SquashedBytesToString(byte[] input)
    {
        string s = String.Empty;
        for(int i = 0; i < input.Length; i++)
        {
            byte c = input[i];
            if((c & 0xe0) != 0)
            {
                Debug.WriteLine("high bits set at position {0}: {1}", i, c);
                return null;
            }

            s += Charset[c];
        }

        return s;
    }

    public static byte[]? Bytes8To5(byte[] data) => ByteSquasher(data, 8, 5);

    public static byte[]? Bytes5To8(byte[] data) => ByteSquasher(data, 5, 8);

    // ByteSquasher squashes full-width (8-bit) bytes into "squashed" 5-bit bytes,
    // and vice versa. It can operate on other widths but in this package only
    // goes 5 to 8 and back again. It can return null if the squashed input
    // you give it isn't actually squashed, or if there is padding (trailing q characters)
    // when going from 5 to 8
    private static byte[]? ByteSquasher(byte[] input, int inputWidth, int outputWidth)
    {
        int bitStash = 0;
        int accumulator = 0;
        var output = new List<byte>();
        int maxOutputValue = (1 << outputWidth) - 1;

        for(int i = 0; i < input.Length; i++)
        {
            byte c = input[i];
            if(c >> inputWidth != 0)
            {
                Debug.WriteLine("byte {0} ({1}) high bits set", i, c);
                return null;
            }

            accumulator = (accumulator << inputWidth) | c;
            bitStash += inputWidth;
            while(bitStash >= outputWidth)
            {
                bitStash -= outputWidth;
                output.Add((byte) ((accumulator >> bitStash) & maxOutputValue));
            }
        }

        // pad if going from 8 to 5
        if(inputWidth == 8 && outputWidth == 5)
        {
            if(bitStash != 0)
            {
                output.Add((byte) ((accumulator << (outputWidth - bitStash)) & maxOutputValue));
            }
        }
        else if(bitStash >= inputWidth || ((accumulator << (outputWidth - bitStash)) & maxOutputValue) != 0)
        {
            // no pad from 5 to 8 allowed
            Debug.WriteLine("invalid padding from {0} to {1} bits", inputWidth, outputWidth);
            return null;
        }

        return output.ToArray();
    }
}