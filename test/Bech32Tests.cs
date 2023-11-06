using Cosm.Net.Encoding;

namespace Cosm.Net.Test;

public class Bech32Tests
{
    private const string ValidUserAddress = "osmo1hku9an4jd2ut8amcrxwwn8yw8fzy0cygls56sy";
    private static readonly byte[] ValidUserAddressBytes = Convert.FromBase64String("vbhezrJquLP3eBmc6ZyOOkRH4Ig=");

    private const string InvalidAddressBadChecksum = "osmo1hku9an4jd2ut8amcrxwwn8yw8fzy0cygls56s6";
    private const string InvalidAddressNoPrefix = "hku9an4jd2ut8amcrxwwn8yw8fzy0cygls56s6";

    [Fact]
    public void TryDecodeAddress_Returns_True_On_Valid_Address()
    {
        byte[] output = new byte[ValidUserAddressBytes.Length];

        Assert.True(Bech32.TryDecodeAddress(ValidUserAddress, output) > 0);

        for(int i = 0; i < ValidUserAddressBytes.Length; i++)
        {
            byte expected = ValidUserAddressBytes[i];
            byte actual = output[i];

            Assert.True(expected == actual,
                $"Expected: '{expected}', Actual: '{actual}' at offset {i}."
            );
        }
    }

    [Fact]
    public void TryDecodeAddress_Returns_False_On_Valid_BadChecksum()
    {
        byte[] output = new byte[ValidUserAddressBytes.Length];
        Assert.False(Bech32.TryDecodeAddress(InvalidAddressBadChecksum, output) > 0);
    }

    [Fact]
    public void TryDecodeAddress_Returns_False_On_Valid_NoPrefix()
    {
        byte[] output = new byte[ValidUserAddressBytes.Length];
        Assert.False(Bech32.TryDecodeAddress(InvalidAddressNoPrefix, output) > 0);
    }

    [Fact]
    public void TryEncodeAddress_Returns_Address_On_Valid_Bytes()
    {
        string address = Bech32.EncodeAddress("osmo", ValidUserAddressBytes);
        Assert.Equal(ValidUserAddress, address);
    }
}