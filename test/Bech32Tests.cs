using Cosm.Net.Encoding;

namespace Cosm.Net.Test;

public class Bech32Tests
{
    private const string ValidUserAddress = "osmo1hku9an4jd2ut8amcrxwwn8yw8fzy0cygls56sy";
    private static readonly byte[] ValidUserAddressBytes = Convert.FromBase64String("vbhezrJquLP3eBmc6ZyOOkRH4Ig=");

    private static readonly string?[] InvalidAddresses = [
        "osmo1hku9an4jd2ut8amcrxwwn8yw8fzy0cygls56s6", //Invalid Checksum
        "osmo1hku9an4jd2ut8amcrxwwn8yw8fzy0cygls56s67", //Added Character
        "1hku9an4jd2ut8amcrxwwn8yw8fzy0cygls56s6", //Empty Prefix
        "hku9an4jd2ut8amcrxwwn8yw8fzy0cygls56s6", //No Prefix
        "aoshgrnioasehnjgj94838t34oiegn", //Random Junk
        "",
        null
    ];

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
    public void TryDecodeAddress_Returns_False_On_InvalidAddress()
    {
        byte[] output = new byte[ValidUserAddressBytes.Length];
        foreach(var address in InvalidAddresses)
        {
            Assert.Equal(-1, Bech32.TryDecodeAddress(address, output));
        }
    }

    [Fact]
    public void ValidateAddress_Returns_True_On_Valid_Address()
    {
        Assert.True(Bech32.ValidateAddress(ValidUserAddress, out var prefix, out var length));
        Assert.Equal("osmo", prefix.ToString());
        Assert.Equal(ValidUserAddressBytes.Length, length);
    }

    [Fact]
    public void ValidateAddress_Returns_False_On_Invalid_Address()
    {
        foreach(var address in InvalidAddresses)
        {
            Assert.False(Bech32.ValidateAddress(address, out var prefix, out var length));
            Assert.Equal("", prefix.ToString());
            Assert.Equal(-1, length);
        }
    }

    [Fact]
    public void TryEncodeAddress_Returns_Address_On_Valid_Bytes()
    {
        string address = Bech32.EncodeAddress("osmo", ValidUserAddressBytes);
        Assert.Equal(ValidUserAddress, address);
    }
}